using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ARM_Simulator.Annotations;
using ARM_Simulator.Resources;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel.Converters
{
    class ColorConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var command = value as ObservableCommand;
            if (command == null)
                return null;

            switch (command.Status)
            {
                case EPipeline.None:
                    return null;
                case EPipeline.Fetch:
                    return new SolidColorBrush(Colors.LawnGreen);
                case EPipeline.Decode:
                    return new SolidColorBrush(Colors.Yellow);
                case EPipeline.Execute:
                    return new SolidColorBrush(Colors.OrangeRed);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
