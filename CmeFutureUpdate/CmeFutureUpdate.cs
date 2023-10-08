using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using mbdt.Utils;
using Mbh5;

namespace CmeFutureUpdate
{
    internal class CmeFutureUpdate
    {
        const int ElementsPerAdd = 40000;

        internal CmeFutureUpdate()
        {
            foreach (var future in futureList)
            {
                int count = 0;// future.Series.Count;
                string symbol = future.Symbol;
                future.Front = new List<FrontSeries>(count);
                for (int i = 0; i < count; ++i)
                {
                    var frontSeries = new FrontSeries
                    {
                        Code = string.Format(CultureInfo.InvariantCulture, "{0}S{1}", symbol, i + 1),
                        SeriesEntry = future.Series[i]
                    };
                    future.Front.Add(frontSeries);
                }
            }
        }

        private class TimeSlot
        {
            public string Id;
            public int DayDelta;
        }
        private class FrontSeries
        {
            public SeriesEntry SeriesEntry;
            public string Code;
            public string InstrumentPath(Future future)
            {
                return $"/XCME_{future.Symbol}_{Code}";
            }
        }
        private class SeriesEntry
        {
            public string Code, MonthShortcut, Month;
            public DateTime FirstTradeDate, LastTradeDate, SettlementDate, RolloverDate;
            public readonly List<Trade> TradeList = new List<Trade>(1024);
            public string InstrumentPath(Future future)
            {
                return $"/XCME_{future.Symbol}_{Code}";
            }
            public string FilePath(Future future, string yyyymmdd, string slotId)
            {
                const string delimiter = "\\";
                string dir = future.DownloadDir(yyyymmdd);
                if (!dir.EndsWith(delimiter))
                    dir = string.Concat(dir, delimiter);
                return string.Concat(dir, $"XCME_{future.Symbol}_{Code}_{yyyymmdd}_{slotId}_eoi.xml");
            }
            public void ExportCsv(Future future, string yyyymmdd)
            {
                if (TradeList.Count < 1)
                    return;
                string file = $"XCME_{future.Symbol}_{Code}_{yyyymmdd}_eoi.csv";
                File.WriteAllLines(file,
                    TradeList.Select(t => string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", new DateTime(t.Ticks).ToString("yyyy/MM/dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture), t.price, t.volume)));
            }
        }
        private class Future
        {
            public string Symbol, Name, Uri, Referer;
            public List<SeriesEntry> Series;
            public List<FrontSeries> Front;
            public List<TimeSlot> TimeSlotList;
            public int SessionStartHour, SessionStartMinute;
            public string DownloadDir(string yyyymmdd)
            {
                const string delimiter = "\\";
                string dir = Properties.Settings.Default.DownloadRepositoryPath;
                if (!dir.EndsWith(delimiter))
                    dir = string.Concat(dir, delimiter);
                dir = string.Concat(dir, Symbol, delimiter, yyyymmdd);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return string.Concat(dir, delimiter);
            }
            public string DownloadZip(string yyyymmdd)
            {
                const string delimiter = "\\";
                string dir = Properties.Settings.Default.DownloadRepositoryPath;
                if (!dir.EndsWith(delimiter))
                    dir = string.Concat(dir, delimiter);
                dir = string.Concat(dir, Symbol);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return string.Concat(dir, delimiter, yyyymmdd, "_eoi.zip");
            }
            public string RepositoryPath()
            {
                const string delimiter = "\\";
                string dir = Properties.Settings.Default.RepositoryDirectory;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!dir.EndsWith(delimiter))
                    dir = string.Concat(dir, delimiter);
                return string.Concat(dir, Symbol, ".h5");
            }
        }
        private readonly List<Future> futureList = new List<Future>
        {
            new Future
            {
                Symbol="ES", Name="E-mini S&P 500 (Dollar)",
                Uri="http://www.cmegroup.com/trading/equity-index/us-index/e-mini-sandp500_product_calendar_futures.html",
                Referer="http://www.cmegroup.com/trading/equity-index/us-index/e-mini-sandp500_quotes_timeSales_globex_futures.html",
                SessionStartHour = 16, SessionStartMinute = 45,
                Series = new List<SeriesEntry>
                {
                   // http://www.cmegroup.com/trading/equity-index/rolldates.html
                   // http://www.cmegroup.com/trading/equity-index/us-index/e-mini-sandp500_product_calendar_futures.html
                   // Contract Month  Product Code  First Trade  Last Trade    Expiration  Rollover
                   // JUN 2012        ESM12         ----------   06/15/2012    06/15/2012  06/07/2012
                   // SEP 2012        ESU12         ----------   09/21/2012    09/21/2012  09/13/2012
                   // DEC 2012        ESZ12         ----------   12/21/2012    12/21/2012  12/13/2012
                   // MAR 2013        ESH13         ----------   03/15/2013    03/15/2013  03/07/2013
                 //new SeriesEntry { Code="ESM13", Month = "JUN 2013", MonthShortcut="JUN-13", FirstTradeDate=new DateTime(2012,  3, 16), LastTradeDate=new DateTime(2013,  6, 21), SettlementDate=new DateTime(2013,  6, 21), RolloverDate=new DateTime(2013,  6, 13)},
                 //new SeriesEntry { Code="ESU13", Month = "SEP 2013", MonthShortcut="SEP-13", FirstTradeDate=new DateTime(2012,  6, 15), LastTradeDate=new DateTime(2013,  9, 20), SettlementDate=new DateTime(2013,  9, 20), RolloverDate=new DateTime(2013,  9, 12)},
                 //new SeriesEntry { Code="ESZ13", Month = "DEC 2013", MonthShortcut="DEC-13", FirstTradeDate=new DateTime(2012,  9, 21), LastTradeDate=new DateTime(2013, 12, 20), SettlementDate=new DateTime(2013, 12, 20), RolloverDate=new DateTime(2013, 12, 12)},
                 //new SeriesEntry { Code="ESH14", Month = "MAR 2014", MonthShortcut="MAR-14", FirstTradeDate=new DateTime(2012, 12, 21), LastTradeDate=new DateTime(2014,  3, 21), SettlementDate=new DateTime(2014,  3, 21), RolloverDate=new DateTime(2014,  3, 13)},
                 //new SeriesEntry { Code="ESM14", Month = "JUN 2014", MonthShortcut="JUN-14", FirstTradeDate=new DateTime(2013,  3, 15), LastTradeDate=new DateTime(2014,  6, 20), SettlementDate=new DateTime(2014,  6, 20), RolloverDate=new DateTime(2014,  6, 12)},
                 //new SeriesEntry { Code="ESU14", Month = "SEP 2014", MonthShortcut="SEP-14", FirstTradeDate=new DateTime(2013,  6, 21), LastTradeDate=new DateTime(2014,  9, 19), SettlementDate=new DateTime(2014,  9, 19), RolloverDate=new DateTime(2014,  9, 11)},
                 //new SeriesEntry { Code="ESZ14", Month = "DEC 2014", MonthShortcut="DEC-14", FirstTradeDate=new DateTime(2013,  9, 20), LastTradeDate=new DateTime(2014, 12, 19), SettlementDate=new DateTime(2014, 12, 19), RolloverDate=new DateTime(2014, 12, 11)},
                 //new SeriesEntry { Code="ESH15", Month = "MAR 2015", MonthShortcut="MAR-15", FirstTradeDate=new DateTime(2013, 12, 20), LastTradeDate=new DateTime(2015,  3, 20), SettlementDate=new DateTime(2015,  3, 20), RolloverDate=new DateTime(2015,  3, 12)},
                 //new SeriesEntry { Code="ESM15", Month = "JUN 2015", MonthShortcut="JUN-15", FirstTradeDate=new DateTime(2014,  3, 21), LastTradeDate=new DateTime(2015,  6, 19), SettlementDate=new DateTime(2015,  6, 19), RolloverDate=new DateTime(2015,  6, 11)},
                 //new SeriesEntry { Code="ESU15", Month = "SEP 2015", MonthShortcut="SEP-15", FirstTradeDate=new DateTime(2014,  6, 20), LastTradeDate=new DateTime(2015,  9, 18), SettlementDate=new DateTime(2015,  9, 18), RolloverDate=new DateTime(2015,  9, 10)},
                 //new SeriesEntry { Code="ESZ15", Month = "DEC 2015", MonthShortcut="DEC-15", FirstTradeDate=new DateTime(2014,  9, 19), LastTradeDate=new DateTime(2015, 12, 18), SettlementDate=new DateTime(2015, 12, 18), RolloverDate=new DateTime(2015, 12, 10)},
                 //new SeriesEntry { Code="ESH16", Month = "MAR 2016", MonthShortcut="MAR-16", FirstTradeDate=new DateTime(2014, 12, 19), LastTradeDate=new DateTime(2016,  3, 18), SettlementDate=new DateTime(2016,  3, 18), RolloverDate=new DateTime(2016,  3, 10)},
                 //new SeriesEntry { Code="ESM16", Month = "JUN 2016", MonthShortcut="JUN-16", FirstTradeDate=new DateTime(2015,  3, 20), LastTradeDate=new DateTime(2016,  6, 17), SettlementDate=new DateTime(2016,  6, 17), RolloverDate=new DateTime(2016,  6,  9)},
                 //new SeriesEntry { Code="ESU16", Month = "SEP 2016", MonthShortcut="SEP-16", FirstTradeDate=new DateTime(2015,  6, 19), LastTradeDate=new DateTime(2016,  9, 16), SettlementDate=new DateTime(2016,  9, 16), RolloverDate=new DateTime(2016,  9,  8)},
                 //new SeriesEntry { Code="ESZ16", Month = "DEC 2016", MonthShortcut="DEC-16", FirstTradeDate=new DateTime(2015,  9, 18), LastTradeDate=new DateTime(2016, 12, 16), SettlementDate=new DateTime(2016, 12, 16), RolloverDate=new DateTime(2016, 12,  8)},
                 //new SeriesEntry { Code="ESH17", Month = "MAR 2017", MonthShortcut="MAR-17", FirstTradeDate=new DateTime(2015, 12, 18), LastTradeDate=new DateTime(2017,  3, 17), SettlementDate=new DateTime(2017,  3, 17), RolloverDate=new DateTime(2017,  3,  8)},
                 //new SeriesEntry { Code="ESM17", Month = "JUN 2017", MonthShortcut="JUN-17", FirstTradeDate=new DateTime(2016,  3, 18), LastTradeDate=new DateTime(2017,  6, 16), SettlementDate=new DateTime(2017,  6, 16), RolloverDate=new DateTime(2017,  6,  8)},
                 //new SeriesEntry { Code="ESU17", Month = "SEP 2017", MonthShortcut="SEP-17", FirstTradeDate=new DateTime(2016,  6, 17), LastTradeDate=new DateTime(2017,  9, 15), SettlementDate=new DateTime(2017,  9, 15), RolloverDate=new DateTime(2017,  9,  7)},
                 //new SeriesEntry { Code="ESZ17", Month = "DEC 2017", MonthShortcut="DEC-17", FirstTradeDate=new DateTime(2016,  9, 16), LastTradeDate=new DateTime(2017, 12, 15), SettlementDate=new DateTime(2017, 12, 15), RolloverDate=new DateTime(2017, 12,  7)},
                 //new SeriesEntry { Code="ESH18", Month = "MAR 2018", MonthShortcut="MAR-18", FirstTradeDate=new DateTime(2016, 12, 16), LastTradeDate=new DateTime(2018,  3, 16), SettlementDate=new DateTime(2018,  3, 16), RolloverDate=new DateTime(2018,  3,  8)},
                 //new SeriesEntry { Code="ESM18", Month = "JUN 2018", MonthShortcut="JUN-18", FirstTradeDate=new DateTime(2017,  3, 17), LastTradeDate=new DateTime(2018,  6, 15), SettlementDate=new DateTime(2018,  6, 15), RolloverDate=new DateTime(2018,  6,  7)},
                 //new SeriesEntry { Code="ESU18", Month = "SEP 2018", MonthShortcut="SEP-18", FirstTradeDate=new DateTime(2017,  6, 16), LastTradeDate=new DateTime(2018,  9, 21), SettlementDate=new DateTime(2018,  9, 21), RolloverDate=new DateTime(2018,  9, 13)},
                 //new SeriesEntry { Code="ESZ18", Month = "DEC 2018", MonthShortcut="DEC-18", FirstTradeDate=new DateTime(2017,  9, 15), LastTradeDate=new DateTime(2018, 12, 21), SettlementDate=new DateTime(2018, 12, 21), RolloverDate=new DateTime(2018, 12, 13)},
                 //new SeriesEntry { Code="ESH19", Month = "MAR 2019", MonthShortcut="MAR-19", FirstTradeDate=new DateTime(2017, 12, 15), LastTradeDate=new DateTime(2019,  3, 15), SettlementDate=new DateTime(2019,  3, 15), RolloverDate=new DateTime(2019,  3,  7)},
                 //new SeriesEntry { Code="ESM19", Month = "JUN 2019", MonthShortcut="JUN-19", FirstTradeDate=new DateTime(2018,  3, 16), LastTradeDate=new DateTime(2019,  6, 21), SettlementDate=new DateTime(2019,  6, 21), RolloverDate=new DateTime(2019,  6, 13)},
                 //new SeriesEntry { Code="ESU19", Month = "SEP 2019", MonthShortcut="SEP-19", FirstTradeDate=new DateTime(2018,  6, 15), LastTradeDate=new DateTime(2019,  9, 20), SettlementDate=new DateTime(2019,  9, 20), RolloverDate=new DateTime(2019,  9, 12)},
                 //new SeriesEntry { Code="ESZ19", Month = "DEC 2019", MonthShortcut="DEC-19", FirstTradeDate=new DateTime(2018,  9, 21), LastTradeDate=new DateTime(2019, 12, 20), SettlementDate=new DateTime(2019, 12, 20), RolloverDate=new DateTime(2019, 12, 12)},
                 //new SeriesEntry { Code="ESH20", Month = "MAR 2020", MonthShortcut="MAR-20", FirstTradeDate=new DateTime(2018, 12, 21), LastTradeDate=new DateTime(2020,  3, 20), SettlementDate=new DateTime(2020,  3, 20), RolloverDate=new DateTime(2020,  3, 12)},
                 //new SeriesEntry { Code="ESM20", Month = "JUN 2020", MonthShortcut="JUN-20", FirstTradeDate=new DateTime(2019,  3, 17), LastTradeDate=new DateTime(2020,  6, 19), SettlementDate=new DateTime(2020,  6, 19), RolloverDate=new DateTime(2020,  6, 11)},
                 //new SeriesEntry { Code="ESU20", Month = "SEP 2020", MonthShortcut="SEP-20", FirstTradeDate=new DateTime(2019,  6, 21), LastTradeDate=new DateTime(2020,  9, 18), SettlementDate=new DateTime(2020,  9, 18), RolloverDate=new DateTime(2020,  9, 10)},
                 //new SeriesEntry { Code="ESZ20", Month = "DEC 2020", MonthShortcut="DEC-20", FirstTradeDate=new DateTime(2019,  9, 20), LastTradeDate=new DateTime(2020, 12, 18), SettlementDate=new DateTime(2020, 12, 18), RolloverDate=new DateTime(2020, 12, 10)},
                 //new SeriesEntry { Code="ESH21", Month = "MAR 2021", MonthShortcut="MAR-21", FirstTradeDate=new DateTime(2019, 12, 20), LastTradeDate=new DateTime(2021,  3, 19), SettlementDate=new DateTime(2021,  3, 19), RolloverDate=new DateTime(2021,  3, 11)},
                 //new SeriesEntry { Code="ESM21", Month = "JUN 2021", MonthShortcut="JUN-21", FirstTradeDate=new DateTime(2020,  3, 20), LastTradeDate=new DateTime(2021,  6, 18), SettlementDate=new DateTime(2021,  6, 18), RolloverDate=new DateTime(2021,  6, 10)},
                 //new SeriesEntry { Code="ESU21", Month = "SEP 2021", MonthShortcut="SEP-21", FirstTradeDate=new DateTime(2020,  6, 19), LastTradeDate=new DateTime(2021,  9, 17), SettlementDate=new DateTime(2021,  9, 17), RolloverDate=new DateTime(2021,  9,  9)},
                 //new SeriesEntry { Code="ESZ21", Month = "DEC 2021", MonthShortcut="DEC-21", FirstTradeDate=new DateTime(2020,  9, 18), LastTradeDate=new DateTime(2021, 12, 17), SettlementDate=new DateTime(2021, 12, 17), RolloverDate=new DateTime(2021, 12,  9)},
                 //new SeriesEntry { Code="ESH22", Month = "MAR 2022", MonthShortcut="MAR-22", FirstTradeDate=new DateTime(2020, 12, 18), LastTradeDate=new DateTime(2022,  3, 18), SettlementDate=new DateTime(2022,  3, 18), RolloverDate=new DateTime(2022,  3,  10)},
                 //new SeriesEntry { Code="ESM22", Month = "JUN 2022", MonthShortcut="JUN-22", FirstTradeDate=new DateTime(2021,  3, 19), LastTradeDate=new DateTime(2022,  6, 17), SettlementDate=new DateTime(2022,  3, 18), RolloverDate=new DateTime(2022,  6,  9)},
                 //new SeriesEntry { Code="ESU22", Month = "SEP 2022", MonthShortcut="SEP-22", FirstTradeDate=new DateTime(2021,  6,  7), LastTradeDate=new DateTime(2022,  9, 16), SettlementDate=new DateTime(2022,  9, 16), RolloverDate=new DateTime(2022,  9,  8)},
                 //new SeriesEntry { Code="ESZ22", Month = "DEC 2022", MonthShortcut="DEC-22", FirstTradeDate=new DateTime(2021,  9, 13), LastTradeDate=new DateTime(2022, 12, 16), SettlementDate=new DateTime(2022, 12, 16), RolloverDate=new DateTime(2022, 12, 12)},
                 //new SeriesEntry { Code="ESH23", Month = "MAR 2023", MonthShortcut="MAR-23", FirstTradeDate=new DateTime(2021, 12, 17), LastTradeDate=new DateTime(2023,  3, 17), SettlementDate=new DateTime(2023,  3, 17), RolloverDate=new DateTime(2023,  3, 13)},
                   new SeriesEntry { Code="ESM23", Month = "JUN 2023", MonthShortcut="JUN-23", FirstTradeDate=new DateTime(2022,  3, 17), LastTradeDate=new DateTime(2023,  6, 16), SettlementDate=new DateTime(2023,  6, 16), RolloverDate=new DateTime(2023,  6, 12)},
                   new SeriesEntry { Code="ESU23", Month = "SEP 2023", MonthShortcut="SEP-23", FirstTradeDate=new DateTime(2022,  6, 18), LastTradeDate=new DateTime(2023,  9, 15), SettlementDate=new DateTime(2023,  9, 15), RolloverDate=new DateTime(2023,  9, 11)},
                   new SeriesEntry { Code="ESZ23", Month = "DEC 2023", MonthShortcut="DEC-23", FirstTradeDate=new DateTime(2022,  9, 13), LastTradeDate=new DateTime(2023, 12, 15), SettlementDate=new DateTime(2023, 12, 15), RolloverDate=new DateTime(2023, 12, 11)},
                   new SeriesEntry { Code="ESH24", Month = "MAR 2024", MonthShortcut="MAR-24", FirstTradeDate=new DateTime(2022, 12, 17), LastTradeDate=new DateTime(2024,  3, 15), SettlementDate=new DateTime(2024,  3, 15), RolloverDate=new DateTime(2024,  3, 11)},
                   new SeriesEntry { Code="ESM24", Month = "JUN 2024", MonthShortcut="JUN-24", FirstTradeDate=new DateTime(2023,  3, 18), LastTradeDate=new DateTime(2024,  6, 21), SettlementDate=new DateTime(2024,  6, 21), RolloverDate=new DateTime(2024,  6, 14)}//,
                },
               TimeSlotList = new List<TimeSlot>
               {
                   new TimeSlot { Id="17", DayDelta = -1},
                   new TimeSlot { Id="18", DayDelta = -1},
                   new TimeSlot { Id="19", DayDelta = -1},
                   new TimeSlot { Id="20", DayDelta = -1},
                   new TimeSlot { Id="21", DayDelta = -1},
                   new TimeSlot { Id="22", DayDelta = -1},
                   new TimeSlot { Id="23", DayDelta = -1},
                   new TimeSlot { Id="24", DayDelta = -1},
                   new TimeSlot { Id="101", DayDelta = 0},
                   new TimeSlot { Id="102", DayDelta = 0},
                   new TimeSlot { Id="103", DayDelta = 0},
                   new TimeSlot { Id="104", DayDelta = 0},
                   new TimeSlot { Id="105", DayDelta = 0},
                   new TimeSlot { Id="106", DayDelta = 0},
                   new TimeSlot { Id="107", DayDelta = 0},
                   new TimeSlot { Id="108", DayDelta = 0},
                   new TimeSlot { Id="109", DayDelta = 0},
                   new TimeSlot { Id="110", DayDelta = 0},
                   new TimeSlot { Id="111", DayDelta = 0},
                   new TimeSlot { Id="112", DayDelta = 0},
                   new TimeSlot { Id="113", DayDelta = 0},
                   new TimeSlot { Id="114", DayDelta = 0},
                   new TimeSlot { Id="115", DayDelta = 0},
                   new TimeSlot { Id="116", DayDelta = 0},
                   new TimeSlot { Id="117", DayDelta = 0}
               }
            }
        };

        public void Update(bool doImport = true, string yyyymmdd = null)
        {
            if (null == yyyymmdd)
                yyyymmdd = YyyymmddNow();
            foreach (var future in futureList)
            {
                Download(future, yyyymmdd);
                if (doImport)
                    Import(future, yyyymmdd);
                string zip = future.DownloadZip(yyyymmdd);
                if (File.Exists(zip))
                    zip = string.Concat(zip, ".", DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture), ".zip");
                string dir = future.DownloadDir(yyyymmdd);
                const string delimiter = "\\";
                if (dir.EndsWith(delimiter))
                    dir = dir.Substring(0, dir.Length - 1);
                Packager.ZipXmlDirectory(zip, dir, true);
            }
        }

        public void ImportText(string symbol, string code, string filePath, string yyyymmdd)
        {
            symbol = symbol.ToUpperInvariant();
            Future future = futureList.Find(f => symbol == f.Symbol.ToUpperInvariant());
            if (null == future)
            {
                Trace.TraceError("Failed to find future symbol {0}, aborting", symbol);
                return;
            }
            code = code.ToUpperInvariant();
            SeriesEntry seriesEntry = future.Series.Find(f => code == f.Code.ToUpperInvariant());
            if (null == seriesEntry)
            {
                Trace.TraceError("Failed to find future symbol {0} code {1}, aborting", symbol, code);
                return;
            }
            var trade = new Trade();
            Trace.TraceInformation("Importing {0}, {1}, [{2}]", future.Symbol, future.Name, future.Uri);
            Trace.TraceInformation("Importing {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}",
                seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString());
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Length < 10)
                    continue;
                var spl = line.Split(';');
                trade.dateTimeTicks = DateTime.Parse(spl[0]).Ticks;
                trade.price = double.Parse(spl[1]);
                trade.volume= double.Parse(spl[2]);
                seriesEntry.TradeList.Add(trade);
            }
            string h5File = future.RepositoryPath();
            string instrumentPath = seriesEntry.InstrumentPath(future);
            if (1 > seriesEntry.TradeList.Count)
            {
                Trace.TraceInformation("Skipped merge {0}, {1}, to file {2}, instrument path {3}: no trade data found", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                return;
            }
            Trace.TraceInformation("Merging {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
            Trace.TraceInformation("Merging {0}, {1}, starting check", seriesEntry.Code, seriesEntry.Month);
            var tradePrevious = new Trade();
            bool tradeActivated = false;
            bool isGood = true;
            foreach (var t in seriesEntry.TradeList)
            {
                if (tradeActivated)
                {
                    if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                    {
                        Trade t2 = t;
                        t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                        Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                        isGood = false;
                        tradePrevious = t2;
                    }
                    else
                        tradePrevious = t;
                }
                else
                {
                    tradeActivated = true;
                    tradePrevious = t;
                }
            }
            if (!isGood)
            {
                Trace.TraceInformation("Prohibited merge {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                return;
            }
            Trace.TraceInformation("Merging {0}, {1}, finished check", seriesEntry.Code, seriesEntry.Month);

            using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
            using (Instrument instrument = repository.Open(instrumentPath, true))
            using (TradeData tradeData = instrument.OpenTrade(true))
            {
                if (Properties.Settings.Default.AppendOnly && tradeData.Count > 0)
                {
                    var lastDateTimeExisting = new DateTime(tradeData.LastTicks);
                    var lastDateTimeToImport = new DateTime(seriesEntry.TradeList[seriesEntry.TradeList.Count - 1].Ticks);
                    var firstDateTimeToImport = new DateTime(seriesEntry.TradeList[0].Ticks);
                    Trace.TraceInformation("Merging {0}, {1}, file {2}, instrument path {3}: import first {4} last {5}, last existing {6}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, firstDateTimeToImport, lastDateTimeToImport, lastDateTimeExisting);
                    if (lastDateTimeToImport <= lastDateTimeExisting)
                    {
                        Trace.TraceInformation("Import data already exist: {0}, {1}, file {2}, instrument path {3}: last import {4} <= last existing {5}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, lastDateTimeToImport, lastDateTimeExisting);
                        return;
                    }
                    else if (firstDateTimeToImport <= lastDateTimeExisting)
                    {
                        Trace.TraceInformation("Import data overlaps existing: {0}, {1}, file {2}, instrument path {3}: fist import {4} <= last existing {5}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, firstDateTimeToImport, lastDateTimeExisting);
                        var lastExistingTicks = tradeData.LastTicks;
                        var countToRemoved = seriesEntry.TradeList.RemoveAll(d => d.Ticks <= lastExistingTicks);
                        Trace.TraceInformation("Removed {0} import elements, fist import now is {1}", countToRemoved, new DateTime(seriesEntry.TradeList[0].Ticks));
                    }
                }
                Trace.TraceInformation("Merging {0}, {1}, starting SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                Trace.TraceInformation("Merging {0}, {1}, finished SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
            }
            int maxChunks = 1 + seriesEntry.TradeList.Count / ElementsPerAdd;
            if (maxChunks == 1)
            {
                Trace.TraceInformation("Merging {0}, {1}, starting Add", seriesEntry.Code, seriesEntry.Month);
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                using (Instrument instrument = repository.Open(instrumentPath, true))
                using (TradeData tradeData = instrument.OpenTrade(true))
                {
                    if (!tradeData.Add(seriesEntry.TradeList,
                        Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                        Properties.Settings.Default.Hdf5VerboseAdd))
                        Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                }
                Trace.TraceInformation("Merging {0}, {1}, finished Add", seriesEntry.Code, seriesEntry.Month);
            }
            else
            {
                var buf = new List<Trade>(maxChunks);
                var src = seriesEntry.TradeList;
                int srcIndex = 0;
                for (int chunk = 1; chunk < maxChunks; ++chunk)
                {
                    Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                    buf.Clear();
                    for (int i = 0; i < ElementsPerAdd; ++i)
                    {
                        buf.Add(src[srcIndex++]);
                    }
                    using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    using (TradeData tradeData = instrument.OpenTrade(true))
                    {
                        if (!tradeData.Add(buf,
                            Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                            Properties.Settings.Default.Hdf5VerboseAdd))
                            Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    }
                    Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                }
                Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                buf.Clear();
                int c = src.Count;
                while (srcIndex < c)
                {
                    buf.Add(src[srcIndex++]);
                }
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                using (Instrument instrument = repository.Open(instrumentPath, true))
                using (TradeData tradeData = instrument.OpenTrade(true))
                {
                    if (!tradeData.Add(buf,
                        Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                        Properties.Settings.Default.Hdf5VerboseAdd))
                        Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                }
                Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
            }
        }

        public void ImportText2(string symbol, string code, string filePath, string yyyymmdd)
        {
            symbol = symbol.ToUpperInvariant();
            Future future = futureList.Find(f => symbol == f.Symbol.ToUpperInvariant());
            if (null == future)
            {
                Trace.TraceError("Failed to find future symbol {0}, aborting", symbol);
                return;
            }
            code = code.ToUpperInvariant();
            SeriesEntry seriesEntry = future.Series.Find(f => code == f.Code.ToUpperInvariant());
            if (null == seriesEntry)
            {
                Trace.TraceError("Failed to find future symbol {0} code {1}, aborting", symbol, code);
                return;
            }
            //var frontEntry = future.Front.Find(f => f.SeriesEntry == seriesEntry);
            //if (null == frontEntry)
            //{
            //    Trace.TraceError("Failed to find future front entry symbol {0} code {1}, aborting", symbol, code);
            //    return;
            //}
            var trade = new Trade();
            Trace.TraceInformation("Importing {0}, {1}, [{2}]", future.Symbol, future.Name, future.Uri);
            Trace.TraceInformation("Importing {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}",
                seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString());
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Length < 10)
                    continue;
                var spl = line.Split(';');
                trade.dateTimeTicks = DateTime.Parse(spl[0]).Ticks;
                trade.price = double.Parse(spl[1]);
                trade.volume = double.Parse(spl[2]);
                /*
                string str = line.Substring(0, 10);
                string[] splitted = str.Split('/');
                int month = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                int day = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                int year = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                str = line.Substring(21, 8);
                splitted = str.Split(':');
                int hour = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                int minute = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                int second = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                var dt = new DateTime(year, month, day, hour, minute, second);
                if (hour >= future.SessionStartHour && minute >= future.SessionStartMinute)
                    dt = dt.AddDays(-1);
                trade.dateTimeTicks = dt.Ticks;
                str = line.Substring(33);
                splitted = str.Split('\t');
                string price = splitted[0].Trim(' ');
                if (price.EndsWith("A", StringComparison.Ordinal) || price.EndsWith("a", StringComparison.Ordinal) ||
                    price.EndsWith("B", StringComparison.Ordinal) || price.EndsWith("b", StringComparison.Ordinal))
                    price = price.Substring(0, price.Length - 1);
                if (double.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    trade.price = v;
                else
                {
                    Trace.TraceError("Failed to parse price string [{0}] in [{1}] in file [{2}], skipping", price, line, filePath);
                    break;
                }
                trade.price /= 100;
                string quantity = splitted[1].Trim(' ');
                if ("0" == quantity && !Properties.Settings.Default.IncludeTradesWithZeroVolume)
                    continue;
                if (double.TryParse(quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                    trade.volume = v;
                else
                {
                    Trace.TraceError("Failed to parse quantity string [{0}] in [{1}] in file [{2}], skipping", quantity, line, filePath);
                    break;
                }
                */
                seriesEntry.TradeList.Add(trade);
            }
            //if (Properties.Settings.Default.ExportToCsv)
            //    seriesEntry.ExportCsv(future, yyyymmdd);
            //if (!Properties.Settings.Default.WriteToH5)
            //    return;
            string h5File = future.RepositoryPath();
            using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
            {
                string instrumentPath = seriesEntry.InstrumentPath(future);
                if (1 > seriesEntry.TradeList.Count)
                {
                    Trace.TraceInformation("Skipped merge {0}, {1}, to file {2}, instrument path {3}: no trade data found", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    return;
                }
                Trace.TraceInformation("Merging {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                Trace.TraceInformation("Merging {0}, {1}, starting check", seriesEntry.Code, seriesEntry.Month);
                var tradePrevious = new Trade();
                bool tradeActivated = false;
                bool isGood = true;
                foreach (var t in seriesEntry.TradeList)
                {
                    if (tradeActivated)
                    {
                        if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                        {
                            Trade t2 = t;
                            t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                            Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                            isGood = false;
                            tradePrevious = t2;
                        }
                        else
                            tradePrevious = t;
                    }
                    else
                    {
                        tradeActivated = true;
                        tradePrevious = t;
                    }
                }
                if (!isGood)
                {
                    Trace.TraceInformation("Prohibited merge {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    return;
                }
                Trace.TraceInformation("Merging {0}, {1}, finished check", seriesEntry.Code, seriesEntry.Month);

                using (Instrument instrument = repository.Open(instrumentPath, true))
                {
                    using (TradeData tradeData = instrument.OpenTrade(true))
                    {
                        Trace.TraceInformation("Merging {0}, {1}, starting SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                        tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                        Trace.TraceInformation("Merging {0}, {1}, finished SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                        int maxChunks = 1 + seriesEntry.TradeList.Count / ElementsPerAdd;
                        if (maxChunks == 1)
                        {
                            Trace.TraceInformation("Merging {0}, {1}, starting Add", seriesEntry.Code, seriesEntry.Month);
                            if (!tradeData.Add(seriesEntry.TradeList,
                                Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                Properties.Settings.Default.Hdf5VerboseAdd))
                                Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                            Trace.TraceInformation("Merging {0}, {1}, finished Add", seriesEntry.Code, seriesEntry.Month);
                        }
                        else
                        {
                            var buf = new List<Trade>(maxChunks);
                            var src = seriesEntry.TradeList;
                            int srcIndex = 0;
                            for (int chunk = 1; chunk < maxChunks; ++chunk)
                            {
                                Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                                buf.Clear();
                                for (int i = 0; i < ElementsPerAdd; ++i)
                                {
                                    buf.Add(src[srcIndex++]);
                                }
                                if (!tradeData.Add(buf,
                                    Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                    Properties.Settings.Default.Hdf5VerboseAdd))
                                    Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                                Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                            }
                            Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                            buf.Clear();
                            int c = src.Count;
                            while (srcIndex < c)
                            {
                                buf.Add(src[srcIndex++]);
                            }
                            if (!tradeData.Add(buf,
                                Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                Properties.Settings.Default.Hdf5VerboseAdd))
                                Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                            Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                        }
                    }
                }

                /*
                                instrumentPath = frontEntry.InstrumentPath(future);
                                if (1 > seriesEntry.TradeList.Count)
                                {
                                    Trace.TraceInformation("Skipped merge {0} to file {1}, instrument path {2}: no trade data found", frontEntry.Code, h5File, instrumentPath);
                                    return;
                                }
                                Trace.TraceInformation("Merging {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                                tradeActivated = false;
                                //isGood = true;
                                foreach (var t in seriesEntry.TradeList)
                                {
                                    if (tradeActivated)
                                    {
                                        if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                                        {
                                            Trade t2 = t;
                                            t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                                            Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                                            isGood = false;
                                            tradePrevious = t2;
                                        }
                                        else
                                            tradePrevious = t;
                                    }
                                    else
                                    {
                                        tradeActivated = true;
                                        tradePrevious = t;
                                    }
                                }
                                if (!isGood)
                                {
                                    Trace.TraceInformation("Prohibited merge {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                                    return;
                                }

                                using (Instrument instrument = repository.Open(instrumentPath, true))
                                {
                                    using (TradeData tradeData = instrument.OpenTrade(true))
                                    {
                                        tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                                        if (!tradeData.Add(seriesEntry.TradeList,
                                            Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                            Properties.Settings.Default.Hdf5VerboseAdd))
                                            Trace.TraceError("Failed to add trade list, {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                                    }
                                }
                */
            }
        }

        public void Import(string symbol, string yyyymmdd = null)
        {
            if (null == yyyymmdd)
                yyyymmdd = YyyymmddNow();
            symbol = symbol.ToUpperInvariant();
            Future future = futureList.Find(f => symbol == f.Symbol.ToUpperInvariant());
            if (null == future)
            {
                Trace.TraceError("Failed to find future symbol {0}, aborting", symbol);
                return;
            }
            Import(future, yyyymmdd);
        }

        private void Import(Future future, string yyyymmdd)
        {
            var trade = new Trade();
            Trace.TraceInformation("Importing {0}, {1}, [{2}]", future.Symbol, future.Name, future.Uri);
            foreach (var seriesEntry in future.Series)
            {
                Trace.TraceInformation("Importing {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}, rollover {5}",
                    seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                    seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString(), seriesEntry.RolloverDate.ToShortDateString());
                foreach (var slot in future.TimeSlotList)
                {
                    string file = seriesEntry.FilePath(future, yyyymmdd, slot.Id);
                    if (!File.Exists(file))
                    {
                        Trace.TraceError("Cannot find file [{0}], skipping", file);
                        continue;
                    }
                    XDocument xdoc = XDocument.Load(file, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                    List<XElement> xelist = xdoc.XPathSelectElements("/timeandSalesList/timeSalesTO/futuresTO/reportRecordList").ToList();
                    foreach (var xel in xelist)
                    {
                        string date = Content(xel, "saledate", file); // <saledate>04/29/2013</saledate>
                        string time = Content(xel, "saletime", file); // <saletime>16:10:52</saletime>
                        string price = Content(xel, "saleprice", file); // <saleprice>157725.0</saleprice>
                        string quantity = Content(xel, "quantity", file); // <quantity>0</quantity>
                        //string indictive = Content(xel, "indictive", file); // <indictive>Indicative</indictive>
                        if (null == date || null == time || null == price || null == quantity)
                            break;
                        if ("0" == quantity && !Properties.Settings.Default.IncludeTradesWithZeroVolume)
                            continue;
                        if (price.EndsWith("A", StringComparison.Ordinal) || price.EndsWith("a", StringComparison.Ordinal) ||
                            price.EndsWith("B", StringComparison.Ordinal) || price.EndsWith("b", StringComparison.Ordinal))
                            price = price.Substring(0, price.Length - 1);
                        string[] splitted = date.Split('/');
                        int month = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int day = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int year = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        splitted = time.Split(':');
                        int hour = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int minute = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int second = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        var dt = new DateTime(year, month, day, hour, minute, second);
                        if (slot.DayDelta != 0)
                            dt = dt.AddDays(slot.DayDelta);
                        trade.dateTimeTicks = dt.Ticks;
                        if (double.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                            trade.price = v;
                        else
                        {
                            Trace.TraceError("Failed to parse price string [{0}] in [{1}] in file [{2}], skipping", price, xel, file);
                            break;
                        }
                        trade.price /= 100;
                        if (double.TryParse(quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                            trade.volume = v;
                        else
                        {
                            Trace.TraceError("Failed to parse quantity string [{0}] in [{1}] in file [{2}], skipping", quantity, xel, file);
                            break;
                        }
                        seriesEntry.TradeList.Add(trade);
                    }
                }
                seriesEntry.TradeList.Sort((a, b) => DateTime.Compare(new DateTime(a.Ticks), new DateTime(b.Ticks)));
                if (Properties.Settings.Default.ExportToCsv)
                    seriesEntry.ExportCsv(future, yyyymmdd);
            }
            if (!Properties.Settings.Default.WriteToH5)
                return;
            string h5File = future.RepositoryPath();
            foreach (var seriesEntry in future.Series)
            {
                string instrumentPath = seriesEntry.InstrumentPath(future);
                if (1 > seriesEntry.TradeList.Count)
                {
                    Trace.TraceInformation("Skipped merge {0}, {1}, to file {2}, instrument path {3}: no trade data found", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    continue;
                }
                Trace.TraceInformation("Merging {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                Trace.TraceInformation("Merging {0}, {1}, starting check", seriesEntry.Code, seriesEntry.Month);
                var tradePrevious = new Trade();
                bool tradeActivated = false;
                bool isGood = true;
                foreach (var t in seriesEntry.TradeList)
                {
                    if (tradeActivated)
                    {
                        if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                        {
                            Trade t2 = t;
                            t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                            Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                            isGood = false;
                            tradePrevious = t2;
                        }
                        else
                            tradePrevious = t;
                    }
                    else
                    {
                        tradeActivated = true;
                        tradePrevious = t;
                    }
                }
                if (!isGood)
                {
                    Trace.TraceInformation("Prohibited merge {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    continue;
                }
                Trace.TraceInformation("Merging {0}, {1}, finished check", seriesEntry.Code, seriesEntry.Month);

                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                using (Instrument instrument = repository.Open(instrumentPath, true))
                using (TradeData tradeData = instrument.OpenTrade(true))
                {
                    if (Properties.Settings.Default.AppendOnly && tradeData.Count > 0)
                    {
                        var lastDateTimeExisting = new DateTime(tradeData.LastTicks);
                        var lastDateTimeToImport = new DateTime(seriesEntry.TradeList[seriesEntry.TradeList.Count - 1].Ticks);
                        var firstDateTimeToImport = new DateTime(seriesEntry.TradeList[0].Ticks);
                        Trace.TraceInformation("Merging {0}, {1}, file {2}, instrument path {3}: import first {4} last {5}, last existing {6}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, firstDateTimeToImport, lastDateTimeToImport, lastDateTimeExisting);
                        if (lastDateTimeToImport <= lastDateTimeExisting)
                        {
                            Trace.TraceInformation("Import data already exist: {0}, {1}, file {2}, instrument path {3}: last import {4} <= last existing {5}, skipping", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, lastDateTimeToImport, lastDateTimeExisting);
                            return;
                        }
                        else if (firstDateTimeToImport <= lastDateTimeExisting)
                        {
                            Trace.TraceInformation("Import data overlaps existing: {0}, {1}, file {2}, instrument path {3}: fist import {4} <= last existing {5}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath, firstDateTimeToImport, lastDateTimeExisting);
                            var lastExistingTicks = tradeData.LastTicks;
                            var countToRemoved = seriesEntry.TradeList.RemoveAll(d => d.Ticks <= lastExistingTicks);
                            Trace.TraceInformation("Removed {0} import elements, fist import now is {1}", countToRemoved, new DateTime(seriesEntry.TradeList[0].Ticks));
                        }
                    }
                    Trace.TraceInformation("Merging {0}, {1}, starting SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                    tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                    Trace.TraceInformation("Merging {0}, {1}, finished SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                }

                int maxChunks = 1 + seriesEntry.TradeList.Count / ElementsPerAdd;
                if (maxChunks == 1)
                {
                    Trace.TraceInformation("Merging {0}, {1}, starting Add", seriesEntry.Code, seriesEntry.Month);
                    using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    using (TradeData tradeData = instrument.OpenTrade(true))
                    {
                        if (!tradeData.Add(seriesEntry.TradeList,
                            Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                            Properties.Settings.Default.Hdf5VerboseAdd))
                            Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    }
                    Trace.TraceInformation("Merging {0}, {1}, finished Add", seriesEntry.Code, seriesEntry.Month);
                }
                else
                {
                    var buf = new List<Trade>(maxChunks);
                    var src = seriesEntry.TradeList;
                    int srcIndex = 0;
                    for (int chunk = 1; chunk < maxChunks; ++chunk)
                    {
                        Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                        buf.Clear();
                        for (int i = 0; i < ElementsPerAdd; ++i)
                        {
                            buf.Add(src[srcIndex++]);
                        }
                        using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                        using (Instrument instrument = repository.Open(instrumentPath, true))
                        using (TradeData tradeData = instrument.OpenTrade(true))
                        {
                            if (!tradeData.Add(buf,
                                Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                Properties.Settings.Default.Hdf5VerboseAdd))
                                Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                        }
                        Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                    }
                    Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                    buf.Clear();
                    int c = src.Count;
                    while (srcIndex < c)
                    {
                        buf.Add(src[srcIndex++]);
                    }
                    using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    using (TradeData tradeData = instrument.OpenTrade(true))
                    {
                        if (!tradeData.Add(buf,
                            Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                            Properties.Settings.Default.Hdf5VerboseAdd))
                            Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    }
                    Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                }
            }
        }

        private void Import2(Future future, string yyyymmdd)
        {
            var trade = new Trade();
            Trace.TraceInformation("Importing {0}, {1}, [{2}]", future.Symbol, future.Name, future.Uri);
            foreach (var seriesEntry in future.Series)
            {
                Trace.TraceInformation("Importing {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}, rollover {5}",
                    seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                    seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString(), seriesEntry.RolloverDate.ToShortDateString());
                foreach (var slot in future.TimeSlotList)
                {
                    string file = seriesEntry.FilePath(future, yyyymmdd, slot.Id);
                    if (!File.Exists(file))
                    {
                        Trace.TraceError("Cannot find file [{0}], skipping", file);
                        continue;
                    }
                    XDocument xdoc = XDocument.Load(file, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                    List<XElement> xelist = xdoc.XPathSelectElements("/timeandSalesList/timeSalesTO/futuresTO/reportRecordList").ToList();
                    foreach (var xel in xelist)
                    {
                        string date = Content(xel, "saledate", file); // <saledate>04/29/2013</saledate>
                        string time = Content(xel, "saletime", file); // <saletime>16:10:52</saletime>
                        string price = Content(xel, "saleprice", file); // <saleprice>157725.0</saleprice>
                        string quantity = Content(xel, "quantity", file); // <quantity>0</quantity>
                        //string indictive = Content(xel, "indictive", file); // <indictive>Indicative</indictive>
                        if (null == date || null == time || null == price || null == quantity)
                            break;
                        if ("0" == quantity && !Properties.Settings.Default.IncludeTradesWithZeroVolume)
                            continue;
                        if (price.EndsWith("A", StringComparison.Ordinal) || price.EndsWith("a", StringComparison.Ordinal) ||
                            price.EndsWith("B", StringComparison.Ordinal) || price.EndsWith("b", StringComparison.Ordinal))
                            price = price.Substring(0, price.Length - 1);
                        string[] splitted = date.Split('/');
                        int month = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int day = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int year = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        splitted = time.Split(':');
                        int hour = int.Parse(splitted[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int minute = int.Parse(splitted[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        int second = int.Parse(splitted[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        var dt = new DateTime(year, month, day, hour, minute, second);
                        if (slot.DayDelta != 0)
                            dt = dt.AddDays(slot.DayDelta);
                        trade.dateTimeTicks = dt.Ticks;
                        if (double.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                            trade.price = v;
                        else
                        {
                            Trace.TraceError("Failed to parse price string [{0}] in [{1}] in file [{2}], skipping", price, xel, file);
                            break;
                        }
                        trade.price /= 100;
                        if (double.TryParse(quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                            trade.volume = v;
                        else
                        {
                            Trace.TraceError("Failed to parse quantity string [{0}] in [{1}] in file [{2}], skipping", quantity, xel, file);
                            break;
                        }
                        seriesEntry.TradeList.Add(trade);
                    }
                }
                seriesEntry.TradeList.Sort((a, b) => DateTime.Compare(new DateTime(a.Ticks), new DateTime(b.Ticks)));
                if (Properties.Settings.Default.ExportToCsv)
                    seriesEntry.ExportCsv(future, yyyymmdd);
            }
            if (!Properties.Settings.Default.WriteToH5)
                return;
            string h5File = future.RepositoryPath();
            using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
            {
                foreach (var seriesEntry in future.Series)
                {
                    string instrumentPath = seriesEntry.InstrumentPath(future);
                    if (1 > seriesEntry.TradeList.Count)
                    {
                        Trace.TraceInformation("Skipped merge {0}, {1}, to file {2}, instrument path {3}: no trade data found", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                        continue;
                    }
                    Trace.TraceInformation("Merging {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                    Trace.TraceInformation("Merging {0}, {1}, starting check", seriesEntry.Code, seriesEntry.Month);
                    var tradePrevious = new Trade();
                    bool tradeActivated = false;
                    bool isGood = true;
                    foreach (var t in seriesEntry.TradeList)
                    {
                        if (tradeActivated)
                        {
                            if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                            {
                                Trade t2 = t;
                                t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                                Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                                isGood = false;
                                tradePrevious = t2;
                            }
                            else
                                tradePrevious = t;
                        }
                        else
                        {
                            tradeActivated = true;
                            tradePrevious = t;
                        }
                    }
                    if (!isGood)
                    {
                        Trace.TraceInformation("Prohibited merge {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                        continue;
                    }
                    Trace.TraceInformation("Merging {0}, {1}, finished check", seriesEntry.Code, seriesEntry.Month);

                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    {
                        using (TradeData tradeData = instrument.OpenTrade(true))
                        {
                            Trace.TraceInformation("Merging {0}, {1}, starting SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                            tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                            Trace.TraceInformation("Merging {0}, {1}, finished SpreadDuplicateTimeTicks", seriesEntry.Code, seriesEntry.Month);
                            int maxChunks = 1 + seriesEntry.TradeList.Count / ElementsPerAdd;
                            if (maxChunks == 1)
                            {
                                Trace.TraceInformation("Merging {0}, {1}, starting Add", seriesEntry.Code, seriesEntry.Month);
                                if (!tradeData.Add(seriesEntry.TradeList,
                                    Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                    Properties.Settings.Default.Hdf5VerboseAdd))
                                    Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                                Trace.TraceInformation("Merging {0}, {1}, finished Add", seriesEntry.Code, seriesEntry.Month);
                            }
                            else
                            {
                                var buf = new List<Trade>(maxChunks);
                                var src = seriesEntry.TradeList;
                                int srcIndex = 0;
                                for (int chunk = 1; chunk < maxChunks; ++chunk)
                                {
                                    Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                                    buf.Clear();
                                    for (int i = 0; i < ElementsPerAdd; ++i)
                                    {
                                        buf.Add(src[srcIndex++]);
                                    }
                                    if (!tradeData.Add(buf,
                                        Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                        Properties.Settings.Default.Hdf5VerboseAdd))
                                        Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                                    Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, chunk, maxChunks);
                                }
                                Trace.TraceInformation("Merging {0}, {1}, starting Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                                buf.Clear();
                                int c = src.Count;
                                while (srcIndex < c)
                                {
                                    buf.Add(src[srcIndex++]);
                                }
                                if (!tradeData.Add(buf,
                                    Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                    Properties.Settings.Default.Hdf5VerboseAdd))
                                    Trace.TraceError("Failed to add trade list, {0}, {1}, to file {2}, instrument path {3}", seriesEntry.Code, seriesEntry.Month, h5File, instrumentPath);
                                Trace.TraceInformation("Merging {0}, {1}, finished Add, chunk {2} of {3}", seriesEntry.Code, seriesEntry.Month, maxChunks, maxChunks);
                            }
                        }
                    }
                }
                foreach (var frontEntry in future.Front)
                {
                    SeriesEntry seriesEntry = frontEntry.SeriesEntry;
                    string instrumentPath = frontEntry.InstrumentPath(future);
                    if (1 > seriesEntry.TradeList.Count)
                    {
                        Trace.TraceInformation("Skipped merge {0} to file {1}, instrument path {2}: no trade data found", frontEntry.Code, h5File, instrumentPath);
                        continue;
                    }
                    Trace.TraceInformation("Merging {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                    var tradePrevious = new Trade();
                    bool tradeActivated = false;
                    bool isGood = true;
                    foreach (var t in seriesEntry.TradeList)
                    {
                        if (tradeActivated)
                        {
                            if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                            {
                                Trade t2 = t;
                                t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                                Trace.TraceError("Found decreasing timestamp: prev [{0}]({1}), this [{2}]({3})", tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t, t.TimeStamp.Replace(".0000000", ""));
                                isGood = false;
                                tradePrevious = t2;
                            }
                            else
                                tradePrevious = t;
                        }
                        else
                        {
                            tradeActivated = true;
                            tradePrevious = t;
                        }
                    }
                    if (!isGood)
                    {
                        Trace.TraceInformation("Prohibited merge {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                        continue;
                    }

                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    {
                        using (TradeData tradeData = instrument.OpenTrade(true))
                        {
                            tradeData.SpreadDuplicateTimeTicks(seriesEntry.TradeList, false);
                            if (!tradeData.Add(seriesEntry.TradeList,
                                Properties.Settings.Default.Hdf5UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip,
                                Properties.Settings.Default.Hdf5VerboseAdd))
                                Trace.TraceError("Failed to add trade list, {0} to file {1}, instrument path {2}", frontEntry.Code, h5File, instrumentPath);
                        }
                    }
                }
            }
        }

        private static string YyyymmddNow()
        {
            DateTime dt = DateTime.UtcNow.AddDays(-1);
            string month = dt.Month.ToString(CultureInfo.InvariantCulture);
            if (dt.Month < 10)
                month = string.Concat("0", month);
            string day = dt.Day.ToString(CultureInfo.InvariantCulture);
            if (dt.Day < 10)
                day = string.Concat("0", day);
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", dt.Year, month, day);
        }

        private static string Content(XElement xel, string name, string file)
        {
            XElement x = xel.Element(name);
            if (null == x)
            {
                Trace.TraceError("Missing [{0}] element in [{1}] in file [{2}], skipping", name, xel, file);
                return null;
            }
            return x.Value;
        }

        private static void Download(Future future, string yyyymmdd)
        {
            Trace.TraceInformation("Downloading {0}, {1}, [{2}]", future.Symbol, future.Name, future.Uri);
            // https://www.cmegroup.com/CmeWS/da/TimeandSales/V1/Report/Venue/G/Exchange/XCME/FOI/FUT/Product/ES/TimeSlot/17/ContractMonth/JUN-21
            const string uriFormat = "https://www.cmegroup.com/CmeWS/da/TimeandSales/V1/Report/Venue/G/Exchange/XCME/FOI/FUT/Product/{0}/TimeSlot/{1}/ContractMonth/{2}";
            string[] splitted = null;
            string str = Properties.Settings.Default.SkipDownloadSeriesCodes;
            if (!string.IsNullOrEmpty(str))
            {
                splitted = str.Split(';');
            }
            foreach (var seriesEntry in future.Series)
            {
                if (null != splitted)
                {
                    foreach (var code in splitted)
                    {
                        if (code == seriesEntry.Code)
                        {
                            Trace.TraceInformation("Skipping download {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}, rollover {5}",
                                seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                                seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString(), seriesEntry.RolloverDate.ToShortDateString());
                            goto labelSkipped;
                        }
                    }
                }
                Trace.TraceInformation("Downloading {0}, {1}, first trade day {2}, last trade day {3}, settlement {4}, rollover {5}",
                    seriesEntry.Code, seriesEntry.Month, seriesEntry.FirstTradeDate.ToShortDateString(),
                    seriesEntry.LastTradeDate.ToShortDateString(), seriesEntry.SettlementDate.ToShortDateString(), seriesEntry.RolloverDate.ToShortDateString());
                foreach (var slot in future.TimeSlotList)
                {
                    string uri = string.Format(CultureInfo.InvariantCulture, uriFormat, future.Symbol, slot.Id, seriesEntry.MonthShortcut);
                    string file = seriesEntry.FilePath(future, yyyymmdd, slot.Id);
                    Downloader2.Download(uri, file,
                        Properties.Settings.Default.DownloadMinimalLength, Properties.Settings.Default.DownloadOverwriteExisting,
                        Properties.Settings.Default.DownloadRetries, Properties.Settings.Default.DownloadTimeout, future.Referer,
                        Properties.Settings.Default.UserAgent);
                    UnpackGzip(file);
                }
                labelSkipped:;
            }
        }

        private static void UnpackGzip(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            string filePathNew = filePath + ".new";

            byte[] zippedBytes;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var numBytes = new FileInfo(filePath).Length;
                    zippedBytes = binaryReader.ReadBytes((int)numBytes);
                }
            }
            if (Properties.Settings.Default.CatchGunzip)
            {
                try
                {
                    using (var gZipStream = new GZipStream(new MemoryStream(zippedBytes), CompressionMode.Decompress))
                    {
                        const int bufferSize = 0x1000;
                        var buffer = new byte[bufferSize];
                        using (var fileStream = new FileStream(filePathNew, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            int bytesRead;
                            while (0 < (bytesRead = gZipStream.Read(buffer, 0, bufferSize)))
                                fileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed to gunzip downloaded data: {0}", ex.Message);
                    if (File.Exists(filePathNew))
                        File.Delete(filePathNew);
                }
            }
            else
            {
                using (var gZipStream = new GZipStream(new MemoryStream(zippedBytes), CompressionMode.Decompress))
                {
                    const int bufferSize = 0x1000;
                    var buffer = new byte[bufferSize];
                    using (var fileStream = new FileStream(filePathNew, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        int bytesRead;
                        while (0 < (bytesRead = gZipStream.Read(buffer, 0, bufferSize)))
                            fileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }

            if (File.Exists(filePathNew))
            {
                File.Delete(filePath);
                File.Move(filePathNew, filePath);
            }
        }
    }
}
