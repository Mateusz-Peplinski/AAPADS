
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AAPADS
{
    public class ChannelColorConverter : IValueConverter
    {
        private static readonly HashSet<int> twoGhzChannels = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int channel && twoGhzChannels.Contains(channel))
            {
                return Brushes.YellowGreen;
            }
            else
            {
                return Brushes.LightSkyBlue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


