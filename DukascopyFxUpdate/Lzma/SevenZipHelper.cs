using System;
using System.IO;


namespace SevenZip.Compression.LZMA
{
    public static class SevenZipHelper
    {
        public static byte[] Decompress(byte[] inputBytes)
        {
            var newInStream = new MemoryStream(inputBytes);
            var decoder = new Decoder();
            newInStream.Seek(0, 0);
            var newOutStream = new MemoryStream();
            var properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);
            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);
            byte[] b = newOutStream.ToArray();
            return b;
        }
    }
}
