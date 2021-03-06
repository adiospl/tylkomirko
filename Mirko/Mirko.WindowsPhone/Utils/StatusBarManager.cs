﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Mirko.Utils
{
    public static class StatusBarManager
    {
        private static DispatcherTimer Timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 7) };

        private static void Timer_Tick(object sender, object e)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = " ";

            Timer.Tick -= Timer_Tick;
        }

        private static Action<StatusBarProgressIndicator> ShowProgressIndicator = new Action<StatusBarProgressIndicator>(async prog =>
        {
            await DispatcherHelper.RunAsync(async () => await prog.ShowAsync());
        });

        private static Action<StatusBarProgressIndicator> HideProgressIndicator = new Action<StatusBarProgressIndicator>(async prog =>
        {
            await prog.HideAsync();
        });

        private static Action<StatusBar> ShowSB = new Action<StatusBar>(async sb =>
        {
            await sb.ShowAsync();
        });

        private static Action<StatusBar> HideSB = new Action<StatusBar>(async sb =>
        {
            await sb.HideAsync();
        });

        public static void Paint(ElementTheme theme)
        {
            var statusBar = StatusBar.GetForCurrentView();

            if (theme == ElementTheme.Light)
            {
                statusBar.BackgroundColor = Colors.White;
                statusBar.ForegroundColor = (Color)Application.Current.Resources["StatusBarForegroundLight"];
            }
            else
            {
                statusBar.BackgroundColor = Colors.Black;
                statusBar.ForegroundColor = Colors.White;
            }
        }

        public static void Init()
        {
            var statusBar = StatusBar.GetForCurrentView();
            var settingsVM = SimpleIoc.Default.GetInstance<Mirko.ViewModel.SettingsViewModel>();
            settingsVM.ThemeChanged += (s, args) => Paint((args as ThemeChangedEventArgs).Theme);
            Paint(settingsVM.SelectedTheme);

            statusBar.BackgroundOpacity = (double)Application.Current.Resources["AppHeaderOpacity"];
            statusBar.ProgressIndicator.Text = " ";
            statusBar.ProgressIndicator.ProgressValue = 0.0;
            ShowProgressIndicator(statusBar.ProgressIndicator);
        }

        public static async Task ShowTextAsync(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();

                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = 0.0;

                if (Timer.IsEnabled)
                {
                    Timer.Stop();
                    Timer.Start();

                    Timer.Tick += Timer_Tick;
                }
                else
                {
                    Timer.Start();
                    Timer.Tick += Timer_Tick;
                }
            });            
        }

        public static void ShowText(string txt)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();

                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = 0.0;

                if (Timer.IsEnabled)
                {
                    Timer.Stop();
                    Timer.Start();

                    Timer.Tick += Timer_Tick;
                }
                else
                {
                    Timer.Start();
                    Timer.Tick += Timer_Tick;
                }
            });
        }

        public static async Task<string> GetText()
        {
            string text = null;

            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                text = statusBar.ProgressIndicator.Text;
            });

            return text;
        }

        public static async Task ShowTextAndProgressAsync(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                
                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = null;
            });
        }

        public static void ShowTextAndProgress(string txt)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();

                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = null;
            });
        }

        public static async Task HideStatusBarAsync()
        {
            await DispatcherHelper.RunAsync(async () => 
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            });
        }

        public static void HideStatusBar()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            });
        }

        public static async Task ShowStatusBarAsync()
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            });
        }

        public static void ShowStatusBar()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            });
        }

        public static async Task ShowProgressAsync(double? prog = null)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = " ";
                statusBar.ProgressIndicator.ProgressValue = prog;
            });
        }

        public static void ShowProgress()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = " ";
                statusBar.ProgressIndicator.ProgressValue = null;
            });
        }

        public static async Task HideProgressAsync()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0.0;
                statusBar.ProgressIndicator.Text = " ";
            });
        }

        public static void HideProgress()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0.0;
                statusBar.ProgressIndicator.Text = " ";
            });
        }
    }
}
