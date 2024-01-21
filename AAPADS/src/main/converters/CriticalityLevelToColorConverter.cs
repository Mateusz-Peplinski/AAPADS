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
    //#####################################################################################
    //#####           Converts detection criticality to a certain colour            #######            
    //#####################################################################################
    public class CriticalityLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string level)
            {
                switch (level)
                {
                    case "LEVEL_5":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff002a"));
                    case "LEVEL_4":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9c00"));
                    case "LEVEL_3":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe000"));
                    case "LEVEL_2":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6ecc25"));
                    case "LEVEL_1":
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#167dff"));
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
