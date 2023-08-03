using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AAPADS
{
    public class RSSIIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rssi)
            {
                if (rssi >= 0 && rssi <= 30)
                {
                    return MaterialDesignThemes.Wpf.PackIconKind.SignalCellularOutline;
                }
                else if (rssi > 30 && rssi <= 60)
                {
                    return MaterialDesignThemes.Wpf.PackIconKind.SignalCellular1;
                }
                else if (rssi > 60 && rssi <= 80)
                {
                    return MaterialDesignThemes.Wpf.PackIconKind.SignalCellular2;
                }
                else if (rssi > 80 && rssi <= 100)
                {
                    return MaterialDesignThemes.Wpf.PackIconKind.SignalCellular3;
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
