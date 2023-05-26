﻿using System;
using Xamarin.Forms;

namespace AnySoftMobile.Utils.Converters;

public class UrlToImageConverter : IValueConverter
{
    public static UrlToImageConverter Instance { get; } = new();
    
    public object? Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var path = (value ?? "").ToString();
        if (path is null)
            return "";
        if (path.StartsWith("/"))
            path = path.Substring(1);
        return $"{App.CdnUrl}{path}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}