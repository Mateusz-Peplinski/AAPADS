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
                    return Brushes.YellowGreen;
                }
                else if (frequency.StartsWith("5"))
                {
                    return Brushes.LightBlue;
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
