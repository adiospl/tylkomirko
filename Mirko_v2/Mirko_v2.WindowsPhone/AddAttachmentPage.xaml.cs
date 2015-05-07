﻿using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Mirko_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddAttachmentPage : Page
    {
        public AddAttachmentPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter == null)
                this.DataContext = null;// SimpleIoc.Default.GetInstance<MessagesViewModel>();
            else if(e.Parameter as string == "PM")
                this.DataContext = SimpleIoc.Default.GetInstance<MessagesViewModel>().CurrentConversation;            
        }
    }
}