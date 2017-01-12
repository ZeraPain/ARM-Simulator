using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
    // Converting the visibility depending on debugger mode on or off
    internal class VisibilityConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = value as bool?;
            var command = parameter as string;

            if (bValue == null)
                return null;

            if (string.IsNullOrEmpty(command))
            {
                return bValue == true ? Visibility.Visible : Visibility.Collapsed;
            }

            if (command == "invert")
            {
                return bValue == true ? Visibility.Collapsed : Visibility.Visible;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
