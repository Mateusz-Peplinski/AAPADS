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
    public class FrequencyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string frequency && frequency != null)
            {
                if (frequency.StartsWith("2"))
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#ef3945");
                    return new SolidColorBrush(color);
                }
                else if (frequency.StartsWith("5"))
                {
                    Color color5GHz = (Color)ColorConverter.ConvertFromString("#167dff");
                    return new SolidColorBrush(color5GHz);
                }
            }
            return Brushes.AntiqueWhite;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
