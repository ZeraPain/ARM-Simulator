using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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

                for (var k = 0; k < 32 / 4; k++)
                {
                    var memoryDataBytes = new byte[4];
                    Array.Copy(data, baseAddr + k*4, memoryDataBytes, 0, 4);
                    
                 if(ShowAsHexadecimal)  memoryOffset[k] = "0x" + BitConverter.ToUInt32(memoryDataBytes, 0).ToString("X8");
                if(!ShowAsHexadecimal) memoryOffset[k] = BitConverter.ToUInt32(memoryDataBytes,0).ToString();
                    if (ShowAsAscii)
                    {/*
                        //byteweise
                        var convert = BitConverter.ToInt32(memoryDataBytes, 0).ToString();
                        int ascii;
                        if (Int32.TryParse(convert, out ascii))
                        {
                            if (ascii >= 0 && ascii < 128)
                            {
                                memoryOffset[k] = ((char) ascii).ToString();
                               
                            }
                        }
                        */
                    }
                }

                if (MemoryView.Count <= i)
                {
                    MemoryView.Add(new ObservableMemoryStream() { BaseAddress = baseAddress, MemoryOffset = memoryOffset});
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
