using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AAPADS
{
    //#####################################################################################
    //#####           Converts detection criticality to a certain colour            #######            
    //#####################################################################################
    public class CriticalityLevelToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string level)
            {
                switch (level)
                {
                    case "LEVEL_5":
                        return "CRITIAL ";
                    case "LEVEL_4":
                        return "   HIGH ";
                    case "LEVEL_3":
                        return " MEDIUM ";
                    case "LEVEL_2":
                        return "   LOW  ";
                    case "LEVEL_1":
                        return "  INFO  ";
                    default:
                        return "ERROR";
                }
            }
            else
            {
                return "ERROR";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
