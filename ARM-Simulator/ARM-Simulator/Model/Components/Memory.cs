using System;
using System.Collections.Generic;
using System.Linq;

namespace ARM_Simulator.Model.Components
{
    public class Memory
    {
        private readonly byte[] _ram;
        private readonly uint _codeSectionEnd;

        public Memory(uint ramSize, uint codeSectionEnd)
        {
            if (codeSectionEnd > ramSize)
                throw new ArgumentException("Code section cannot be bigger than ram size");

            _ram = new byte[ramSize];
            _codeSectionEnd = codeSectionEnd;
        }

        public int GetRamSize()
        {
            return _ram.Length - 0x8;
        }

        public void LoadSource(List<int> source)
        {
            var data = source.SelectMany(BitConverter.GetBytes).ToArray();

            if (data.Length > _codeSectionEnd)
                throw new IndexOutOfRangeException("Code section is too small to load the source");

            Array.Copy(data, 0x0, _ram, 0x0, data.Length);
        }

        public void WriteInt(uint address, int data)
        {
            Write(address, BitConverter.GetBytes(data));
        }

        public void WriteByte(uint address, byte data)
        {
            Write(address, BitConverter.GetBytes(data));
        }

        private void Write(uint address, byte[] data)
        {
            if (address < _codeSectionEnd)
                throw new AccessViolationException("Cannot write to the code section");

            if (address + data.Length >= _ram.Length)
                throw new AccessViolationException("Memory out of range requested");

            Array.Copy(data, 0, _ram, address, data.Length);
        }

        public int ReadInt(uint address)
        {
            return BitConverter.ToInt32(Read(address, 4), 0);
        }

        public byte ReadByte(uint address)
        {
            return Read(address, 1)[0];
        }

        private byte[] Read(uint address, int length)
        {
            if (address > _ram.Length)
                throw new IndexOutOfRangeException("Undefined memory location");

            var buffer = new byte[length];
            Array.Copy(_ram, address, buffer, 0, buffer.Length);

            return buffer;
        }
    }
}
