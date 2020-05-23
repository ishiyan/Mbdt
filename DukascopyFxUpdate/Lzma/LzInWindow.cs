// LzInWindow.cs

using System;

namespace SevenZip.Compression.LZ
{
    public class InWindow
    {
        protected Byte[] bufferBase; // pointer to buffer with data
        System.IO.Stream stream;
        UInt32 posLimit; // offset (from _buffer) of first byte when new block reading must be done
        bool streamEndWasReached; // if (true) then _streamPos shows real end of stream

        UInt32 pointerToLastSafePosition;

        protected UInt32 bufferOffset;

        private UInt32 blockSize; // Size of Allocated memory block
        protected UInt32 pos; // offset (from _buffer) of curent byte
        UInt32 keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        UInt32 keepSizeAfter; // how many BYTEs must be kept buffer after _pos
        protected UInt32 streamPos; // offset (from _buffer) of first not read byte from Stream

        private void MoveBlock()
        {
            UInt32 offset = bufferOffset + pos - keepSizeBefore;
            // we need one additional byte, since MovePos moves on 1 byte.
            if (offset > 0)
                offset--;

            UInt32 numBytes = bufferOffset + streamPos - offset;

            // check negative offset ????
            for (UInt32 i = 0; i < numBytes; i++)
                bufferBase[i] = bufferBase[offset + i];
            bufferOffset -= offset;
        }

        private void ReadBlock()
        {
            if (streamEndWasReached)
                return;
            while (true)
            {
                var size = (int)((0 - bufferOffset) + blockSize - streamPos);
                if (size == 0)
                    return;
                int numReadBytes = stream.Read(bufferBase, (int)(bufferOffset + streamPos), size);
                if (numReadBytes == 0)
                {
                    posLimit = streamPos;
                    UInt32 pointerToPostion = bufferOffset + posLimit;
                    if (pointerToPostion > pointerToLastSafePosition)
                        posLimit = pointerToLastSafePosition - bufferOffset;

                    streamEndWasReached = true;
                    return;
                }
                streamPos += (UInt32)numReadBytes;
                if (streamPos >= pos + keepSizeAfter)
                    posLimit = streamPos - keepSizeAfter;
            }
        }

        void Free() { bufferBase = null; }

        protected void Create(UInt32 keepSizBefore, UInt32 keepSizAfter, UInt32 keepSizeReserv)
        {
            keepSizeBefore = keepSizBefore;
            keepSizeAfter = keepSizAfter;
            UInt32 blockSiz = keepSizBefore + keepSizAfter + keepSizeReserv;
            if (bufferBase == null || blockSize != blockSiz)
            {
                Free();
                blockSize = blockSiz;
                bufferBase = new Byte[blockSize];
            }
            pointerToLastSafePosition = blockSize - keepSizAfter;
        }

        protected void SetStream(System.IO.Stream strm) { stream = strm; }
        protected void ReleaseStream() { stream = null; }

        protected void Init()
        {
            bufferOffset = 0;
            pos = 0;
            streamPos = 0;
            streamEndWasReached = false;
            ReadBlock();
        }

        protected void MovePos()
        {
            pos++;
            if (pos > posLimit)
            {
                UInt32 pointerToPostion = bufferOffset + pos;
                if (pointerToPostion > pointerToLastSafePosition)
                    MoveBlock();
                ReadBlock();
            }
        }

        protected Byte GetIndexByte(Int32 index) { return bufferBase[bufferOffset + pos + index]; }

        // index + limit have not to exceed _keepSizeAfter;
        protected UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit)
        {
            if (streamEndWasReached)
                if ((pos + index) + limit > streamPos)
                    limit = streamPos - (UInt32)(pos + index);
            distance++;
            // Byte *pby = _buffer + (size_t)_pos + index;
            UInt32 pby = bufferOffset + pos + (UInt32)index;

            UInt32 i;
            for (i = 0; i < limit && bufferBase[pby + i] == bufferBase[pby + i - distance]; i++)
            { }
            return i;
        }

        protected UInt32 GetNumAvailableBytes() { return streamPos - pos; }

        protected void ReduceOffsets(Int32 subValue)
        {
            bufferOffset += (UInt32)subValue;
            posLimit -= (UInt32)subValue;
            pos -= (UInt32)subValue;
            streamPos -= (UInt32)subValue;
        }
    }
}
