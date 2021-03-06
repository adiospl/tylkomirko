﻿using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                await StatusBarManager.HideStatusBarAsync();
                var VM = this.DataContext as SettingsViewModel;

                if (VM.SelectedTheme == ElementTheme.Dark)
                    NightMode.IsChecked = true;
                else
                    DayMode.IsChecked = true;

                DayMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Light;
                NightMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Dark;
            };

            this.Unloaded += async (s, e) => await StatusBarManager.ShowStatusBarAsync();
        }
    }
}
