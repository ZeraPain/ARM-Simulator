using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel
{
    internal class VisibilityConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var debugmode = value as bool?;
            var grid = parameter as string;
            if (debugmode == null || parameter == null) return null;

            switch (grid)
            {
                case "Editor":
                    return debugmode == true ? Visibility.Collapsed : Visibility.Visible;
                case "Debugger":
                    return debugmode == true ? Visibility.Visible : Visibility.Collapsed;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
