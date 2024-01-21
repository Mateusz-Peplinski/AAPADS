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
    //#####                       Converts WNIC status to a colour                  #######            
    //#####################################################################################
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "ACTIVE":
                        Color colorActive = (Color)ColorConverter.ConvertFromString("#6ecc25");
                        return new SolidColorBrush(colorActive);

                    default:
                        Color colorOtherStatus = (Color)ColorConverter.ConvertFromString("#ef3945");
                        return new SolidColorBrush(colorOtherStatus);
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

