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
    //#####################################################################################
    //#####             Converts WiFi RSSI Value to a certain colour                #######            
    //#####################################################################################
    public class RSSIColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rssi)
            {
                if (rssi >= 0 && rssi <= 30)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ef3945"));
                }
                else if (rssi > 30 && rssi <= 60)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9c00"));
                }
                else if (rssi > 60 && rssi <= 80)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe000"));
                }
                else if (rssi > 80 && rssi <= 100)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ee6c"));
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
