using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace AAPADS
{
    public class CriticalityLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string level)
            {
                switch (level)
                {
                    case "LEVEL_5":
                        return Brushes.Red;
                    case "LEVEL_4":
                        return Brushes.Orange;
                    case "LEVEL_3":
                        return Brushes.Yellow;
                    case "LEVEL_2":
                        return Brushes.LightGreen;
                    case "LEVEL_1":
                        return Brushes.LightBlue;
                    default:
                        return Brushes.Black;
                }
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
