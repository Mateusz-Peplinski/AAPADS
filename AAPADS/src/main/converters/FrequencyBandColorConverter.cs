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
                        Color color24GHz = (Color)ColorConverter.ConvertFromString("#ef3945");
                        return new SolidColorBrush(color24GHz);

                    case "5 GHz":
                        Color color5GHz = (Color)ColorConverter.ConvertFromString("#167dff");
                        return new SolidColorBrush(color5GHz);

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
