using System;

namespace mbdt.DukascopyFxUpdate
{
    internal class DukascopyFxContext
    {
        internal readonly string MonthDirectoryDownloadBase;
        internal readonly string DayDirectoryDownloadBase;
        internal readonly string MonthUriPrefix;
        internal readonly string DayUriPrefix;
        internal bool Ok;
        internal static string Referrer = "http://www.dukascopy.com";
        internal static string DownloadDir = "downloads\\dukascopy_fx";
        internal static string RepositoryPath = "repository\\dukascopy_fx";
        internal static bool DownloadOverwrite;
        //internal static int WorkerThreadDelayMilliseconds = 2000;
        internal static int DownloadTimeout = 180000;
        internal static int DownloadRetries = 10;
        internal static int DownloadLookbackDays = 10;
        internal static int DownloadLookbackMonths = 1;

        internal static readonly string[] MonthBinFiles =
        {
            "ASK_candles_hour_1.bin", "BID_candles_hour_1.bin",
            "ASK_candles_day_1.bin", "BID_candles_day_1.bin"
        };

        internal static readonly string[] DayBinFiles =
        {
            "ASK_candles_min_1.bin", "BID_candles_min_1.bin"
        };

        internal static readonly string[] TickBinFiles =
        {
            "00h_ticks.bin", "01h_ticks.bin", "02h_ticks.bin", "03h_ticks.bin",
            "04h_ticks.bin", "05h_ticks.bin", "06h_ticks.bin", "07h_ticks.bin",
            "08h_ticks.bin", "09h_ticks.bin", "10h_ticks.bin", "11h_ticks.bin",
            "12h_ticks.bin", "13h_ticks.bin", "14h_ticks.bin", "15h_ticks.bin",
            "16h_ticks.bin", "17h_ticks.bin", "18h_ticks.bin", "19h_ticks.bin",
            "20h_ticks.bin", "21h_ticks.bin", "22h_ticks.bin", "23h_ticks.bin"
        };

        internal static readonly string[] MonthBi5Files =
        {
            "ASK_candles_hour_1.bi5", "BID_candles_hour_1.bi5",
            "ASK_candles_day_1.bi5", "BID_candles_day_1.bi5"
        };

        internal static readonly string[] DayBi5Files =
        {
            "ASK_candles_min_1.bi5", "BID_candles_min_1.bi5"
        };

        internal static readonly string[] TickBi5Files =
        {
            "00h_ticks.bi5", "01h_ticks.bi5", "02h_ticks.bi5", "03h_ticks.bi5",
            "04h_ticks.bi5", "05h_ticks.bi5", "06h_ticks.bi5", "07h_ticks.bi5",
            "08h_ticks.bi5", "09h_ticks.bi5", "10h_ticks.bi5", "11h_ticks.bi5",
            "12h_ticks.bi5", "13h_ticks.bi5", "14h_ticks.bi5", "15h_ticks.bi5",
            "16h_ticks.bi5", "17h_ticks.bi5", "18h_ticks.bi5", "19h_ticks.bi5",
            "20h_ticks.bi5", "21h_ticks.bi5", "22h_ticks.bi5", "23h_ticks.bi5"
        };

        internal static string[] Symbols =
        {
            "AUDNZD", "AUDJPY", "AUDUSD",
            "CADJPY", "CHFJPY", "EURAUD", "EURCAD",
            "EURCHF", "EURGBP", "EURJPY", "EURUSD",
            "EURNOK", "EURSEK", "GBPCHF", "GBPJPY",
            "GBPUSD", "NZDUSD", "USDCAD", "USDCHF",
            "USDJPY", "USDNOK", "USDSEK"
        };

        internal DukascopyFxContext(string symbol, DateTime dateTime)
        {
            var year = dateTime.ToString("yyyy");
            var month = (dateTime.Month -1).ToString("D2");
            var day = dateTime.ToString("dd");
            const string uriPrefix = "http://www.dukascopy.com/datafeed/";
            MonthUriPrefix = string.Concat(uriPrefix, symbol, "/", year, "/", month, "/");
            DayUriPrefix = string.Concat(MonthUriPrefix, day, "/");
            MonthDirectoryDownloadBase = string.Concat(DownloadDir, "\\", symbol, "\\", year, "\\", month, "\\");
            DayDirectoryDownloadBase = string.Concat(MonthDirectoryDownloadBase, day, "\\");
        }
    }
}
