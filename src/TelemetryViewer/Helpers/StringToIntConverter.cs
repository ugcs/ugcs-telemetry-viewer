using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace UGCS.TelemetryViewer.Helpers
{
    public class StringToIntConverter : IValueConverter
    {
        public static int? ConvertBack(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (int.TryParse(value, out int i))
            {
                return i;
            }
            return null;
        }

        public static string Convert(int? value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value as int?);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value as string);
        }
    }
}
