using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{   
    // Converter that is used for displaying the numeric type depening on the user selection for the memory view
    internal class MemoryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memStream = value as byte[];
            var sIndex = parameter as string;

            if (memStream == null || sIndex == null)
                return null;

            var index = int.Parse(sIndex);
            if (index * 4 >= memStream.Length)
                return null;

            var memoryDataBytes = new byte[4];
            Array.Copy(memStream, index * 4, memoryDataBytes, 0, 4);

            string valueString;

            if (SimulatorViewModel.StaticShowAsHexadecimal) // HEX
            {
                valueString = SimulatorViewModel.StaticShowAsByte
                    ? BitConverter.ToString(memoryDataBytes).Replace("-", " ") // HEX BYTE
                    : "0x" + BitConverter.ToUInt32(memoryDataBytes, 0).ToString("X8"); // HEX WORD
            }
            else // DEC
            {
                if (SimulatorViewModel.StaticShowAsByte)
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? memoryDataBytes.Aggregate("", (current, memoryDataByte) => current + ((sbyte) memoryDataByte).ToString() + " ") // DEC SIGNED BYTE
                        : memoryDataBytes.Aggregate("", (current, memoryDataByte) => current + memoryDataByte.ToString() + " "); // DEC UNSIGNED BYTE
                }
                else
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? BitConverter.ToInt32(memoryDataBytes, 0).ToString() // DEC SIGNED WORD
                        : BitConverter.ToUInt32(memoryDataBytes, 0).ToString(); // DEC UNSIGNED WORD
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
