using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{
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

            if (SimulatorViewModel.StaticShowAsHexadecimal)
            {
                valueString = SimulatorViewModel.StaticShowAsByte
                    ? BitConverter.ToString(memoryDataBytes).Replace("-", " ")
                    : "0x" + BitConverter.ToUInt32(memoryDataBytes, 0).ToString("X8");
            }
            else
            {
                if (SimulatorViewModel.StaticShowAsByte)
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? memoryDataBytes.Aggregate("", (current, memoryDataByte) => current + ((sbyte) memoryDataByte).ToString() + " ")
                        : memoryDataBytes.Aggregate("", (current, memoryDataByte) => current + memoryDataByte.ToString() + " ");
                }
                else
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? BitConverter.ToInt32(memoryDataBytes, 0).ToString()
                        : BitConverter.ToUInt32(memoryDataBytes, 0).ToString();
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
