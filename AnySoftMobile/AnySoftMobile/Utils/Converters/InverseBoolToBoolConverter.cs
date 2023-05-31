using System;
using System.Globalization;
using Xamarin.Forms;

namespace AnySoftMobile.Utils.Converters;

public class InverseBoolToBoolConverter : IValueConverter
{
    public static InverseBoolToBoolConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is not true;
}