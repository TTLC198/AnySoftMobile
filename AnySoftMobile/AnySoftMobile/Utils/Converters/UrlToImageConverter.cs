using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using AnySoftMobile;
using Xamarin.Forms;

namespace AnySoftDesktop.Utils.Converters;

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