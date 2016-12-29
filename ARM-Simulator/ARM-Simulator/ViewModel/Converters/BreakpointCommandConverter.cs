using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ARM_Simulator.Annotations;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class BreakpointCommandConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var commandList = value as ObservableCollection<ObservableCommand>;

            if (commandList == null) return null;
            return (from command in commandList where command.Breakpoint select command.Commandline);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
