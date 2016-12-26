using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.ViewModel.Converters
{
   internal class NumberSystemConverter
    {
        [CanBeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var registerValue = value as string;
            var radiobutton = parameter as string;
            if (registerValue == null || parameter == null) return null;

            //test!!!
            switch (radiobutton)
            {
                case "decimal":
                    return (int.Parse(registerValue,NumberStyles.HexNumber));
                case "hexadecimal":

                case "signed":

                case "unsigned":

                case "word":

                case "byte":

                    break;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
