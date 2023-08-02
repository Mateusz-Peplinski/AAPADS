using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AAPADS
{
    public class SSIDColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string signalStrength = value as string;

            // Map signal strength levels to colors
            switch (signalStrength)
            {
                case "EXCELLENT":
                    return Brushes.LimeGreen;
                case "GOOD":
                    return Brushes.Green;
                case "FAIR":
                    return Brushes.Orange;
                case "POOR":
                    return Brushes.DarkOrange;
                case "VERY POOR":
                    return Brushes.Red;
                default:
                    return Brushes.Black; // Default color
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}