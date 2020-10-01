using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace UGCS.TelemetryViewer.Helpers
{
    public class StringDoubleConverter : IValueConverter
    {
        public static double? ConvertBack(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (double.TryParse(value, out double d))
            {
                return d;
            }
            return null;
        }

        public static string Convert(double? value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value as double?);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value as string);
        }
    }
}
