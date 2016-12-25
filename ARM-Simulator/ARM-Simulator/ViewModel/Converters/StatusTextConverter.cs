using System;
using System.Globalization;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
   internal class StatusTextConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var debugmode = value as bool?;
            var text = parameter as string;

            if (debugmode == null || parameter == null) return null;

            switch (text)
            {
                case "Editor":
                    return debugmode == true ? "Debugmode" : "Editor";
                case "Debugger":
                    return debugmode == true ? "Editor" : "Debugmode";
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}

