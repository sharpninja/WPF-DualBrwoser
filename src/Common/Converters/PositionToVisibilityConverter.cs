using System.Globalization;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DualBrowser.Converters;

public class PositionToVisibilityConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value is string s && s is "Primary" or "Secondary")
        {
            return s switch
            {
                "Primary" => Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }

        return Visibility.Collapsed;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        return null;
    }
}