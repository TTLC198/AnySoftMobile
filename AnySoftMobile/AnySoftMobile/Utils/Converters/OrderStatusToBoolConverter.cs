using System;
using System.Globalization;
using Xamarin.Forms;

namespace AnySoftMobile.Utils.Converters;

public class OrderStatusToBoolConverter : IValueConverter
{
    public static OrderStatusToBoolConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 
            value is not (string and "Paid");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}