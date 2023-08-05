using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AAPADS
{
    public class DetectionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "NEW":
                        return Brushes.OrangeRed;
                    case "IN-PROGRESS":
                        return Brushes.LightBlue;
                    case "CLOSED":
                        return Brushes.LightGreen;
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

