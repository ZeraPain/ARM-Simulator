using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ARM_Simulator.Model;

namespace ARM_Simulator.ViewModel.Converters
{
    internal class RegisterValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var regValue = value as int?;
            if (regValue == null)
                return null;

            string valueString;

            if (SimulatorViewModel.StaticShowAsHexadecimal)
            {
                valueString = SimulatorViewModel.StaticShowAsByte
                    ? ((byte)regValue).ToString("X")
                    : "0x" + ((int)regValue).ToString("X");
            }
            else
            {
                if (SimulatorViewModel.StaticShowAsByte)
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? ((sbyte)regValue).ToString()
                        : ((byte)regValue).ToString();
                }
                else
                {
                    valueString = SimulatorViewModel.StaticShowAsSigned
                        ? ((int)regValue).ToString()
                        : ((uint)regValue).ToString();
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = value as string;
            if (newValue == null)
                return null;

            int regValue = 0;

            try
            {
                if (SimulatorViewModel.StaticShowAsByte)
                {
                    if (SimulatorViewModel.StaticShowAsSigned)
                    {
                        regValue = SimulatorViewModel.StaticShowAsHexadecimal
                            ? sbyte.Parse(newValue, NumberStyles.HexNumber)
                            : sbyte.Parse(newValue);
                    }
                    else
                    {
                        regValue = SimulatorViewModel.StaticShowAsHexadecimal
                            ? byte.Parse(newValue, NumberStyles.HexNumber)
                            : byte.Parse(newValue);
                    }
                }
                else
                {
                    regValue = SimulatorViewModel.StaticShowAsSigned 
                        ? Parser.ParseImmediate<int>(newValue) 
                        : System.Convert.ToInt32(Parser.ParseImmediate<uint>(newValue));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }

            return regValue;
        }
    }
}
