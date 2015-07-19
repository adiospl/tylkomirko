﻿using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using QKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class NewEntryPage : UserControl, IHaveAppBar
    {
        private BindingExpression EntryPreviewBinding = null;

        public NewEntryPage()
        {
            this.InitializeComponent();

            this.LayoutRoot.Loaded += (s, e) => FormattingPopup.IsOpen = true;
        }

        private TextBox CurrentEditor()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("Editor") as TextBox;
        }

        private ScrollViewer CurrentEntryPreview()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("EntryPreview") as ScrollViewer;
        }

        private TextBlock CurrentAttachmentSymbol()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("AttachmentSymbol") as TextBlock;
        }

        private Rectangle CurrentFooter()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("Footer") as Rectangle;
        }

        private void ContentRoot_LayoutChangeCompleted(object sender, LayoutChangeEventArgs e)
        {
            if (e.IsDefaultLayout)
            {
                PageTitle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CurrentFooter().Height = 100;

                if (EntryPreviewBinding != null)
                    CurrentEntryPreview().SetBinding(ScrollViewer.VisibilityProperty, EntryPreviewBinding.ParentBinding);
                
            }
            else
            {
                PageTitle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CurrentFooter().Height = 50;

                if(EntryPreviewBinding == null)
                    EntryPreviewBinding = CurrentEntryPreview().GetBindingExpression(ScrollViewer.VisibilityProperty);
                CurrentEntryPreview().Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void HandleSendButton()
        {
            string txt = CurrentEditor().Text;
            var vm = this.DataContext as NewEntryViewModel;
            var attachmentName = vm.Data.AttachmentName;

            if (txt.Length > 0 || !string.IsNullOrEmpty(attachmentName))
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        private void AttachmentSymbol_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = this.Resources["DeleteAttachmentFlyout"] as MenuFlyout;
            mf.ShowAt(CurrentAttachmentSymbol());
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as NewEntryViewModel;
            vm.RemoveAttachment();
            HandleSendButton();
        }

        #region AppBar
        private CommandBar AppBar = null;
        private AppBarButton SendButton = null;
        private AppBarToggleButton FormattingButton = null;

        public CommandBar CreateCommandBar()
        {
            SendButton = new AppBarButton()
            {
                Label = "wyślij",
                Icon = new SymbolIcon(Symbol.Send),
                IsEnabled = false,
            };
            SendButton.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("SendMessageCommand"),
            });

            var lenny = new AppBarButton()
            {
                Label = "lenny",
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/appbar.smiley.glasses.png") },
            };
            lenny.Click += async (s, e) =>
            {
                var editor = CurrentEditor();
                if (editor.FocusState == FocusState.Pointer || editor.FocusState == FocusState.Programmatic)
                    editor.Focus(FocusState.Unfocused);

                var f = this.Resources["LennysFlyout"] as Flyout;

                await Task.Delay(200);

                f.ShowAt(this);
            };

            var attachment = new AppBarButton()
            {
                Label = "załącznik",
                Icon = new SymbolIcon(Symbol.Attach),
            };
            attachment.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("AddAttachment"),
            });

            FormattingButton = new AppBarToggleButton()
            {
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };
            FormattingButton.Click += FormattingButton_Click;

            AppBar = new CommandBar();
            AppBar.PrimaryCommands.Add(SendButton);
            AppBar.PrimaryCommands.Add(lenny);
            AppBar.PrimaryCommands.Add(attachment);
            AppBar.PrimaryCommands.Add(FormattingButton);

            return AppBar;
        }
        #endregion

        #region Formatting
        internal enum FormattingEnum
        {
            NONE,
            BOLD,
            ITALIC,
            CODE,
            SPOILER,
            LINK,
            QUOTE
        };

        private FormattingEnum SelectedFormatting;
        private readonly Dictionary<FormattingEnum, Tuple<string, string>> FormattingPrefixes = new Dictionary<FormattingEnum, Tuple<string, string>>()
        {
            { FormattingEnum.BOLD, Tuple.Create<string, string>("**", "** ") },
            { FormattingEnum.ITALIC, Tuple.Create<string, string>("_", "_ ") },
            { FormattingEnum.CODE, Tuple.Create<string, string>("`", "` ") },
            { FormattingEnum.QUOTE, Tuple.Create<string, string>("\n> ", "\n") },
            { FormattingEnum.SPOILER, Tuple.Create<string, string>("\n! ", "\n") },
        };

        private void AppendTextAndHideFlyout(string text, string flyoutName)
        {
            var start = CurrentEditor().SelectionStart;
            var newText = CurrentEditor().Text.Insert(start, text);

            CurrentEditor().Text = newText;
            CurrentEditor().SelectionStart = start + newText.Length;

            var f = this.Resources[flyoutName] as FlyoutBase;
            f.Hide();
        }

        private void HyperlinkFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = "[" + this.DescriptionTextBox.Text + "](" + this.LinkTextBox.Text + ")";
            AppendTextAndHideFlyout(txt, "HyperlinkFlyout");

            this.DescriptionTextBox.Text = "";
            this.LinkTextBox.Text = "";
        }

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            AppendTextAndHideFlyout(txt, "LennysFlyout");
        }

        private void FormattingPopup_Loaded(object sender, RoutedEventArgs e)
        {
            var popup = sender as Popup;
            if (popup.VerticalOffset == 0)
            {
                var stackPanel = popup.Child as Grid;
                stackPanel.Width = Window.Current.Bounds.Width;
                popup.Width = Window.Current.Bounds.Width;

                popup.VerticalOffset = this.ActualHeight - 2 * stackPanel.Height;
            }
        }

        private void SetFormattingButton()
        {
            string label = "", icon = "";

            if (SelectedFormatting == FormattingEnum.BOLD)
            {
                label = "pogrubienie";
                icon = "appbar.text.bold.png";
            }
            else if (SelectedFormatting == FormattingEnum.ITALIC)
            {
                label = "pochylenie";
                icon = "appbar.text.italic.png";
            }
            else if (SelectedFormatting == FormattingEnum.CODE)
            {                
                label = "kod";
                icon = "appbar.code.xml.png";
            }
            else if(SelectedFormatting == FormattingEnum.SPOILER)
            {
                label = "spoiler";
            }
            else if (SelectedFormatting == FormattingEnum.QUOTE)
            {
                label = "cytat";
                icon = "appbar.quote.png";
            }

            FormattingButton.Label = label;
            if (!string.IsNullOrEmpty(icon))
                FormattingButton.Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/" + icon) };
            else
                FormattingButton.Icon = new FontIcon() { Glyph = "!", FontSize = 28, FontWeight = FontWeights.Bold };
            FormattingButton.IsChecked = true;
            FormattingButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void FormattingChanged(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var tag = button.Tag as string;

            if (tag == "bold")
                SelectedFormatting = FormattingEnum.BOLD;
            else if (tag == "italic")
                SelectedFormatting = FormattingEnum.ITALIC;
            else if (tag == "code")
                SelectedFormatting = FormattingEnum.CODE;
            else if (tag == "spoiler")
                SelectedFormatting = FormattingEnum.SPOILER;
            else if (tag == "link")
                SelectedFormatting = FormattingEnum.LINK;
            else if (tag == "quote")
                SelectedFormatting = FormattingEnum.QUOTE;

            if(SelectedFormatting == FormattingEnum.LINK)
            {
                var f = Resources["HyperlinkFlyout"] as Flyout;
                f.ShowAt(this);
                return;
            }

            if (CurrentEditor().SelectedText.Length > 0)
            {
                var selectedText = CurrentEditor().SelectedText;
                var prefix = FormattingPrefixes[SelectedFormatting].Item1;
                var newText = prefix + selectedText +
                    FormattingPrefixes[SelectedFormatting].Item2;

                var start = CurrentEditor().SelectionStart;
                var length = CurrentEditor().SelectionLength;
                var txt = CurrentEditor().Text.Remove(start, length).Insert(start, newText);
                CurrentEditor().Text = txt;

                CurrentEditor().SelectionStart = start + newText.Length;
                CurrentEditor().Focus(FocusState.Programmatic);

                SelectedFormatting = FormattingEnum.NONE;
            }
            else
            {
                var start = CurrentEditor().SelectionStart;
                var prefix = FormattingPrefixes[SelectedFormatting].Item1;
                                
                if(SelectedFormatting == FormattingEnum.SPOILER || SelectedFormatting == FormattingEnum.QUOTE)
                {
                    if (CurrentEditor().Text.Length == 0 || CurrentEditor().Text.EndsWith("\n"))
                        prefix = prefix.TrimStart(); // remove newline character
                }

                var newText = CurrentEditor().Text.Insert(start, prefix);

                CurrentEditor().Text = newText;
                CurrentEditor().SelectionStart = start + prefix.Length;
                SetFormattingButton();
                CurrentEditor().Focus(FocusState.Programmatic);
            }
        }

        private void FormattingButton_Click(object sender, RoutedEventArgs e)
        {
            var insertion = FormattingPrefixes[SelectedFormatting].Item2;
            if (CurrentEditor().SelectedText.Length > 0)
            {
                var selectedText = CurrentEditor().SelectedText;
                var newText = selectedText + insertion;

                var start = CurrentEditor().SelectionStart;
                var length = CurrentEditor().SelectionLength;
                var txt = CurrentEditor().Text.Remove(start, length).Insert(start, newText);
                CurrentEditor().Text = txt;

                CurrentEditor().SelectionStart = start + newText.Length;
            }
            else
            {
                var length = CurrentEditor().Text.Length;
                CurrentEditor().Text += insertion;
                CurrentEditor().SelectionStart = length + insertion.Length;
            }

            SelectedFormatting = FormattingEnum.NONE;
            FormattingButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            CurrentEditor().Focus(FocusState.Programmatic);
        }

        #endregion

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            HandleSendButton();
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleSendButton();
        }
    }
}
