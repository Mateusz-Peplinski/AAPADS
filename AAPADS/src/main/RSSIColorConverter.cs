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
                if (rssi >= 0 && rssi <= 40)
                {
                    return Brushes.Red;
                }
                else if (rssi > 40 && rssi <= 70)
                {
                    return Brushes.Orange;
                }
                else if (rssi > 70 && rssi <= 100)
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
