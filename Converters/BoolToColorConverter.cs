using Microsoft.Maui.Graphics;
using System.Globalization;

namespace CrossPlatformProject2.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isSelected && isSelected ? Colors.Green : Colors.Blue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
