using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class DisplayValueConverter : IValueConverter
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

            var valueString = BitConverter.ToUInt32(memoryDataBytes, 0).ToString();
            if (MemoryViewModel.StaticShowAsHexadecimal) valueString = "0x" + BitConverter.ToUInt32(memoryDataBytes, 0).ToString("X8");
            if (MemoryViewModel.StaticShowAsByte && MemoryViewModel.StaticShowAsHexadecimal) valueString = BitConverter.ToString(memoryDataBytes).Replace("-", " ");
            if (MemoryViewModel.StaticShowAsByte && !MemoryViewModel.StaticShowAsHexadecimal) valueString = memoryDataBytes.Aggregate("", (current, memoryDataByte) => current + memoryDataByte.ToString() + " ");

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
