using System;
using System.Globalization;
using System.Windows.Data;
using ARM_Simulator.Annotations;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class BreakpointConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var command = value as ObservableCommand;
            if (command == null)
                return null;

            if (command.Breakpoint)
                return "\u25A0";

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
