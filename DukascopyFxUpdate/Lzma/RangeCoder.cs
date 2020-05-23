using System;

namespace SevenZip.Compression.RangeCoder
{
    class Encoder
    {
        public const uint KTopValue = (1 << 24);

        System.IO.Stream Stream;

        public UInt64 Low;
        public uint Range;
        uint cacheSize;
        byte cache;

        long StartPosition;

        public void SetStream(System.IO.Stream stream)
        {
            Stream = stream;
        }

        public void ReleaseStream()
        {
            Stream = null;
        }

        public void Init()
        {
            StartPosition = Stream.Position;

            Low = 0;
            Range = 0xFFFFFFFF;
            cacheSize = 1;
            cache = 0;
        }

        public void FlushData()
        {
            for (int i = 0; i < 5; i++)
                ShiftLow();
        }

        public void FlushStream()
        {
            Stream.Flush();
        }

        public void ShiftLow()
        {
            if ((uint)Low < 0xFF000000 || (uint)(Low >> 32) == 1)
            {
                byte temp = cache;
                do
                {
                    Stream.WriteByte((byte)(temp + (Low >> 32)));
                    temp = 0xFF;
                }
                while (--cacheSize != 0);
                cache = (byte)(((uint)Low) >> 24);
            }
            cacheSize++;
            Low = ((uint)Low) << 8;
        }

        public void EncodeDirectBits(uint v, int numTotalBits)
        {
            for (int i = numTotalBits - 1; i >= 0; i--)
            {
                Range >>= 1;
                if (((v >> i) & 1) == 1)
                    Low += Range;
                if (Range < KTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }
        }

        public long GetProcessedSizeAdd()
        {
            return cacheSize +
                Stream.Position - StartPosition + 4;
            // (long)Stream.GetProcessedSize();
        }
    }

    class Decoder
    {
        public const uint KTopValue = (1 << 24);
        public uint Range;
        public uint Code;
        // public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
        public System.IO.Stream Stream;

        public void Init(System.IO.Stream stream)
        {
            // Stream.Init(stream);
            Stream = stream;

            Code = 0;
            Range = 0xFFFFFFFF;
            for (int i = 0; i < 5; i++)
                Code = (Code << 8) | (byte)Stream.ReadByte();
        }

        public void ReleaseStream()
        {
            // Stream.ReleaseStream();
            Stream = null;
        }

        public uint DecodeDirectBits(int numTotalBits)
        {
            uint range = Range;
            uint code = Code;
            uint result = 0;
            for (int i = numTotalBits; i > 0; i--)
            {
                range >>= 1;
                uint t = (code - range) >> 31;
                code -= range & (t - 1);
                result = (result << 1) | (1 - t);

                if (range < KTopValue)
                {
                    code = (code << 8) | (byte)Stream.ReadByte();
                    range <<= 8;
                }
            }
            Range = range;
            Code = code;
            return result;
        }
    }
}
