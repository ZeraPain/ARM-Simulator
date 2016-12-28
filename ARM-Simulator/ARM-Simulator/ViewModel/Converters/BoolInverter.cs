using System;
using System.Globalization;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class BoolInverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as bool?;

            return !status;
        }

        [CanBeNull]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as bool?;

            return !status;
        }
    }
}
