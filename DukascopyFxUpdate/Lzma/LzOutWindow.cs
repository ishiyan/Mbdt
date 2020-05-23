// LzOutWindow.cs

namespace SevenZip.Compression.LZ
{
    public class OutWindow
    {
        byte[] buffer;
        uint pos;
        uint windowSize;
        uint streamPos;
        System.IO.Stream stream;

        public void Create(uint windowSiz)
        {
            if (windowSize != windowSiz)
            {
                // System.GC.Collect();
                buffer = new byte[windowSiz];
            }
            windowSize = windowSiz;
            pos = 0;
            streamPos = 0;
        }

        private void Init(System.IO.Stream strm, bool solid)
        {
            ReleaseStream();
            stream = strm;
            if (!solid)
            {
                streamPos = 0;
                pos = 0;
            }
        }

        public void Init(System.IO.Stream strm) { Init(strm, false); }

        public void ReleaseStream()
        {
            Flush();
            stream = null;
        }

        public void Flush()
        {
            uint size = pos - streamPos;
            if (size == 0)
                return;
            stream.Write(buffer, (int)streamPos, (int)size);
            if (pos >= windowSize)
                pos = 0;
            streamPos = pos;
        }

        public void CopyBlock(uint distance, uint len)
        {
            uint poz = pos - distance - 1;
            if (poz >= windowSize)
                poz += windowSize;
            for (; len > 0; len--)
            {
                if (poz >= windowSize)
                    poz = 0;
                buffer[pos++] = buffer[poz++];
                if (pos >= windowSize)
                    Flush();
            }
        }

        public void PutByte(byte b)
        {
            buffer[pos++] = b;
            if (pos >= windowSize)
                Flush();
        }

        public byte GetByte(uint distance)
        {
            uint poz = pos - distance - 1;
            if (poz >= windowSize)
                poz += windowSize;
            return buffer[poz];
        }
    }
}
