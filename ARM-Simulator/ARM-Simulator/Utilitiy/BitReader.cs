namespace ARM_Simulator.Utilitiy
{
    internal class BitReader
    {
        private readonly int _bitValue;

        public BitReader(int bitValue)
        {
            _bitValue = bitValue;
        }

        public int ReadBits(int start, int count, bool signed = false)
        {
            var readerBits = 0;

            for (var i = 0; i < count; i++)
            {
                var valueBit = _bitValue & (1 << (start + i));
                if (valueBit != 0)
                    readerBits = readerBits | (1 << i);
            }

            if (!signed)
                return readerBits;

            if ((readerBits & (1 << (count -1))) != 0)
                readerBits -= 1 << count;

            return readerBits;
        }

    }
}
