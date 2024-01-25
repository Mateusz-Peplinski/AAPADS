using System;
using System.Globalization;
using System.Windows.Data;

namespace AAPADS
{
    [ValueConversion(typeof(int), typeof(string))]
    public class HoursToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int hours)
            {
                if (hours == 24)
                {
                    return "24:00:00";
                }
                return TimeSpan.FromHours(hours).ToString(@"hh\:mm\:ss");
            }
            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeString && TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
            {
                return (int)timeSpan.TotalHours;
            }
            return 0;
        }
    }
}
