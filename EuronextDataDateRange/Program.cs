using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Mbh5;

namespace EuronextDataDateRange
{
    internal static class Program
    {
        private static readonly bool SortContent =false;
        private static readonly List<Info> Infos = new List<Info>(1024 * 7);

        private class Info
        {
            public string FilePath { get; set; }
            public string InstrumentPath { get; set; }
            public string FullPath { get; set; }
            public DateTime MaxDate { get; set; }
            public bool HasIntraday { get; set; }
            public bool HasIntradayPriceOnly { get; set; }
            public bool HasHistory { get; set; }
            public bool HasHistoryAdjusted { get; set; }
            public bool HasHistoryPriceOnly { get; set; }
            public bool HasHistoryPriceOnlyAdjusted { get; set; }
            public string IntradayName { get; set; }
            public long IntradayCount { get; set; }
            public DateTime IntradayStart { get; set; }
            public DateTime IntradayEnd { get; set; }
            public string IntradayPriceOnlyName { get; set; }
            public long IntradayPriceOnlyCount { get; set; }
            public DateTime IntradayPriceOnlyStart { get; set; }
            public DateTime IntradayPriceOnlyEnd { get; set; }
            public string HistoryName { get; set; }
            public long HistoryCount { get; set; }
            public DateTime HistoryStart { get; set; }
            public DateTime HistoryEnd { get; set; }
            public string HistoryPriceOnlyName { get; set; }
            public long HistoryPriceOnlyCount { get; set; }
            public DateTime HistoryPriceOnlyStart { get; set; }
            public DateTime HistoryPriceOnlyEnd { get; set; }
            public string HistoryAdjustedName { get; set; }
            public long HistoryAdjustedCount { get; set; }
            public DateTime HistoryAdjustedStart { get; set; }
            public DateTime HistoryAdjustedEnd { get; set; }
            public string HistoryPriceOnlyAdjustedName { get; set; }
            public long HistoryPriceOnlyAdjustedCount { get; set; }
            public DateTime HistoryPriceOnlyAdjustedStart { get; set; }
            public DateTime HistoryPriceOnlyAdjustedEnd { get; set; }

            public string PrintInfo()
            {
                var builder = new StringBuilder();
                builder.Append(FullPath);
                if (HasIntraday)
                {
                    builder.Append("; ");
                    builder.Append(IntradayName);
                    builder.Append(" ");
                    builder.Append(IntradayCount);
                    builder.Append(" ");
                    builder.Append(IntradayStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(IntradayEnd.ToShortDateString());
                }

                if (HasIntradayPriceOnly)
                {
                    builder.Append("; ");
                    builder.Append(IntradayPriceOnlyName);
                    builder.Append(" ");
                    builder.Append(IntradayPriceOnlyCount);
                    builder.Append(" ");
                    builder.Append(IntradayPriceOnlyStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(IntradayPriceOnlyEnd.ToShortDateString());
                }

                if (HasHistory)
                {
                    builder.Append("; ");
                    builder.Append(HistoryName);
                    builder.Append(" ");
                    builder.Append(HistoryCount);
                    builder.Append(" ");
                    builder.Append(HistoryStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(HistoryEnd.ToShortDateString());
                }

                if (HasHistoryPriceOnly)
                {
                    builder.Append("; ");
                    builder.Append(HistoryPriceOnlyName);
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyCount);
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyEnd.ToShortDateString());
                }

                if (HasHistoryAdjusted)
                {
                    builder.Append("; ");
                    builder.Append(HistoryAdjustedName);
                    builder.Append(" ");
                    builder.Append(HistoryAdjustedCount);
                    builder.Append(" ");
                    builder.Append(HistoryAdjustedStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(HistoryAdjustedEnd.ToShortDateString());
                }

                if (HasHistoryPriceOnlyAdjusted)
                {
                    builder.Append("; ");
                    builder.Append(HistoryPriceOnlyAdjustedName);
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyAdjustedCount);
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyAdjustedStart.ToShortDateString());
                    builder.Append(" ");
                    builder.Append(HistoryPriceOnlyAdjustedEnd.ToShortDateString());
                }

                return builder.ToString();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: file_or_directory_name");
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Repository.InterceptErrorStack();
            TraverseTree(args[0], AuditDataSets);
            Infos.Sort((x, y) => DateTime.Compare(x.MaxDate, y.MaxDate));
            foreach (var info in Infos)
            {
                Trace.WriteLine(info.PrintInfo());
            }
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
                Dictionary<GroupInfo,Info> instruments = new Dictionary<GroupInfo, Info>();
                foreach (var dataInfo in list)
                {
                    var groupInfo = dataInfo.Parent;
                    var key = dataInfo.Parent.Path;
                    if (!instruments.TryGetValue(groupInfo, out var info))
                    {
                        instruments.Add(groupInfo, new Info { FilePath = sourceFileName, InstrumentPath = key, FullPath = file + key});
                    }
                }

                foreach (var kv in instruments)
                {
                    var groupInfo = kv.Key;
                    var info = kv.Value;
                    using (var instrument = repository.Open(groupInfo.Path, false))
                    {
                        if (null == instrument)
                        {
                            string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] failed to open the instrument", file + groupInfo.Path);
                            Trace.WriteLine(msg);
                        }
                        else
                        {
                            foreach (var dataInfo in instrument.DatasetList(false))
                            {
                                switch (dataInfo.ParsedDataType)
                                {
                                    case DataType.Trade:
                                        using (TradeData data = instrument.OpenTrade())
                                        {
                                            info.HasIntraday = true;
                                            info.IntradayName = dataInfo.Name;
                                            info.IntradayCount = data.Count;
                                            info.IntradayStart = new DateTime(data.FirstTicks);
                                            info.IntradayEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.IntradayStart)
                                            {
                                                info.MaxDate = info.IntradayStart;
                                            }
                                            if (info.MaxDate < info.IntradayEnd)
                                            {
                                                info.MaxDate = info.IntradayEnd;
                                            }
                                        }
                                        break;
                                    case DataType.TradePriceOnly:
                                        using (TradePriceOnlyData data = instrument.OpenTradePriceOnly())
                                        {
                                            info.HasIntradayPriceOnly = true;
                                            info.IntradayPriceOnlyName = dataInfo.Name;
                                            info.IntradayPriceOnlyCount = data.Count;
                                            info.IntradayPriceOnlyStart = new DateTime(data.FirstTicks);
                                            info.IntradayPriceOnlyEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.IntradayPriceOnlyStart)
                                            {
                                                info.MaxDate = info.IntradayPriceOnlyStart;
                                            }
                                            if (info.MaxDate < info.IntradayPriceOnlyEnd)
                                            {
                                                info.MaxDate = info.IntradayPriceOnlyEnd;
                                            }
                                        }
                                        break;
                                    case DataType.Quote:
                                        using (QuoteData data = instrument.OpenQuote())
                                        {
                                            long count = data.Count;
                                            string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found quote type of length {1}", file + groupInfo.Path, count);
                                            Trace.WriteLine(msg);
                                        }
                                        break;
                                    case DataType.QuotePriceOnly:
                                        using (QuotePriceOnlyData data = instrument.OpenQuotePriceOnly())
                                        {
                                            long count = data.Count;
                                            string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found quote price only type of length {1}", file + groupInfo.Path, count);
                                            Trace.WriteLine(msg);
                                        }
                                        break;
                                    case DataType.Ohlcv:
                                        using (OhlcvData data = instrument.OpenOhlcv(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                        {
                                            info.HasHistory = true;
                                            info.HistoryName = dataInfo.Name;
                                            info.HistoryCount = data.Count;
                                            info.HistoryStart = new DateTime(data.FirstTicks);
                                            info.HistoryEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.HistoryStart)
                                            {
                                                info.MaxDate = info.HistoryStart;
                                            }
                                            if (info.MaxDate < info.HistoryEnd)
                                            {
                                                info.MaxDate = info.HistoryEnd;
                                            }
                                        }
                                        break;
                                    case DataType.OhlcvAdjusted:
                                        using (OhlcvData data = instrument.OpenOhlcvAdjusted(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                        {
                                            info.HasHistoryAdjusted = true;
                                            info.HistoryAdjustedName = dataInfo.Name;
                                            info.HistoryAdjustedCount = data.Count;
                                            info.HistoryAdjustedStart = new DateTime(data.FirstTicks);
                                            info.HistoryAdjustedEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.HistoryAdjustedStart)
                                            {
                                                info.MaxDate = info.HistoryAdjustedStart;
                                            }
                                            if (info.MaxDate < info.HistoryAdjustedEnd)
                                            {
                                                info.MaxDate = info.HistoryAdjustedEnd;
                                            }
                                        }
                                        break;
                                    case DataType.OhlcvPriceOnly:
                                        using (OhlcvPriceOnlyData data = instrument.OpenOhlcvPriceOnly(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                        {
                                            info.HasHistoryPriceOnly = true;
                                            info.HistoryPriceOnlyName = dataInfo.Name;
                                            info.HistoryPriceOnlyCount = data.Count;
                                            info.HistoryPriceOnlyStart = new DateTime(data.FirstTicks);
                                            info.HistoryPriceOnlyEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.HistoryPriceOnlyStart)
                                            {
                                                info.MaxDate = info.HistoryPriceOnlyStart;
                                            }
                                            if (info.MaxDate < info.HistoryPriceOnlyEnd)
                                            {
                                                info.MaxDate = info.HistoryPriceOnlyEnd;
                                            }
                                        }
                                        break;
                                    case DataType.OhlcvAdjustedPriceOnly:
                                        using (OhlcvPriceOnlyData data = instrument.OpenOhlcvAdjustedPriceOnly(dataInfo.ParsedOhlcvKind, dataInfo.ParsedTimeFrame))
                                        {
                                            info.HasHistoryPriceOnlyAdjusted = true;
                                            info.HistoryPriceOnlyAdjustedName = dataInfo.Name;
                                            info.HistoryPriceOnlyAdjustedCount = data.Count;
                                            info.HistoryPriceOnlyAdjustedStart = new DateTime(data.FirstTicks);
                                            info.HistoryPriceOnlyAdjustedEnd = new DateTime(data.LastTicks);
                                            if (info.MaxDate < info.HistoryPriceOnlyAdjustedStart)
                                            {
                                                info.MaxDate = info.HistoryPriceOnlyAdjustedStart;
                                            }
                                            if (info.MaxDate < info.HistoryPriceOnlyAdjustedEnd)
                                            {
                                                info.MaxDate = info.HistoryPriceOnlyAdjustedEnd;
                                            }
                                        }
                                        break;
                                    case DataType.Scalar:
                                        using (ScalarData data = instrument.OpenScalar(dataInfo.ParsedScalarKind, dataInfo.ParsedTimeFrame))
                                        {
                                            long count = data.Count;
                                            string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] found scalar type of length {1}", file + groupInfo.Path, count);
                                            Trace.WriteLine(msg);
                                        }
                                        break;
                                }

                            }
                        }

                    }
                    Infos.Add(info);
                }
            }
        }
    }
}
