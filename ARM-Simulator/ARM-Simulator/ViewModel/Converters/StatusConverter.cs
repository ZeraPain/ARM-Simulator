using System;
using System.Globalization;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{
    // Converter that is used for the StatusBar to visualize the debugger status with a picture
   internal class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var debugmode = value as bool?;
            if (debugmode == null)
                return null;

            return debugmode == true ? "/Resources/Images/on.png" : "/Resources/Images/off.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
