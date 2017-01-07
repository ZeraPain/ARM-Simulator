using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using ARM_Simulator.Annotations;
using ARM_Simulator.Model.Components;
using ARM_Simulator.ViewModel.Observables;

namespace ARM_Simulator.ViewModel
{
    internal class MemoryViewModel : ContextMenuHandler
    {
        public ObservableCollection<ObservableMemoryStream> MemoryView { get; protected set; }
        
        private readonly Memory _memory;

        public MemoryViewModel(Memory memory)
        {
            _memory = memory;
            _memory.PropertyChanged += Update;
            _memory.AllowUnsafeCode = true;

            ContextMenuUpdate = UpdateMemoryView;

            MemoryView = new ObservableCollection<ObservableMemoryStream>();
           
        }

        private void Update(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Ram":
                    UpdateMemoryView();
                    break;
            }
        }

        private void UpdateMemoryView()
        {
            var data = _memory.Ram;

            for (var i = 0; i < data.Length/32; i++)
            {
                var baseAddr = i*32;
                var baseAddress = "0x" + baseAddr.ToString("X8");
                var memoryOffset = new string[8];
                var ascii = "";

                for (var k = 0; k < 32 / 4; k++)
                {
                    var memoryDataBytes = new byte[4];
                    Array.Copy(data, baseAddr + k * 4, memoryDataBytes, 0, 4);

                    var valueString =  BitConverter.ToUInt32(memoryDataBytes, 0).ToString();
                    if (ShowAsHexadecimal) valueString = "0x" + BitConverter.ToUInt32(memoryDataBytes, 0).ToString("X8");
                    if (ShowAsByte && ShowAsHexadecimal) valueString = BitConverter.ToString(memoryDataBytes).Replace("-"," ");
                    if (ShowAsByte && !ShowAsHexadecimal)
                    {
                        // Convert decimal number to ShowAsByte
                    }
 
                    memoryOffset[k] = valueString;

                    //build string of Ascii symbols
                    var asciiBytes = Encoding.ASCII.GetString(memoryDataBytes);
                    ascii += asciiBytes+" ";
                    
                    
                }

                if (MemoryView.Count <= i)
                {
                    MemoryView.Add(new ObservableMemoryStream() { BaseAddress = baseAddress, MemoryOffset = memoryOffset, Ascii = ascii});
                }
                else
                {
                    MemoryView[i].BaseAddress = baseAddress;
                    MemoryView[i].MemoryOffset = memoryOffset;
                }
            }
        }
    }
}
