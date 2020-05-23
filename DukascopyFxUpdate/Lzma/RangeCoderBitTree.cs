using System;

namespace SevenZip.Compression.RangeCoder
{
    struct BitTreeEncoder
    {
        readonly BitEncoder[] Models;
        readonly int NumBitLevels;

        public BitTreeEncoder(int numBitLevels)
        {
            NumBitLevels = numBitLevels;
            Models = new BitEncoder[1 << numBitLevels];
        }

        public void Init()
        {
            for (uint i = 1; i < (1 << NumBitLevels); i++)
                Models[i].Init();
        }

        public void Encode(Encoder rangeEncoder, UInt32 symbol)
        {
            UInt32 m = 1;
            for (int bitIndex = NumBitLevels; bitIndex > 0; )
            {
                bitIndex--;
                UInt32 bit = (symbol >> bitIndex) & 1;
                Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
            }
        }

        public void ReverseEncode(Encoder rangeEncoder, UInt32 symbol)
        {
            UInt32 m = 1;
            for (UInt32 i = 0; i < NumBitLevels; i++)
            {
                UInt32 bit = symbol & 1;
                Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        public UInt32 GetPrice(UInt32 symbol)
        {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int bitIndex = NumBitLevels; bitIndex > 0; )
            {
                bitIndex--;
                UInt32 bit = (symbol >> bitIndex) & 1;
                price += Models[m].GetPrice(bit);
                m = (m << 1) + bit;
            }
            return price;
        }

        public UInt32 ReverseGetPrice(UInt32 symbol)
        {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int i = NumBitLevels; i > 0; i--)
            {
                UInt32 bit = symbol & 1;
                symbol >>= 1;
                price += Models[m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static UInt32 ReverseGetPrice(BitEncoder[] models, UInt32 startIndex,
            int numBitLevels, UInt32 symbol)
        {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int i = numBitLevels; i > 0; i--)
            {
                UInt32 bit = symbol & 1;
                symbol >>= 1;
                price += models[startIndex + m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static void ReverseEncode(BitEncoder[] models, UInt32 startIndex,
            Encoder rangeEncoder, int numBitLevels, UInt32 symbol)
        {
            UInt32 m = 1;
            for (int i = 0; i < numBitLevels; i++)
            {
                UInt32 bit = symbol & 1;
                models[startIndex + m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }
    }

    struct BitTreeDecoder
    {
        readonly BitDecoder[] Models;
        readonly int NumBitLevels;

        public BitTreeDecoder(int numBitLevels)
        {
            NumBitLevels = numBitLevels;
            Models = new BitDecoder[1 << numBitLevels];
        }

        public void Init()
        {
            for (uint i = 1; i < (1 << NumBitLevels); i++)
                Models[i].Init();
        }

        public uint Decode(Decoder rangeDecoder)
        {
            uint m = 1;
            for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
                m = (m << 1) + Models[m].Decode(rangeDecoder);
            return m - ((uint)1 << NumBitLevels);
        }

        public uint ReverseDecode(Decoder rangeDecoder)
        {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
            {
                uint bit = Models[m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= (bit << bitIndex);
            }
            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] models, UInt32 startIndex,
            Decoder rangeDecoder, int numBitLevels)
        {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
            {
                uint bit = models[startIndex + m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= (bit << bitIndex);
            }
            return symbol;
        }
    }
}
