using System;

namespace ARM_Simulator.Utilitiy
{
    internal class BitWriter
    {
        private int _bitValue;

        public BitWriter()
        {
            _bitValue = 0;
        }

        public void WriteBits(int value, int start, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var valueBit = value & (1 << i);
                if (valueBit != 0)
                    _bitValue = _bitValue | 1 << (start + i);
            }
        }

        public byte[] GetValue() => BitConverter.GetBytes(_bitValue);
    }
}
