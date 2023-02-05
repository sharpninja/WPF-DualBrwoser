using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DualBrowser.Converters;

public class PositionToVisibilityConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && s is "Primary" or "Secondary")
        {
            return s switch
            {
                "Primary" => Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }

        return Binding.DoNothing;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}