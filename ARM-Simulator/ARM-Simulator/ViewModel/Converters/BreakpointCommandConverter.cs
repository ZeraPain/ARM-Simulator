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
            ObservableCollection<string> commandBreakpointList = null;
            if (commandList != null)
            {
                foreach (var command in commandList)
                {
                    commandBreakpointList = new ObservableCollection<string>();
                    if (command.Breakpoint)
                    {
                        commandBreakpointList.Add(command.Commandline);
                    }
                }
            }
            return commandBreakpointList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
