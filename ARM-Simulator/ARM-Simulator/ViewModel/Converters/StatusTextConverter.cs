using System;
using System.Globalization;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
    // Converter that is used for the StatusBar to show the current status of the debugger
   internal class StatusTextConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var debugmode = value as bool?;
            if (debugmode == null)
                return null;

            return debugmode == true ? "Debugmode ON" : "Debugmode OFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}

