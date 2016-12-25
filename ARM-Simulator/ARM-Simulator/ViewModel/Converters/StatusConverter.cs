using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
   internal class StatusConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var debugmode = value as bool?;
            var image = parameter as string;
         
            if (debugmode == null || parameter == null) return null;

            switch (image)
            {
                case "Editor":
                    return debugmode == true ? "/Resources/Images/on.png" : "/Resources/Images/off.png";      
                case "Debugger":
                    return debugmode== true ? "/Resources/Images/off.png" : "/Resources/Images/on.png";
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
