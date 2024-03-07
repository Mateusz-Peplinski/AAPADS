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
    public class FrameTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var frameType = value as string;
            if (!string.IsNullOrEmpty(frameType))
            {
                if (frameType.Equals("802.11 ManagementDeauthentication", StringComparison.OrdinalIgnoreCase) ||
                    frameType.Equals("802.11 ManagementDisassociation", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 239, 57, 69)); // #ef3945
                }
                else if (frameType.Equals("802.11 Data", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 110, 204, 37)); // #6ecc25
                }
                else if (frameType.Equals("802.11 ManagementBeacon", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 125, 255)); // #ffe000
                }
                else
                {    
                    return new SolidColorBrush(Color.FromArgb(255, 255, 224, 0)); // #ffe000
                }
            }
            return new SolidColorBrush(Colors.Black); // Fallback color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


}
