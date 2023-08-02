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
    public class FrequencyBandColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string band)
            {
                switch (band)
                {
                    case "2.4 GHz":
                        return Brushes.YellowGreen;  
                    case "5 GHz":
                        return Brushes.LightSkyBlue; 
                    default:
                        return Brushes.AntiqueWhite; 
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
