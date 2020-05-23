using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

using Mbh5;

namespace mbdt.DukascopyFxUpdate
{
    internal static class DukascopyFxImport
    {
        internal static bool DoImport(string directoryOrFile, bool candles, int debugTraceLevel)
        {
            bool status = true;
            try
            {
                Import(directoryOrFile, candles, debugTraceLevel);
            }
            catch
            {
                status = false;
            }
            return status;
        }

        private static void Import(string directoryOrFile, bool candles, int debugTraceLevel)
        {
            if (!Directory.Exists(DukascopyFxContext.RepositoryPath))
                Directory.CreateDirectory(DukascopyFxContext.RepositoryPath);
            var quoteList = new List<Quote>(1024);
            var ohlcvList = new List<Ohlcv>(1024);
            TraverseTree(directoryOrFile, s =>
            {
                var fi = new FileInfo(s);
                if (0 == fi.Length)
                    Trace.TraceInformation("Zero length file [{0}], skipping", fi.FullName);
                else if (s.Contains("h_ticks.bi"))
                {
                    quoteList.Clear();
                    string instrumentName = FxInstrumentName(fi.DirectoryName);
                    if (null != instrumentName)
                    {
                        Trace.TraceInformation("Parsing [{0}] ticks from [{1}]", instrumentName, fi.FullName);
                        ParseFile(s, quoteList, fi.FullName, instrumentName);
                        string h5FilePath = string.Concat(DukascopyFxContext.RepositoryPath, "\\", instrumentName, ".h5");
                        string h5InstrumentPath = string.Concat("/dukascopy/fx/", instrumentName);
                        int count = quoteList.Count;
                        if (0 < count)
                        {
                            Trace.TraceInformation("Importing [{0}] parsed ticks to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                            using (Repository repository = Repository.OpenReadWrite(h5FilePath, true, Properties.Settings.Default.CorkTheCache))
                            {
                                if (null == repository)
                                    Trace.TraceError("Failed to open [{0}]", h5FilePath);
                                else
                                {
                                    using (Instrument instrument = repository.Open(h5InstrumentPath, true))
                                    {
                                        if (null == instrument)
                                            Trace.TraceError("Failed to open [{0}] in [{1}]", instrumentName, h5FilePath);
                                        else
                                        {
                                            using (QuoteData data = instrument.OpenQuote(true))
                                            {
                                                if (null == data)
                                                    Trace.TraceError("Failed to open quote data in [{0}] in [{1}]", instrumentName, h5FilePath);
                                                else
                                                {
                                                    data.SpreadDuplicateTimeTicks(quoteList, true);
                                                    if (!data.Add(quoteList, DuplicateTimeTicks.Skip, true))
                                                        Trace.TraceError("Error importing [{0}] parsed ticks to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                            Trace.TraceWarning("No [{0}] ticks parsed from [{1}]", instrumentName, fi.FullName);
                        if (0 < count && 0 < debugTraceLevel)
                        {
                            if (1 == debugTraceLevel)
                            {
                                Quote q = quoteList[0];
                                Trace.WriteLine(
                                    $"{new DateTime(q.dateTimeTicks)} ms {new DateTime(q.dateTimeTicks).Millisecond}, ask {q.askPrice}, {q.askSize}, bid {q.bidPrice}, {q.bidSize}");
                                q = quoteList[count - 1];
                                Trace.WriteLine($"... {count - 2} ...");
                                Trace.WriteLine(
                                    $"{new DateTime(q.dateTimeTicks)} ms {new DateTime(q.dateTimeTicks).Millisecond}, ask {q.askPrice}, {q.askSize}, bid {q.bidPrice}, {q.bidSize}");
                            }
                            else if (2 <= debugTraceLevel)
                            {
                                foreach (var v in quoteList)
                                {
                                    Trace.WriteLine(
                                        $"{new DateTime(v.dateTimeTicks)} ms {new DateTime(v.dateTimeTicks).Millisecond}, ask {v.askPrice}, {v.askSize}, bid {v.bidPrice}, {v.bidSize}");
                                }
                            }
                        }
                    }
                }
                else if (candles)
                {
                    var timeFrame = DataTimeFrame.Aperiodic;
                    var kind = OhlcvKind.Ask;
                    if (s.EndsWith("ASK_candles_min_1.bin"))
                        timeFrame = DataTimeFrame.Minute1;
                    else if (s.EndsWith("BID_candles_min_1.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute1;
                    }
                    else if (s.EndsWith("ASK_candles_hour_1.bin"))
                        timeFrame = DataTimeFrame.Hour1;
                    else if (s.EndsWith("BID_candles_hour_1.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Hour1;
                    }
                    else if (s.EndsWith("ASK_candles_day_1.bin"))
                        timeFrame = DataTimeFrame.Day1;
                    else if (s.EndsWith("BID_candles_day_1.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Day1;
                    }
                    if (DataTimeFrame.Aperiodic == timeFrame)
                        Trace.TraceWarning("Unknown filename [{0}], skipping", fi.FullName);
                    else
                    {
                        ohlcvList.Clear();
                        string instrumentName = FxInstrumentName(fi.DirectoryName);
                        if (null != instrumentName)
                        {
                            Trace.TraceInformation("Parsing [{0}] ohlcvs from [{1}]", instrumentName, fi.FullName);
                            ParseFile(s, ohlcvList, fi.FullName, instrumentName);
                            string h5FilePath = string.Concat(DukascopyFxContext.RepositoryPath, "\\", instrumentName, ".h5");
                            string h5InstrumentPath = string.Concat("/dukascopy/fx/", instrumentName);
                            int count = ohlcvList.Count;
                            if (0 < count)
                            {
                                Trace.TraceInformation("Importing [{0}] parsed ohlcvs to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                using (Repository repository = Repository.OpenReadWrite(h5FilePath, true, Properties.Settings.Default.CorkTheCache))
                                {
                                    if (null == repository)
                                        Trace.TraceError("Failed to open [{0}]", h5FilePath);
                                    else
                                    {
                                        using (Instrument instrument = repository.Open(h5InstrumentPath, true))
                                        {
                                            if (null == instrument)
                                                Trace.TraceError("Failed to open [{0}] in [{1}]", instrumentName, h5FilePath);
                                            else
                                            {
                                                using (OhlcvData data = instrument.OpenOhlcv(kind, timeFrame, true))
                                                {
                                                    if (null == data)
                                                        Trace.TraceError("Failed to open ohlcv data in [{0}] in [{1}]", instrumentName, h5FilePath);
                                                    else
                                                    {
                                                        data.SpreadDuplicateTimeTicks(ohlcvList, true);
                                                        if (!data.Add(ohlcvList, DuplicateTimeTicks.Skip, true))
                                                            Trace.TraceError("Error importing [{0}] parsed ohlcvs to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                                Trace.TraceWarning("No [{0}] ohlcvs parsed from [{1}]", instrumentName, fi.FullName);
                            if (0 < count && 0 < debugTraceLevel)
                            {
                                if (1 == debugTraceLevel)
                                {
                                    Ohlcv o = ohlcvList[0];
                                    Trace.WriteLine(
                                        $"{new DateTime(o.dateTimeTicks)} ms {new DateTime(o.dateTimeTicks).Millisecond}, open {o.open}, high {o.high}, low {o.low}, close {o.close}, volume {o.volume}");
                                    o = ohlcvList[count - 1];
                                    Trace.WriteLine($"... {count - 2} ...");
                                    Trace.WriteLine(
                                        $"{new DateTime(o.dateTimeTicks)} ms {new DateTime(o.dateTimeTicks).Millisecond}, open {o.open}, high {o.high}, low {o.low}, close {o.close}, volume {o.volume}");
                                }
                                else if (2 <= debugTraceLevel)
                                {
                                    foreach (var o in ohlcvList)
                                    {
                                        Trace.WriteLine(
                                            $"{new DateTime(o.dateTimeTicks)} ms {new DateTime(o.dateTimeTicks).Millisecond}, open {o.open}, high {o.high}, low {o.low}, close {o.close}, volume {o.volume}");
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                {
                    try
                    {
                        action(entry);
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceError("Traverse action exception: {0}", exception.Message);
                    }
                }
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
            {
                try
                {
                    action(root);
                }
                catch (Exception exception)
                {
                    Trace.TraceError("Traverse action exception: {0}", exception.Message);
                }
            }
        }

        private static string FxInstrumentName(string directory)
        {
            foreach (var v in DukascopyFxContext.Symbols)
            {
                if (directory.Contains(v))
                    return v;
            }
            Trace.TraceError(string.Concat("No FX instrument name found in the path: [", directory, "]"));
            return null;
        }

        private static DateTime FxInstrumentBaseDateTime(string fullPath, string pair, out double factor)
        {
            if (pair.Contains("JPY") || pair.Contains("RUB") || pair.Contains("XAGUSD") || pair.Contains("XAUUSD"))
                factor = 0.001;
            else
                factor = 0.00001;
            pair = string.Concat("\\", pair, "\\");
            int i = fullPath.LastIndexOf(pair, StringComparison.Ordinal);
            if (i < 0)
                throw new ArgumentException(string.Concat("No FX instrument [", pair, "] found in the path: [", fullPath, "]"));
            string s = fullPath.Substring(i + pair.Length);
            if (!int.TryParse(s.Substring(0, 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out int year))
                throw new ArgumentException(string.Concat("No FX instrument [", pair, "] year found in the path: [", fullPath, "]"));
            s = s.Substring(5);
            if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out int month))
                throw new ArgumentException(string.Concat("No FX instrument [", pair, "] month found in the path: [", fullPath, "]"));
            ++month;
            s = s.Substring(3);
            if (s.Contains("\\"))
            {
                if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out int day))
                    throw new ArgumentException(string.Concat("No FX instrument [", pair, "] day found in the path: [", fullPath, "]"));
                s = s.Substring(3);
                if (s.Contains("_ticks."))
                {
                    if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out int hour))
                        throw new ArgumentException(string.Concat("No FX instrument [", pair, "] hour found in the path: [", fullPath, "]"));
                    return new DateTime(year, month, day, hour, 0, 0);
                }
                return new DateTime(year, month, day);
            }
            return new DateTime(year, month, 1);
        }

        internal static bool IsOldFormat;
        private static void ParseFile(string zipFile, List<Quote> list, string fullPath, string pair)
        {
            var mbQuote = new Quote();
            if (IsOldFormat)
            {
                try
                {
                    // Old format first.
                    using (MemoryStream stream = UnzippedFirstEntry(zipFile))
                    {
                        if (null != stream)
                        {
                            byte[] bytes = stream.ToArray();
                            long length = stream.Length, offset = 0;
                            while (offset < length)
                            {
                                mbQuote.dateTimeTicks = ExtractDateTime(bytes, ref offset);
                                mbQuote.askPrice = ExtractDouble(bytes, ref offset);
                                mbQuote.bidPrice = ExtractDouble(bytes, ref offset);
                                mbQuote.askSize = ExtractDouble(bytes, ref offset);
                                mbQuote.bidSize = ExtractDouble(bytes, ref offset);
                                list.Add(mbQuote);
                            }
                        }
                    }
                }
                catch
                {
                    const double factorSize = 100000;
                    DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out double factorPrice);
                    byte[] input = LzmaFirstEntry(zipFile);
                    byte[] bytes = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(input);
                    long length = bytes.Length, offset = 0;
                    while (offset < length)
                    {
                        mbQuote.dateTimeTicks = dateTime.AddMilliseconds(ExtractDateTime4(bytes, ref offset)).Ticks;
                        mbQuote.askPrice = factorPrice * ExtractInt4(bytes, ref offset);
                        mbQuote.bidPrice = factorPrice * ExtractInt4(bytes, ref offset);
                        mbQuote.askSize = Math.Round(factorSize * ExtractDouble4(bytes, ref offset));
                        mbQuote.bidSize = Math.Round(factorSize * ExtractDouble4(bytes, ref offset));
                        list.Add(mbQuote);
                    }
                }
            }
            else
            {
                const double factorSize = 100000;
                DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out double factorPrice);
                byte[] input = LzmaFirstEntry(zipFile);
                byte[] bytes = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(input);
                long length = bytes.Length, offset = 0;
                while (offset < length)
                {
                    mbQuote.dateTimeTicks = dateTime.AddMilliseconds(ExtractDateTime4(bytes, ref offset)).Ticks;
                    mbQuote.askPrice = factorPrice * ExtractInt4(bytes, ref offset);
                    mbQuote.bidPrice = factorPrice * ExtractInt4(bytes, ref offset);
                    mbQuote.askSize = Math.Round(factorSize * ExtractDouble4(bytes, ref offset));
                    mbQuote.bidSize = Math.Round(factorSize * ExtractDouble4(bytes, ref offset));
                    list.Add(mbQuote);
                }
            }
        }

        private static void ParseFile(string zipFile, List<Ohlcv> list, string fullPath, string pair)
        {
            var mbOhlcv = new Ohlcv();
            if (IsOldFormat)
            {
                try
                {
                    // Old format first.
                    using (MemoryStream stream = UnzippedFirstEntry(zipFile))
                    {
                        if (null != stream)
                        {
                            byte[] bytes = stream.ToArray();
                            long length = stream.Length, offset = 0;
                            while (offset < length)
                            {
                                mbOhlcv.dateTimeTicks = ExtractDateTime(bytes, ref offset);
                                mbOhlcv.open = ExtractDouble(bytes, ref offset);
                                mbOhlcv.close = ExtractDouble(bytes, ref offset);
                                mbOhlcv.low = ExtractDouble(bytes, ref offset);
                                mbOhlcv.high = ExtractDouble(bytes, ref offset);
                                mbOhlcv.volume = ExtractDouble(bytes, ref offset);
                                if (Math.Abs(0.0 - mbOhlcv.volume) > 1e-8)
                                    list.Add(mbOhlcv);
                            }
                        }
                    }
                }
                catch
                {
                    // const double factorVolume = 100000;
                    DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out double factorPrice);
                    byte[] input = LzmaFirstEntry(zipFile);
                    byte[] bytes = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(input);
                    long length = bytes.Length, offset = 0;
                    while (offset < length)
                    {
                        mbOhlcv.dateTimeTicks = dateTime.AddSeconds(ExtractDateTime4(bytes, ref offset)).Ticks;
                        mbOhlcv.open = factorPrice * ExtractInt4(bytes, ref offset);
                        mbOhlcv.close = factorPrice * ExtractInt4(bytes, ref offset);
                        mbOhlcv.low = factorPrice * ExtractInt4(bytes, ref offset);
                        mbOhlcv.high = factorPrice * ExtractInt4(bytes, ref offset);
                        // mbOhlcv.volume = Math.Round(factorVolume * ExtractDouble4(bytes, ref offset));
                        mbOhlcv.volume = Math.Round(ExtractDouble4(bytes, ref offset), 2);
                        if (Math.Abs(0.0 - mbOhlcv.volume) > 1e-8)
                            list.Add(mbOhlcv);
                    }
                }
            }
            else
            {
                // const double factorVolume = 100000;
                DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out double factorPrice);
                byte[] input = LzmaFirstEntry(zipFile);
                byte[] bytes = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(input);
                long length = bytes.Length, offset = 0;
                while (offset < length)
                {
                    mbOhlcv.dateTimeTicks = dateTime.AddSeconds(ExtractDateTime4(bytes, ref offset)).Ticks;
                    mbOhlcv.open = factorPrice * ExtractInt4(bytes, ref offset);
                    mbOhlcv.close = factorPrice * ExtractInt4(bytes, ref offset);
                    mbOhlcv.low = factorPrice * ExtractInt4(bytes, ref offset);
                    mbOhlcv.high = factorPrice * ExtractInt4(bytes, ref offset);
                    // mbOhlcv.volume = Math.Round(factorVolume * ExtractDouble4(bytes, ref offset));
                    mbOhlcv.volume = Math.Round(ExtractDouble4(bytes, ref offset), 2);
                    if (Math.Abs(0.0 - mbOhlcv.volume) > 1e-8)
                        list.Add(mbOhlcv);
                }
            }
        }

        private static byte[] LzmaFirstEntry(string lzmaFile)
        {
            if (!File.Exists(lzmaFile))
            {
                Trace.TraceError(string.Concat("Lzma file does not exist: ", lzmaFile));
                return null;
            }
            var fileInfo = new FileInfo(lzmaFile);
            if (0 == fileInfo.Length)
            {
                Trace.TraceInformation(string.Concat("Lzma file has zero length: ", lzmaFile));
                return null;
            }
            var fileStream = new FileStream(lzmaFile, FileMode.Open, FileAccess.Read);
            var data = new byte[fileStream.Length];
            fileStream.Read(data, 0, Convert.ToInt32(fileStream.Length));
            fileStream.Close();
            return data;
        }

        private static MemoryStream UnzippedFirstEntry(string zipFile)
        {
            if (!File.Exists(zipFile))
            {
                Trace.TraceError(string.Concat("Zip file does not exist: ", zipFile));
                return null;
            }
            var fileInfo = new FileInfo(zipFile);
            if (0 == fileInfo.Length)
            {
                Trace.TraceInformation(string.Concat("Zip file has zero length: ", zipFile));
                return null;
            }
            ZipStorer zip = ZipStorer.Open(zipFile, FileAccess.Read);
            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
            if (1 != dir.Count)
            {
                zip.Close();
                Trace.TraceError(string.Concat("Zip file does not contain the only entry: ", zipFile));
                return null;
            }
            var stream = new MemoryStream(1024 * 256);
            zip.ExtractFile(dir[0], stream);
            zip.Close();
            return stream;
        }

        private static readonly DateTime StartOfEpoch = new DateTime(1970, 1, 1);
        private static long ExtractDateTime(byte[] bytes, ref long offset)
        {
            offset += 8;
            long off = offset;
            return StartOfEpoch.AddMilliseconds(
                ((bytes[--off] & 0xFFL) << 0) +
                ((bytes[--off] & 0xFFL) << 8) +
                ((bytes[--off] & 0xFFL) << 16) +
                ((bytes[--off] & 0xFFL) << 24) +
                ((bytes[--off] & 0xFFL) << 32) +
                ((bytes[--off] & 0xFFL) << 40) +
                ((bytes[--off] & 0xFFL) << 48) +
                (((long)bytes[--off]) << 56)).Ticks;
        }

        private static double ExtractDouble(byte[] bytes, ref long offset)
        {
            offset += 8;
            long off = offset;
            return BitConverter.Int64BitsToDouble(
                ((bytes[--off] & 0xFFL) << 0) +
                ((bytes[--off] & 0xFFL) << 8) +
                ((bytes[--off] & 0xFFL) << 16) +
                ((bytes[--off] & 0xFFL) << 24) +
                ((bytes[--off] & 0xFFL) << 32) +
                ((bytes[--off] & 0xFFL) << 40) +
                ((bytes[--off] & 0xFFL) << 48) +
                (((long)bytes[--off]) << 56));
        }

        private static float ExtractDouble4(byte[] bytes, ref long offset)
        {
            offset += 4;
            long off = offset;
            var b4 = new byte[4];
            b4[0] = bytes[--off];
            b4[1] = bytes[--off];
            b4[2] = bytes[--off];
            b4[3] = bytes[--off];
            return BitConverter.ToSingle(b4, 0);
        }

        private static int ExtractInt4(byte[] bytes, ref long offset)
        {
            offset += 4;
            long off = offset;
            var b4 = new byte[4];
            b4[0] = bytes[--off];
            b4[1] = bytes[--off];
            b4[2] = bytes[--off];
            b4[3] = bytes[--off];
            return BitConverter.ToInt32(b4, 0);
        }

        private static long ExtractDateTime4(byte[] bytes, ref long offset)
        {
            offset += 4;
            long off = offset;
            return ((bytes[--off] & 0xFFL) << 0) +
                ((bytes[--off] & 0xFFL) << 8) +
                ((bytes[--off] & 0xFFL) << 16) +
                ((bytes[--off] & 0xFFL) << 24);
        }
    }
}
