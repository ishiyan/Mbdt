using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Mbh5;

namespace EuronextHistoryRepair
{
    internal static class Program
    {
        private static readonly bool SortContent = Properties.Settings.Default.SortContent;

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: file_or_directory_name");
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Repository.InterceptErrorStack();
            TraverseTree(args[0], AuditDataSets);
        }

        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                    action(entry);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
                action(root);
        }

        private static void AuditDataSets(string sourceFileName)
        {
            if (!sourceFileName.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                return;
            using (Repository repository = Repository.OpenReadWrite(sourceFileName, false))
            {
                List<DataInfo> list = repository.ContentList(SortContent);
                string file = string.Concat(sourceFileName, ":");
                foreach (var dataInfo in list)
                {
                    string full = string.Concat(file, dataInfo.Path);
                    if (!dataInfo.IsValidName)
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] invalid content list entry", full);
                        Trace.WriteLine(msg);
                        continue;
                    }
                    // Trace.TraceInformation(full);
                    using (Instrument instrument = repository.Open(dataInfo.Parent.Path, false))
                    {
                        if (null == instrument)
                        {
                            string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] failed to open the instrument", full);
                            Trace.WriteLine(msg);
                            continue;
                        }
                        switch (dataInfo.ParsedDataType)
                        {
                            case DataType.Trade:
                                using (TradeData data = instrument.OpenTrade())
                                {
                                    // ReSharper disable once UnusedVariable
                                    long count = data.Count;
                                }
                                break;
                            case DataType.TradePriceOnly:
                                using (TradePriceOnlyData data = instrument.OpenTradePriceOnly())
                                {
                                    // ReSharper disable once UnusedVariable
                                    long count = data.Count;
                                }
                                break;
                            case DataType.Quote:
                                using (QuoteData data = instrument.OpenQuote())
                                {
                                    long count = data.Count;
                                    string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found quote type of length {1}", full, count);
                                    Trace.WriteLine(msg);
                                }
                                break;
                            case DataType.QuotePriceOnly:
                                using (QuotePriceOnlyData data = instrument.OpenQuotePriceOnly())
                                {
                                    long count = data.Count;
                                    string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found quote price only type of length {1}", full, count);
                                    Trace.WriteLine(msg);
                                }
                                break;
                            case DataType.Ohlcv:
                            case DataType.OhlcvAdjusted:
                                using (OhlcvData data = dataInfo.ParsedDataType == DataType.Ohlcv
                                    ? instrument.OpenOhlcv(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame)
                                    : instrument.OpenOhlcvAdjusted(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                {
                                    var all = new List<Ohlcv>((int)data.Count);
                                    data.Fetch(all);
                                    var corrected = Correct(all, full);
                                    if (corrected.Count > 0)
                                    {
                                        data.Add(corrected, DuplicateTimeTicks.Update, false);
                                    }
                                }
                                break;
                            case DataType.OhlcvPriceOnly:
                            case DataType.OhlcvAdjustedPriceOnly:
                                using (OhlcvPriceOnlyData data = dataInfo.ParsedDataType == DataType.OhlcvPriceOnly
                                    ? instrument.OpenOhlcvPriceOnly(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame)
                                    : instrument.OpenOhlcvAdjustedPriceOnly(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                {
                                    var all = new List<OhlcvPriceOnly>((int)data.Count);
                                    data.Fetch(all);
                                    var corrected = Correct(all, full);
                                    if (corrected.Count > 0)
                                    {
                                        data.Add(corrected, DuplicateTimeTicks.Update, false);
                                    }
                                }
                                break;
                            case DataType.Scalar:
                                using (ScalarData data = instrument.OpenScalar(dataInfo.ParsedScalarKind, dataInfo.ParsedTimeFrame))
                                {
                                    long count = data.Count;
                                    string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found scalar type of length {1}", full, count);
                                    Trace.WriteLine(msg);
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static List<Ohlcv> Correct(List<Ohlcv> list, string path)
        {
            var corrected = new List<Ohlcv>();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < list.Count; ++i)
            {
                var ohlcv = list[i];
                if (ohlcv.Close > double.Epsilon)
                {
                    if (ohlcv.Open < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                    {
                        // All three are zero, only close is present.
                        Trace.TraceError("date [{0}]: open [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        ohlcv.high = ohlcv.Close;
                        ohlcv.low = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                    {
                        // Close equals open, high and low are zero.
                        Trace.TraceError("date [{0}]: open equals close [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.high = ohlcv.Close;
                        ohlcv.low = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.Open < double.Epsilon && Math.Abs(ohlcv.High - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.Low - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only open is zero.
                        Trace.TraceError("date [{0}]: close [{4}], high [{2}], low [{3}] are the same, open [{1}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.High < double.Epsilon && Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.Low - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only high is zero.
                        Trace.TraceError("date [{0}]: open [{1}], low [{3}], close [{4}] are the same, high [{2}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.Low < double.Epsilon && Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.High - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only low is zero.
                        Trace.TraceError("date [{0}]: open [{1}], high [{2}], close [{4}] are the same, low [{3}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                }
                if (ohlcv.Open < double.Epsilon || ohlcv.High < double.Epsilon || ohlcv.Low < double.Epsilon || ohlcv.Close < double.Epsilon)
                    Trace.TraceError("date [{0}]: found zero price: open [{1}] high [{2}] low [{3}] close [{4}], file {5}", ohlcv.DateStamp, ohlcv.Open,
                        ohlcv.High, ohlcv.Low, ohlcv.Close, path);
            }

            return corrected;
        }

        private static List<OhlcvPriceOnly> Correct(List<OhlcvPriceOnly> list, string path)
        {
            var corrected = new List<OhlcvPriceOnly>();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < list.Count; ++i)
            {
                var ohlcv = list[i];
                if (ohlcv.Close > double.Epsilon)
                {
                    if (ohlcv.Open < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                    {
                        // All three are zero, only close is present.
                        Trace.TraceError("date [{0}]: open [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        ohlcv.high = ohlcv.Close;
                        ohlcv.low = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                    {
                        // Close equals open, high and low are zero.
                        Trace.TraceError("date [{0}]: open equals close [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.high = ohlcv.Close;
                        ohlcv.low = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.Open < double.Epsilon && Math.Abs(ohlcv.High - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.Low - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only open is zero.
                        Trace.TraceError("date [{0}]: close [{4}], high [{2}], low [{3}] are the same, open [{1}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.High < double.Epsilon && Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.Low - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only high is zero.
                        Trace.TraceError("date [{0}]: open [{1}], low [{3}], close [{4}] are the same, high [{2}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                    else if (ohlcv.Low < double.Epsilon && Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && Math.Abs(ohlcv.High - ohlcv.Close) < double.Epsilon)
                    {
                        // All three are the same, only low is zero.
                        Trace.TraceError(
                            "date [{0}]: open [{1}], high [{2}], close [{4}] are the same, low [{3}] is zero: replacing with close [{4}], file {5}",
                            ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close, path);
                        ohlcv.open = ohlcv.Close;
                        corrected.Add(ohlcv);
                    }
                }
                if (ohlcv.Open < double.Epsilon || ohlcv.High < double.Epsilon || ohlcv.Low < double.Epsilon || ohlcv.Close < double.Epsilon)
                    Trace.TraceError("date [{0}]: found zero price: open [{1}] high [{2}] low [{3}] close [{4}], file {5}", ohlcv.DateStamp, ohlcv.Open,
                        ohlcv.High, ohlcv.Low, ohlcv.Close, path);
            }

            return corrected;
        }
    }
}
