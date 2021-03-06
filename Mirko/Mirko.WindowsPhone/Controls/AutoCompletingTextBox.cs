﻿using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Mirko.Controls
{
    public class AutoCompletingTextBox : TextBox
    {
        private bool HashtagDetected = false;
        private bool AtDetected = false; // '@'
        private Popup SuggestionsPopup = null;
        private static CacheViewModel Cache = null;
        
        public bool AreSuggestionsOpen
        {
            get { return (bool)GetValue(AreSuggestionsOpenProperty); }
            set { SetValue(AreSuggestionsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AreSuggestionsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AreSuggestionsOpenProperty =
            DependencyProperty.Register("AreSuggestionsOpen", typeof(bool), typeof(AutoCompletingTextBox), new PropertyMetadata(false));               

        public AutoCompletingTextBox()
        {
            if (Cache == null)
            {
                Cache = SimpleIoc.Default.GetInstance<CacheViewModel>();
                if (Cache.PopularHashtags.Count == 0)
                    Cache.GetPopularHashtags();
                if (Cache.ObservedUsers.Count == 0)
                    Cache.GetObservedUsers();
            }

            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += (s, args) =>
            {
                if (SuggestionsPopup != null && SuggestionsPopup.VerticalOffset == 0)
                {
                    var screen = Window.Current.Bounds;
                    var app = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds;

                    var appBarHeight = screen.Bottom - app.Bottom;
                    var statusBarHeight = app.Top - screen.Top;

                    SuggestionsPopup.VerticalOffset = screen.Height - args.OccludedRect.Height - statusBarHeight - appBarHeight;
                }
            };

            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += (s, args) =>
            {
                if (SuggestionsPopup != null && SuggestionsPopup.IsOpen)
                {
                    SuggestionsPopup.IsOpen = false;
                    AreSuggestionsOpen = false;
                }
            };

            base.Loaded += (s,e) =>
            {
                var navService = SimpleIoc.Default.GetInstance<NavigationService>();

                SuggestionsPopup = navService.GetPopup();

                SuggestionsPopup.Width = Window.Current.Bounds.Width;
                var content = SuggestionsPopup.Child as SuggestionsPopupContent;
                content.Width = Window.Current.Bounds.Width;

                var border = content.Content as Border;
                var lv = border.Child as ListView;
                lv.ItemClick += (se, args) =>
                {
                    var hashtag = args.ClickedItem as string;
                    ReplaceWordAtPointer(hashtag);
                };
                lv.SetBinding(ListView.ItemsSourceProperty, new Binding()
                {
                    Source = Cache,
                    Path = new PropertyPath("Suggestions"),
                });
            };

            base.TextChanged += AutoCompletingTextBox_TextChanged;
        }

        private void AutoCompletingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // extract word around SelectionStart
            string currentWord = GetWordAtPointer();

            Debug.WriteLine("currentWord: " + currentWord);

            if(currentWord.StartsWith("#") || currentWord.StartsWith("@"))
            {
                if (currentWord.StartsWith("#"))
                    HashtagDetected = true;
                else
                    AtDetected = true;

                Cache.GenerateSuggestions(currentWord, HashtagDetected);

                SuggestionsPopup.IsOpen = true;
                AreSuggestionsOpen = true;

                this.IsEnabled = false;
                this.IsTextPredictionEnabled = false;               
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
            else if(currentWord.StartsWith("") && (HashtagDetected || AtDetected))
            {
                SuggestionsPopup.IsOpen = false;
                AreSuggestionsOpen = false;
                HashtagDetected = false;
                AtDetected = false;

                this.IsEnabled = false;
                this.IsTextPredictionEnabled = true;
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
        }

        private bool CharPredicate(char c)
        {
            bool isSeparator = char.IsSeparator(c) || c == '\r' || c == '\n';
            bool isPunctuation = c == '#' || c == '@' || !char.IsPunctuation(c);

            return !isSeparator && isPunctuation;
        }

        private string GetWordCharactersBefore()
        {
            var start = this.GetNormalizedSelectionStart();
            var backwards = Text.Substring(0, start);
            var wordCharactersBeforePointer = new string(backwards.Reverse().TakeWhile(c => CharPredicate(c)).Reverse().ToArray());

            return wordCharactersBeforePointer;
        }

        private string GetWordCharactersAfter()
        {
            var start = this.GetNormalizedSelectionStart();
            var fowards = Text.Substring(start, Text.Length - start);
            var wordCharactersAfterPointer = new string(fowards.TakeWhile(c => CharPredicate(c)).ToArray());

            return wordCharactersAfterPointer;
        }

        private string GetWordAtPointer()
        {
            return string.Join(string.Empty, GetWordCharactersBefore(), GetWordCharactersAfter());
        }
        
        private void ReplaceWordAtPointer(string replacementWord)
        {
            var newText = this.Text;
            var before = GetWordCharactersBefore();
            var after = GetWordCharactersAfter();
            var start = this.GetNormalizedSelectionStart();
            var idx = start - before.Length;

            // first, we have to remove current word.
            newText = newText.Remove(idx, before.Length);
            newText = newText.Remove(idx, after.Length);

            newText = newText.Insert(idx, replacementWord);

            try
            {
                this.Text = newText;
                this.SelectionStart = idx + before.Length + replacementWord.Length - after.Length;
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
            }
        }
    }
}
