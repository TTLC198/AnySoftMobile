using System;
using System.Globalization;
using Xamarin.Forms;

namespace AnySoftMobile.Utils.Converters;

public class NullToBoolConverter : IValueConverter
{
    public static NullToBoolConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseNullToBoolConverter : IValueConverter
{
    public static InverseNullToBoolConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? true : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}