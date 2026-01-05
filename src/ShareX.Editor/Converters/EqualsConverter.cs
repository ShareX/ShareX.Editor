using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ShareX.Editor.Converters
{
    public class EqualsConverter : IValueConverter
    {
        public static readonly EqualsConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueString = value?.ToString();
            var parameterString = parameter?.ToString();
            return string.Equals(valueString, parameterString, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // One-way converter
            throw new NotSupportedException();
        }
    }
}
