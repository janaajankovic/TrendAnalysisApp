using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrendAnalysis.UI.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // Ako je true, vrati Visible; inače, vrati Collapsed
                return booleanValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed; // Default za nepoznate vrijednosti
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Konverzija nazad nije potrebna za ProgressBar
            return DependencyProperty.UnsetValue;
        }
    }
}