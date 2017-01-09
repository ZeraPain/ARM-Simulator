using System;
using System.Globalization;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class UintToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var address = value as uint?;
            if (address == null)
                return null;

            var hexCount = parameter as string ?? "X8";

            return "0x" + ((uint)address).ToString(hexCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
