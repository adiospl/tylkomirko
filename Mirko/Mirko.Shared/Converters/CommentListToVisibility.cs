﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WykopSDK.API.Models;

namespace Mirko.Converters
{
    public class CommentListToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            var list = value as List<EntryComment>;

            if (list?.Count() > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
