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
        private static readonly Dictionary<string, Repository> fileDictionary = new Dictionary<string, Repository>(128);

        static internal bool DoCleanup()
        {
            bool status = true;
            foreach (var v in fileDictionary)
            {
                try
                {
                    v.Value.Close();
                }
                catch
                {
                    status = false;
                }
            }
            return status;
        }

        static internal bool DoImport(string directoryOrFile, bool candles, int debugTraceLevel)
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

        static private void Import(string directoryOrFile, bool candles, int debugTraceLevel)
        {
            if (!Directory.Exists(DukascopyFxContext.RepositoryPath))
                Directory.CreateDirectory(DukascopyFxContext.RepositoryPath);
            var quoteList = new List<Quote>(1024);
            var ohlcvList = new List<Ohlcv>(1024);
            bool alwaysCloseFile = Properties.Settings.Default.AlwaysCloseFile;
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
                            Repository repository;
                            if (!fileDictionary.TryGetValue(h5FilePath, out repository))
                            {
                                repository = Repository.OpenReadWrite(h5FilePath, true, Properties.Settings.Default.CorkTheCache);
                                if (null != repository && !alwaysCloseFile)
                                    fileDictionary.Add(h5FilePath, repository);
                            }
                            if (null == repository)
                                Trace.TraceError("Failed to open [{0}]", h5FilePath);
                            else
                            {
                                Instrument instrument = repository.Open(h5InstrumentPath, true);
                                if (null == instrument)
                                    Trace.TraceError("Failed to open [{0}] in [{1}]", instrumentName, h5FilePath);
                                else
                                {
                                    QuoteData data = instrument.OpenQuote(true);
                                    if (null == data)
                                        Trace.TraceError("Failed to open quote data in [{0}] in [{1}]", instrumentName, h5FilePath);
                                    else
                                    {
                                        data.SpreadDuplicateTimeTicks(quoteList, true);
                                        if (!data.Add(quoteList, DuplicateTimeTicks.Skip, true))
                                            Trace.TraceError("Error importing [{0}] parsed ticks to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                        data.Flush();
                                        if (alwaysCloseFile)
                                            data.Close();
                                    }
                                    if (alwaysCloseFile)
                                        instrument.Close();
                                }
                                if (alwaysCloseFile)
                                    repository.Close();
                            }
                        }
                        else
                            Trace.TraceWarning("No [{0}] ticks parsed from [{1}]", instrumentName, fi.FullName);
                        if (0 < count && 0 < debugTraceLevel)
                        {
                            if (1 == debugTraceLevel)
                            {
                                Quote q = quoteList[0];
                                Trace.WriteLine(string.Format("{0} ms {1}, ask {2}, {3}, bid {4}, {5}", new DateTime(q.dateTimeTicks), new DateTime(q.dateTimeTicks).Millisecond, q.askPrice, q.askSize, q.bidPrice, q.bidSize));
                                q = quoteList[count - 1];
                                Trace.WriteLine(string.Format("... {0} ...", count - 2));
                                Trace.WriteLine(string.Format("{0} ms {1}, ask {2}, {3}, bid {4}, {5}", new DateTime(q.dateTimeTicks), new DateTime(q.dateTimeTicks).Millisecond, q.askPrice, q.askSize, q.bidPrice, q.bidSize));
                            }
                            else if (2 <= debugTraceLevel)
                            {
                                foreach (var v in quoteList)
                                {
                                    Trace.WriteLine(string.Format("{0} ms {1}, ask {2}, {3}, bid {4}, {5}", new DateTime(v.dateTimeTicks), new DateTime(v.dateTimeTicks).Millisecond, v.askPrice, v.askSize, v.bidPrice, v.bidSize));
                                }
                            }
                        }
                    }
                }
                else if (candles)
                {
                    DataTimeFrame timeFrame = DataTimeFrame.Aperiodic;
                    OhlcvKind kind = OhlcvKind.Ask;
                    if (s.EndsWith("ASK_candles_sec_10.bin"))
                        timeFrame = DataTimeFrame.Second10;
                    else if (s.EndsWith("BID_candles_sec_10.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Second10;
                    }
                    else if (s.EndsWith("ASK_candles_min_1.bin"))
                        timeFrame = DataTimeFrame.Minute1;
                    else if (s.EndsWith("BID_candles_min_1.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute1;
                    }
                    else if (s.EndsWith("ASK_candles_min_5.bin"))
                        timeFrame = DataTimeFrame.Minute5;
                    else if (s.EndsWith("BID_candles_min_5.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute5;
                    }
                    else if (s.EndsWith("ASK_candles_min_10.bin"))
                        timeFrame = DataTimeFrame.Minute10;
                    else if (s.EndsWith("BID_candles_min_10.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute10;
                    }
                    else if (s.EndsWith("ASK_candles_min_15.bin"))
                        timeFrame = DataTimeFrame.Minute15;
                    else if (s.EndsWith("BID_candles_min_15.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute15;
                    }
                    else if (s.EndsWith("ASK_candles_min_30.bin"))
                        timeFrame = DataTimeFrame.Minute30;
                    else if (s.EndsWith("BID_candles_min_30.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Minute30;
                    }
                    else if (s.EndsWith("ASK_candles_hour_1.bin"))
                        timeFrame = DataTimeFrame.Hour1;
                    else if (s.EndsWith("BID_candles_hour_1.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Hour1;
                    }
                    else if (s.EndsWith("ASK_candles_hour_4.bin"))
                        timeFrame = DataTimeFrame.Hour4;
                    else if (s.EndsWith("BID_candles_hour_4.bin"))
                    {
                        kind = OhlcvKind.Bid;
                        timeFrame = DataTimeFrame.Hour4;
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
                                Repository repository;
                                if (!fileDictionary.TryGetValue(h5FilePath, out repository))
                                {
                                    repository = Repository.OpenReadWrite(h5FilePath, true, Properties.Settings.Default.CorkTheCache);
                                    if (null != repository && !alwaysCloseFile)
                                        fileDictionary.Add(h5FilePath, repository);
                                }
                                if (null == repository)
                                    Trace.TraceError("Failed to open [{0}]", h5FilePath);
                                else
                                {
                                    Instrument instrument = repository.Open(h5InstrumentPath, true);
                                    if (null == instrument)
                                        Trace.TraceError("Failed to open [{0}] in [{1}]", instrumentName, h5FilePath);
                                    else
                                    {
                                        OhlcvData data = instrument.OpenOhlcv(kind, timeFrame, true);
                                        if (null == data)
                                            Trace.TraceError("Failed to open ohlcv data in [{0}] in [{1}]", instrumentName, h5FilePath);
                                        else
                                        {
                                            data.SpreadDuplicateTimeTicks(ohlcvList, true);
                                            if (!data.Add(ohlcvList, DuplicateTimeTicks.Skip, true))
                                                Trace.TraceError("Error importing [{0}] parsed ohlcvs to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                            data.Flush();
                                            if (alwaysCloseFile)
                                                data.Close();
                                        }
                                        if (alwaysCloseFile)
                                            instrument.Close();
                                    }
                                    if (alwaysCloseFile)
                                        repository.Close();
                                }
                            }
                            else
                                Trace.TraceWarning("No [{0}] ohlcvs parsed from [{1}]", instrumentName, fi.FullName);
                            if (0 < count && 0 < debugTraceLevel)
                            {
                                if (1 == debugTraceLevel)
                                {
                                    Ohlcv o = ohlcvList[0];
                                    Trace.WriteLine(string.Format("{0} ms {1}, open {2}, high {3}, low {4}, close {5}, volume {6}", new DateTime(o.dateTimeTicks), new DateTime(o.dateTimeTicks).Millisecond, o.open, o.high, o.low, o.close, o.volume));
                                    o = ohlcvList[count - 1];
                                    Trace.WriteLine(string.Format("... {0} ...", count - 2));
                                    Trace.WriteLine(string.Format("{0} ms {1}, open {2}, high {3}, low {4}, close {5}, volume {6}", new DateTime(o.dateTimeTicks), new DateTime(o.dateTimeTicks).Millisecond, o.open, o.high, o.low, o.close, o.volume));
                                }
                                else if (2 <= debugTraceLevel)
                                {
                                    foreach (var o in ohlcvList)
                                    {
                                        Trace.WriteLine(string.Format("{0} ms {1}, open {2}, high {3}, low {4}, close {5}, volume {6}", new DateTime(o.dateTimeTicks), new DateTime(o.dateTimeTicks).Millisecond, o.open, o.high, o.low, o.close, o.volume));
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        static private void TraverseTree(string root, Action<string> action)
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

        static private string FxInstrumentName(string directory)
        {
            foreach (var v in DukascopyFxContext.Symbols)
            {
                if (directory.Contains(v))
                    return v;
            }
            Trace.TraceError(String.Concat("No FX instrument name found in the path: [", directory, "]"));
            return null;
        }

        static private DateTime FxInstrumentBaseDateTime(string fullPath, string pair, out double factor)
        {
            if (pair.Contains("JPY") || pair.Contains("RUB") || pair.Contains("XAGUSD") || pair.Contains("XAUUSD"))
                factor = 0.001;
            else
                factor = 0.00001;
            pair = string.Concat("\\", pair, "\\");
            int i = fullPath.LastIndexOf(pair);
            if (i < 0)
                throw new ArgumentException(String.Concat("No FX instrument [", pair, "] found in the path: [", fullPath, "]"));
            string s = fullPath.Substring(i + pair.Length);
            int year;
            if (!int.TryParse(s.Substring(0, 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
                throw new ArgumentException(String.Concat("No FX instrument [", pair, "] year found in the path: [", fullPath, "]"));
            s = s.Substring(5);
            int month;
            if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out month))
                throw new ArgumentException(String.Concat("No FX instrument [", pair, "] month found in the path: [", fullPath, "]"));
            ++month;
            s = s.Substring(3);
            if (s.Contains("\\"))
            {
                int day;
                if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
                    throw new ArgumentException(String.Concat("No FX instrument [", pair, "] day found in the path: [", fullPath, "]"));
                s = s.Substring(3);
                if (s.Contains("_ticks."))
                {
                    int hour;
                    if (!int.TryParse(s.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out hour))
                        throw new ArgumentException(String.Concat("No FX instrument [", pair, "] hour found in the path: [", fullPath, "]"));
                    return new DateTime(year, month, day, hour, 0, 0);
                }
                return new DateTime(year, month, day);
            }
            return new DateTime(year, month, 1);
        }

        internal static bool IsOldFormat;
        static private void ParseFile(string zipFile, List<Quote> list, string fullPath, string pair)
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
                    double factorPrice;
                    DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out factorPrice);
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
                double factorPrice;
                DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out factorPrice);
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

        static private void ParseFile(string zipFile, List<Ohlcv> list, string fullPath, string pair)
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
                                if (0.0 != mbOhlcv.volume)
                                    list.Add(mbOhlcv);
                            }
                        }
                    }
                }
                catch
                {
                    //const double factorVolume = 100000;
                    double factorPrice;
                    DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out factorPrice);
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
                        //mbOhlcv.volume = Math.Round(factorVolume * ExtractDouble4(bytes, ref offset));
                        mbOhlcv.volume = Math.Round(ExtractDouble4(bytes, ref offset), 2);
                        if (0.0 != mbOhlcv.volume)
                            list.Add(mbOhlcv);
                    }
                }
            }
            else
            {
                //const double factorVolume = 100000;
                double factorPrice;
                DateTime dateTime = FxInstrumentBaseDateTime(fullPath, pair, out factorPrice);
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
                    //mbOhlcv.volume = Math.Round(factorVolume * ExtractDouble4(bytes, ref offset));
                    mbOhlcv.volume = Math.Round(ExtractDouble4(bytes, ref offset), 2);
                    if (0.0 != mbOhlcv.volume)
                        list.Add(mbOhlcv);
                }
            }
        }

        static private byte[] LzmaFirstEntry(string lzmaFile)
        {
            if (!File.Exists(lzmaFile))
            {
                Trace.TraceError(String.Concat("Lzma file does not exist: ", lzmaFile));
                return null;
            }
            var fileInfo = new FileInfo(lzmaFile);
            if (0 == fileInfo.Length)
            {
                Trace.TraceInformation(String.Concat("Lzma file has zero length: ", lzmaFile));
                return null;
            }
            var fileStream = new FileStream(lzmaFile, FileMode.Open, FileAccess.Read);
            var data = new byte[fileStream.Length];
            fileStream.Read(data, 0, Convert.ToInt32(fileStream.Length));
            fileStream.Close();
            return data;
        }

        static private MemoryStream UnzippedFirstEntry(string zipFile)
        {
            if (!File.Exists(zipFile))
            {
                Trace.TraceError(String.Concat("Zip file does not exist: ", zipFile));
                return null;
            }
            var fileInfo = new FileInfo(zipFile);
            if (0 == fileInfo.Length)
            {
                Trace.TraceInformation(String.Concat("Zip file has zero length: ", zipFile));
                return null;
            }
            ZipStorer zip = ZipStorer.Open(zipFile, FileAccess.Read);
            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
            if (1 != dir.Count)
            {
                zip.Close();
                Trace.TraceError(String.Concat("Zip file does not contain the only entry: ", zipFile));
                return null;
            }
            var stream = new MemoryStream(1024 * 256);
            zip.ExtractFile(dir[0], stream);
            zip.Close();
            return stream;
        }

        static private readonly DateTime StartOfEpoch = new DateTime(1970, 1, 1);
        static private long ExtractDateTime(byte[] bytes, ref long offset)
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

        static private double ExtractDouble(byte[] bytes, ref long offset)
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

        static private float ExtractDouble4(byte[] bytes, ref long offset)
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

        static private int ExtractInt4(byte[] bytes, ref long offset)
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

        static private long ExtractDateTime4(byte[] bytes, ref long offset)
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
