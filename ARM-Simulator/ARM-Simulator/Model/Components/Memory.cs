using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ARM_Simulator.Annotations;

namespace ARM_Simulator.Model.Components
{
    public class Memory : INotifyPropertyChanged
    {
        public byte[] Ram { get; protected set; }
        private readonly uint _codeSectionEnd;

        public Memory(uint ramSize, uint codeSectionEnd)
        {
            if (codeSectionEnd > ramSize)
                throw new ArgumentException("Code section cannot be bigger than ram size");

            Ram = new byte[ramSize];
            _codeSectionEnd = codeSectionEnd;
        }

        public int GetRamSize() => Ram.Length;

        public void LoadSource([NotNull] List<int> source)
        {
            var data = source.SelectMany(BitConverter.GetBytes).ToArray();

            if (data.Length > _codeSectionEnd)
                throw new IndexOutOfRangeException("Code section is too small to load the source");

            Array.Copy(data, 0x0, Ram, 0x0, data.Length);
            OnPropertyChanged(nameof(Ram));
        }

        public void WriteInt(uint address, int data) => Write(address, BitConverter.GetBytes(data));

        public void WriteByte(uint address, byte data) => Write(address, BitConverter.GetBytes(data));

        public void Write(uint address, [NotNull] byte[] data)
        {
            if (address < _codeSectionEnd)
                throw new AccessViolationException("Cannot write to the code section");

            if (address + data.Length > Ram.Length)
                throw new AccessViolationException("Memory out of range requested");

            Array.Copy(data, 0, Ram, address, data.Length);
            OnPropertyChanged(nameof(Ram));
        }

        public int ReadInt(uint address) => BitConverter.ToInt32(Read(address, 4), 0);

        public byte ReadByte(uint address) => Read(address, 1)[0];

        [NotNull]
        public byte[] Read(uint address, int length)
        {
            if (address + length > Ram.Length)
                throw new IndexOutOfRangeException("Undefined memory location");

            var buffer = new byte[length];
            Array.Copy(Ram, address, buffer, 0, buffer.Length);

            return buffer;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
