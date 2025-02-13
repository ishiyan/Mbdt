// LzmaBase.cs

namespace SevenZip.Compression.LZMA
{
    internal abstract class Base
    {
        public const uint KNumStates = 12;

        public struct State
        {
            public uint Index;
            public void Init() { Index = 0; }
            public void UpdateChar()
            {
                if (Index < 4) Index = 0;
                else if (Index < 10) Index -= 3;
                else Index -= 6;
            }
            public void UpdateMatch() { Index = (uint)(Index < 7 ? 7 : 10); }
            public void UpdateRep() { Index = (uint)(Index < 7 ? 8 : 11); }
            public void UpdateShortRep() { Index = (uint)(Index < 7 ? 9 : 11); }
            public bool IsCharState() { return Index < 7; }
        }

        public const int KNumPosSlotBits = 6;

        private const int kNumLenToPosStatesBits = 2; // it's for speed optimization
        public const uint KNumLenToPosStates = 1 << kNumLenToPosStatesBits;

        public const uint KMatchMinLen = 2;

        public static uint GetLenToPosState(uint len)
        {
            len -= KMatchMinLen;
            if (len < KNumLenToPosStates)
                return len;
            return KNumLenToPosStates - 1;
        }

        public const int KNumAlignBits = 4;

        public const uint KStartPosModelIndex = 4;
        public const uint KEndPosModelIndex = 14;

        public const uint KNumFullDistances = 1 << ((int)KEndPosModelIndex / 2);

        public const int KNumPosStatesBitsMax = 4;
        public const uint KNumPosStatesMax = (1 << KNumPosStatesBitsMax);

        public const int KNumLowLenBits = 3;
        public const int KNumMidLenBits = 3;
        public const int KNumHighLenBits = 8;
        public const uint KNumLowLenSymbols = 1 << KNumLowLenBits;
        public const uint KNumMidLenSymbols = 1 << KNumMidLenBits;
    }
}
