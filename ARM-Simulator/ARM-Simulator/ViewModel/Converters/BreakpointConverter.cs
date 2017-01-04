using System;
using System.Globalization;
using System.Windows.Data;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class BreakpointConverter : IValueConverter
    {
        [NotNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var breakpoint = value as bool?;
            return breakpoint == true ? "\u25A0" : "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
