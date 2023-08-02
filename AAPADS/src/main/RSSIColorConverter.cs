using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AAPADS
{
    public class RSSIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rssi)
            {
                if (rssi >= 0 && rssi <= 30)
                {
                    return Brushes.Red;
                }
                else if (rssi > 30 && rssi <= 60)
                {
                    return Brushes.Orange;
                }
                else if (rssi > 60 && rssi <= 80)
                {
                    return Brushes.Yellow;
                }
                else if (rssi > 80 && rssi <= 100)
                {
                    return Brushes.Green;
                }
            }

            return Brushes.Black; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
