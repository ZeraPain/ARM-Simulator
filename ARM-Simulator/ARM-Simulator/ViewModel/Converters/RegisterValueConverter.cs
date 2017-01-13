using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ARM_Simulator.Model;

namespace ARM_Simulator.ViewModel.Converters
{
    /* Converter that is used for displaying the numeric type depening on the user selection for the register view
     * and manages the editing of register values
     */
    internal class RegisterValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var regValue = value as int?;
            if (regValue == null)
                return null;

            string valueString;

            if (SimulatorViewModel.StaticShowAsHexadecimal) // HEX
            {
                valueString = SimulatorViewModel.StaticShowAsByte
                    ? "0x" + ((byte)regValue).ToString("X") // HEX BYTE
                    : "0x" + ((int)regValue).ToString("X"); // HEX WORD
            }
            else // DEC
            {
                if (SimulatorViewModel.StaticShowAsByte)
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? ((sbyte)regValue).ToString() // DEC SIGNED BYTE
                        : ((byte)regValue).ToString(); // DEC UNSIGNED BYTE
                }
                else
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? ((int)regValue).ToString() // DEC SIGNED WORD
                        : ((uint)regValue).ToString(); // DEC UNSIGNED WORD
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = value as string;
            if (newValue == null)
                return null;

            try
            {
                // User can enter any kind of calue, doesnt matter if its signed, hex (0x) or dec. Register will be automatically updated
                return unchecked((int)Parser.ParseImmediate<long>(newValue));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
                return newValue;
            }
        }
    }
}
