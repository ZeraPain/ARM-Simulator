using System;
using System.Globalization;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class ByteArrayToAsciiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memBytes = value as byte[];
            if (memBytes == null)
                return null;

            var ascii = "";

            foreach (var memByte in memBytes)
            {
                if (memByte > 31 && memByte < 127)
                    ascii += (char)memByte;
                else
                    ascii += ".";
            }

            return ascii;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
