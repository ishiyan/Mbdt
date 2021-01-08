using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.Serialization;
using System.Xml;
using mbdt.EuronextIntradayUpdate.Properties;
using mbdt.Utils;
using Mbh5;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext intraday utilities.
    /// </summary>
    static class EuronextEodIntraday
    {
        #region Constants
        private const string Intraday = "intraday";
        #endregion

        static readonly List<EuronextInstrumentContext> ApprovedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
        static readonly List<EuronextInstrumentContext> DiscoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);
        static long downloadedApprovedInstruments, mergedApprovedInstruments, approvedInstruments;
        static long downloadedDiscoveredInstruments, mergedDiscoveredInstruments, discoveredInstruments;
        private static readonly string LastWorkingDay = GetLastWorkingDay();

        private static string GetLastWorkingDay()
        {
            var date = DateTime.Today;
            var dayOfWeek = date.DayOfWeek;
            while (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
                dayOfWeek = date.DayOfWeek;
            }

            date = date.AddDays(-Settings.Default.StartDateDaysBack);

            return date.ToString("yyyy'-'MM'-'dd");
        }

        #region SecondsFromHHsMMsSS
        /// <summary>
        /// Converts a hh:mm:ss time stamp to a number of seconds from beginning of this day.
        /// </summary>
        /// <param name="hhSmmSss">A time stamp.</param>
        /// <returns>The number of seconds or -1 in case of invalid input string.</returns>
        private static int SecondsFromHhsMmsSs(string hhSmmSss)
        {
            if (hhSmmSss.Length > 7)
            {
                char c = hhSmmSss[0];
                if ('0' <= c && c <= '9')
                {
                    int value = 10 * (c - '0');
                    c = hhSmmSss[1];
                    if ('0' <= c && c <= '9')
                    {
                        value += c - '0';
                        if (0 <= value && value < 24) // Hour
                        {
                            int second = value * 3600;
                            c = hhSmmSss[2];
                            if (':' == c)
                            {
                                c = hhSmmSss[3];
                                if ('0' <= c && c <= '9')
                                {
                                    value = 10 * (c - '0');
                                    c = hhSmmSss[4];
                                    if ('0' <= c && c <= '9')
                                    {
                                        value += c - '0';
                                        if (0 <= value && value < 60) // Minute
                                        {
                                            second += value * 60;
                                            c = hhSmmSss[5];
                                            if (':' == c)
                                            {
                                                c = hhSmmSss[6];
                                                if ('0' <= c && c <= '9')
                                                {
                                                    value = 10 * (c - '0');
                                                    c = hhSmmSss[7];
                                                    if ('0' <= c && c <= '9')
                                                    {
                                                        value += c - '0';
                                                        if (0 <= value && value < 60) // Second
                                                            return second + value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }
        #endregion

        #region SplitLine
        private static string[] SplitLine(string line)
        {
            line = line.Replace("Â ", "");
            line = line.Replace("Â", "");
            line = line.Replace(" ", "");
            line = line.Replace(",", "");
            return line.Split('\t');
        }
        #endregion

        #region ParseVolume
        /// <summary>
        /// Parses a volume string.
        /// </summary>
        /// <param name="line">A string to parse.</param>
        /// <returns>A value of the volume.</returns>
        private static int ParseVolume(string line)
        {
            int volume = 0;
            foreach (char c in line)
            {
                if ('0' <= c && c <= '9')
                {
                    volume *= 10;
                    volume += c - '0';
                }
            }
            return volume;
        }
        #endregion

        #region ImportCsv
        /// <summary>
        /// Imports a downloded Euronext intraday csv file into a stack containing trades.
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="hasVolume">If ticks have non-zero volume.</param>
        /// <returns>A stack containing imported trade instances.</returns>
        private static Stack<Trade> ImportCsv(EuronextInstrumentContext context, out int jdn, out bool hasVolume)
        {
            var tickStack = new Stack<Trade>(4096);
            jdn = 0;
            hasVolume = false;
            var trade = new Trade();
            using (var csvStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                const string errorFormat = "invalid intraday csv{0}, line {1} [{2}] file {3}, skipping";
                int lineNumber = 4;
                csvStreamReader.ReadLine();        // Intraday of current trading day (21/03/08)
                csvStreamReader.ReadLine();        // Empty line
                string line = csvStreamReader.ReadLine();
                if (null == line)
                {
                    Trace.TraceError(errorFormat, " header datestamp", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
                int seconds = line.Contains("CFI classification") ? 1 : 0;
                line = csvStreamReader.ReadLine(); // ABN AMRO HOLDING<tab>NL0000301109<tab>NL0000301109<tab>AMS<tab>EURONEXT AMSTERDAM<tab>1.0<tab>Number of units<tab>AABA<tab>ESXXXX Equities<tab>20/03/08 17:35 CET
                if (null != line)
                {
                    csvStreamReader.ReadLine();    // Empty line
                    csvStreamReader.ReadLine();    // Date - time<tab>Trade id<tab>Quote<tab>Volume
                    string[] splitted = line.Split('\t');
                    string time;
                    if (8 < splitted.Length)
                    {
                        try
                        {
                            time = (9 < splitted.Length && 1 == seconds) ? splitted[9] : splitted[8];
                            jdn = JulianDayNumber.FromDdsMmsYy(time);
                        }
                        catch (Exception /*e*/)
                        {
                            //Trace.TraceError(errorFormat, string.Concat(" header datestamp (jd: ", e.Message, ")"), lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            Trace.TraceError(errorFormat, " header datestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            jdn = 0;
                            return null;
                        }
                    }
                    else
                    {
                        Trace.TraceError(errorFormat, " header datestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        return null;
                    }
                    DateTime dt0 = JulianDayNumber.ToDateTime(jdn);
                    lineNumber = 7;
                    while (null != (line = csvStreamReader.ReadLine()))
                    {
                        splitted = SplitLine(line);
                        if (4 == splitted.Length)
                        {
                            time = splitted[0];
                            if (-1 != (seconds = SecondsFromHhsMmsSs(time)))
                            {
                                double p;
                                try
                                {
                                    p = double.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat);
                                }
                                catch (Exception)
                                {
                                    Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                    p = double.NaN;
                                }
                                DateTime dt = dt0.AddSeconds(seconds);
                                trade.dateTimeTicks = dt.Ticks;
                                trade.price = p;
                                int v = ParseVolume(splitted[3]);
                                if (0 < v)
                                    hasVolume = true;
                                trade.volume = v;
                                tickStack.Push(trade);
                            }
                            else
                                Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        }
                        else
                            Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    }
                }
                else
                {
                    Trace.TraceError(errorFormat, " header datestamp", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (1 > tickStack.Count)
            {
                Trace.TraceError("no intraday data found in csv file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return tickStack;
        }
        #endregion

        #region ImportCsvh
        /// <summary>
        /// Imports a downloded Euronext intraday csvh file into a stack containing trades.
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="hasVolume">If ticks have non-zero volume.</param>
        /// <returns>A stack containing imported tick trade instances.</returns>
        private static Stack<Trade> ImportCsvh(EuronextInstrumentContext context, int jdn, out bool hasVolume)
        {
            var tickStack = new Stack<Trade>(4096);
            var trade = new Trade();
            hasVolume = false;
            DateTime dt0 = JulianDayNumber.ToDateTime(jdn);
            using (var csvhStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                const string errorFormat = "invalid intraday csvh{0}, line {1} [{2}] file {3}, skipping";
                int lineNumber = 1;
                string line = csvhStreamReader.ReadLine();
                if (null != line)
                {
                    //line = line.Replace("´╗┐", "");
                    do
                    {
                        string[] splitted = SplitLine(line);
                        if (4 == splitted.Length)
                        {
                            string time = splitted[0];
                            int seconds;
                            if (-1 != (seconds = SecondsFromHhsMmsSs(time)))
                            {
                                double p;
                                try
                                {
                                    p = double.Parse(splitted[2], CultureInfo.InvariantCulture.NumberFormat);
                                }
                                catch (Exception)
                                {
                                    Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                    p = double.NaN;
                                }
                                DateTime dt = dt0.AddSeconds(seconds);
                                trade.dateTimeTicks = dt.Ticks;
                                trade.price = p;
                                int v = ParseVolume(splitted[3]);
                                if (0 < v)
                                    hasVolume = true;
                                trade.volume = v;
                                tickStack.Push(trade);
                            }
                            else
                                Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        }
                        else
                            Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvhStreamReader.ReadLine()));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header datestamp", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (1 > tickStack.Count)
            {
                Trace.TraceError("no intraday data found in csvh file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return tickStack;
        }
        #endregion

        #region ImportJs
        private static bool ParseJs(string str, EuronextInstrumentContext context, ref Trade trade,  out int jdn, ref bool hasVolume, out int tradeId)
        {
            jdn = 0;
            tradeId = 0;
            string[] splitted = Regex.Split(str, @",""");
            if (7 > splitted.Length)
            {
                Trace.TraceError("invalid intraday js: illegal number of splitted entries in [{0}], file {1}, skipping", str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            string entry = splitted[0];
            // "ISIN":"FR0010930636"
            //           11111111112
            // 012345678901234567890
            if (!entry.StartsWith(@"""ISIN"":""") || '\"' != entry[entry.Length - 1])
            {
                Trace.TraceError("invalid intraday js: invalid [ISIN] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            entry = entry.Substring(8, entry.Length - 9); // FR0010930636
            if (entry != context.Isin)
            {
                Trace.TraceError("invalid intraday js: ISIN in instrument context [{0}] differs from [ISIN] entry [{1}] in [{2}], file {3}, skipping", context.Isin, entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            entry = splitted[2];
            // tradeId":7982
            //           111
            // 0123456789012
            if (!entry.StartsWith(@"tradeId"":") )
            {
                Trace.TraceError("invalid intraday js: invalid [tradeId] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            entry = entry.Substring(9); // 7982 or null
            try
            {
                tradeId = entry.StartsWith("null", StringComparison.OrdinalIgnoreCase) ? 0 : int.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid intraday js: invalid [tradeId] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            entry = splitted[4];
            // dateAndTime":"29\/08\/2012 09:00:02"
            //           11111111112222222222333333
            // 012345678901234567890123456789012345
            if (!entry.StartsWith(@"dateAndTime"":""") || 36 != entry.Length || '\\' != entry[16] || '/' != entry[17] || '\\' != entry[20] || '/' != entry[21] || ' ' != entry[26] || ':' != entry[29] || ':' != entry[32])
            {
                Trace.TraceError("invalid intraday js: invalid [dateAndTime] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            int day = 10 * (entry[14] - '0') + (entry[15] - '0');
            int month = 10 * (entry[18] - '0') + (entry[19] - '0');
            int year = 1000 * (entry[22] - '0') + 100 * (entry[23] - '0') + 10 * (entry[24] - '0') + (entry[25] - '0');
            int hour = 10 * (entry[27] - '0') + (entry[28] - '0');
            int minute = 10 * (entry[30] - '0') + (entry[31] - '0');
            int second = 10 * (entry[33] - '0') + (entry[34] - '0');
            jdn = JulianDayNumber.ToJdn(year, month, day);
            trade.dateTimeTicks = new DateTime(year, month, day, hour, minute, second).Ticks;

            entry = splitted[5];
            // price":"1,329.39"
            //           11111
            // 012345678901234
            if (!entry.StartsWith(@"price"":""") || '\"' != entry[entry.Length - 1])
            {
                Trace.TraceError("invalid intraday js: invalid [price] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            entry = entry.Substring(8, entry.Length - 9); // 1,329.39
            entry = entry.Replace(",", ""); // 1329.39
            try
            {
                trade.price = double.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid intraday js: invalid [price] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            entry = splitted[6];
            // numberOfShares":"1,118.00"
            // numberOfShares":"0,00"
            // numberOfShares":null
            //           111111111122
            // 0123456789012345678901
            if (entry.StartsWith(@"numberOfShares"":null", StringComparison.OrdinalIgnoreCase))
            {
                trade.volume = 0;
                hasVolume = false;
            }
            else
            {
                if (!entry.StartsWith(@"numberOfShares"":""", StringComparison.OrdinalIgnoreCase) || '\"' != entry[entry.Length - 1])
                {
                    Trace.TraceError("invalid intraday js: invalid [numberOfShares] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    return false;
                }
                entry = entry.Substring(17, entry.Length - 18); // 1,118.00 // 0,00
                entry = entry.Replace(",", ""); // 1118.00 // 000
                try
                {
                    trade.volume = double.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
                    if (0 >= trade.volume)
                        hasVolume = false;
                }
                catch (Exception)
                {
                    Trace.TraceError("invalid intraday js: invalid [numberOfShares] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    return false;
                }
            }

            //if (0 == tradeId)
            {
                entry = splitted[7];
                // TRADE_QUALIFIER":"OffBook Out of market"
                // TRADE_QUALIFIER":"OffBook Delta Neutral"
                // TRADE_QUALIFIER":"Exchange Continuous"
                //           111111111122
                // 0123456789012345678901
                if (null != entry && entry.StartsWith(@"TRADE_QUALIFIER"":""", StringComparison.OrdinalIgnoreCase) && '\"' == entry[entry.Length - 1])
                {
                    entry = entry.Substring(18, entry.Length - 19);
                    if (entry.StartsWith("OffBook", StringComparison.OrdinalIgnoreCase))
                    {
                        if (tradeId > 0)
                        {
                            Trace.TraceInformation("found [OffBook ...] trade [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                            tradeId = -2;
                        }
                        else
                        {
                            Trace.TraceInformation("found [OffBook ...] trade [{0}] in [{1}], file {2}, disabling sorting on tradeId", entry, str, Path.GetFileName(context.DownloadedPath));
                            tradeId = -1;
                        }
                    }
                    else if (entry.StartsWith("Automatic indicative index", StringComparison.OrdinalIgnoreCase)
                        || entry.StartsWith("Options liquidation index", StringComparison.OrdinalIgnoreCase)
                        || entry.StartsWith("Closing Reference index", StringComparison.OrdinalIgnoreCase))
                    {
                        Trace.TraceInformation("found [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                        tradeId = -2;
                    }
                }
            }
            return true;
        }

        private class Payload
        {
            public int TradeId;
            public Trade Trade;
        }

        /// <summary>
        /// Imports a downloded Euronext intraday js file into a stack containing trades.
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="hasVolume">If ticks have non-zero volume.</param>
        /// <returns>A stack containing imported trade instances.</returns>
        private static Stack<Trade> ImportJs(EuronextInstrumentContext context, out int jdn, out bool hasVolume)
        {
            jdn = 0;
            hasVolume = true;
            string s = File.ReadAllText(context.DownloadedPath, Encoding.UTF8);
            int i = s.IndexOf("[{", StringComparison.Ordinal);
            if (i < 0)
            {
                Trace.TraceError("no intraday data found in js file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            int tradeId, counter = 0;
            bool mustSort = false, sortId = true;
            var tickList = new List<Payload>(4096);
            var trade = new Trade();
            s = s.Substring(i + 2);
            while ((i = s.IndexOf("},{", StringComparison.Ordinal)) >= 0)
            {
                if (!ParseJs(s.Substring(0, i), context, ref trade, out jdn, ref hasVolume, out tradeId))
                    return null;
                if (-2 == tradeId)
                {
                    if (0 == counter)
                        counter = 1;
                    else if (1 == counter)
                        counter = 2;
                    s = s.Substring(i + 3);
                    continue;
                }
                if (-1 == tradeId)
                    sortId = false;
                if (tradeId != 0)
                    mustSort = true;
                else
                {
                    // The very first sample is sometimes the last sample of the previous day,
                    // so the timestamps are decreasing. This happens only with indices (tradeId = 0).
                    if (0 == counter)
                        counter = 1;
                    else if (1 == counter)
                    {
                        counter = 2;
                        if (tickList.Count > 0)
                        {
                            var dateTime = new DateTime(tickList[0].Trade.Ticks);
                            if (dateTime.Ticks > trade.Ticks && (dateTime.Hour == 16 || dateTime.Hour == 17 ||
                                dateTime.Hour == 18 || dateTime.Hour == 19 || dateTime.Hour == 20 ||
                                dateTime.Hour == 21 || dateTime.Hour == 22 || dateTime.Hour == 23))
                            {
                                Trace.TraceError("Dropped the very first entry with decreasing timestamp: first [{0}], second [{1}],  file {2}", dateTime, trade.Time, Path.GetFileName(context.DownloadedPath));
                                tickList.Clear();
                            }
                        }
                    }
                }
                tickList.Add(new Payload { TradeId = tradeId, Trade = trade });
                s = s.Substring(i + 3);
            }
            i = s.IndexOf("}]", StringComparison.Ordinal);
            if (!ParseJs(s.Substring(0, i), context, ref trade, out jdn, ref hasVolume, out tradeId))
                return null;
            if (-1 == tradeId)
                sortId = false;
            if (-2 == tradeId)
                tradeId = 0;
            else
                tickList.Add(new Payload { TradeId = tradeId, Trade = trade });
            if (tradeId > 0)
                mustSort = true;
            if (mustSort)
            {
                if (sortId)
                    tickList.Sort((p1, p2) => p1.TradeId.CompareTo(p2.TradeId));
                else
                    tickList.Sort((p1, p2) => p1.Trade.Ticks.CompareTo(p2.Trade.Ticks));
            }
            int count = tickList.Count;
            var tickStack = new Stack<Trade>(count);
            for (int j = --count; j >= 0; --j)
                tickStack.Push(tickList[j].Trade);
            return tickStack;
        }
        #endregion

        #region ImportJson

        private static void ExtractDate(string s, out int year, out int month, out int day)
        {
            // 12\/07\/2019
            year = 1000 * (s[8] - '0') + 100 * (s[9] - '0') + 10 * (s[10] - '0') + 1 * (s[11] - '0');
            month = 10 * (s[4] - '0') + 1 * (s[5] - '0');
            day = 10 * (s[0] - '0') + 1 * (s[1] - '0');
        }

        private static bool ParseJson(string str, EuronextInstrumentContext context, int year, int month, int day, ref Trade trade, out int jdn, ref bool hasVolume, out int tradeId)
        {
            // "tradeId":418817,"time":"17:38:45","price":"34.69","volume":"2,336","type":"Trading at last"
            // "tradeId":"-","time":"18:05:02","price":"567.41","volume":"-","type":"Closing Reference index"
            jdn = 0;
            tradeId = 0;
            string[] splitted = Regex.Split(str, @",""");
            if (5 > splitted.Length)
            {
                Trace.TraceError("invalid intraday json: illegal number of splitted entries in [{0}], file {1}, skipping", str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            string entry = splitted[0];
            // "tradeId":418817
            // "tradeId":"-"
            //           111111
            // 0123456789012345
            if (!entry.StartsWith(@"""tradeId"":", StringComparison.OrdinalIgnoreCase))
            {
                Trace.TraceError("invalid intraday json: invalid [tradeId] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            entry = entry.Substring(10); // 418817 or "-"
            try
            {
                tradeId = entry.StartsWith(@"""-""", StringComparison.Ordinal) ? 0 : int.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid intraday json: invalid [tradeId] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            entry = splitted[1];
            // time":"17:38:45"
            //           111111
            // 0123456789012345
            if (!entry.StartsWith(@"time"":""", StringComparison.OrdinalIgnoreCase) || 16 != entry.Length || ':' != entry[9] || ':' != entry[12] || '\"' != entry[15])
            {
                Trace.TraceError("invalid intraday json: invalid [time] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            int hour = 10 * (entry[7] - '0') + (entry[8] - '0');
            int minute = 10 * (entry[10] - '0') + (entry[11] - '0');
            int second = 10 * (entry[13] - '0') + (entry[14] - '0');
            jdn = JulianDayNumber.ToJdn(year, month, day);
            trade.dateTimeTicks = new DateTime(year, month, day, hour, minute, second).Ticks;

            entry = splitted[2];
            // price":"1,329.39"
            //           1111111
            // 01234567890123456
            if (!entry.StartsWith(@"price"":""", StringComparison.OrdinalIgnoreCase) || '\"' != entry[entry.Length - 1])
            {
                Trace.TraceError("invalid intraday json: invalid [price] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }
            entry = entry.Substring(8, entry.Length - 9); // 1,329.39
            entry = entry.Replace(",", ""); // 1329.39
            try
            {
                trade.price = double.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid intraday json: invalid [price] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                return false;
            }

            entry = splitted[3];
            // volume":"1,118.00"
            // volume":"-"
            //           11111
            // 012345678901234
            if (entry.StartsWith(@"volume"":""-""", StringComparison.OrdinalIgnoreCase))
            {
                trade.volume = 0;
                hasVolume = false;
            }
            else
            {
                if (!entry.StartsWith(@"volume"":""", StringComparison.OrdinalIgnoreCase) || '\"' != entry[entry.Length - 1])
                {
                    Trace.TraceError("invalid intraday json: invalid [volume] splitted entry [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    return false;
                }
                entry = entry.Substring(9, entry.Length - 10); // 1,118.00 // 0,00
                entry = entry.Replace(",", ""); // 1118.00 // 000
                try
                {
                    trade.volume = double.Parse(entry, CultureInfo.InvariantCulture.NumberFormat);
                    if (0 >= trade.volume)
                        hasVolume = false;
                }
                catch (Exception)
                {
                    Trace.TraceError("invalid intraday json: invalid [volume] [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    return false;
                }
            }

            entry = splitted[4];
            // type":"OffBook Out of market"
            // type":"OffBook Delta Neutral"
            // type":"Exchange Continuous"
            // type":"Trading at last"
            // type":"Auction"
            // type":"Retail Matching Facility"
            // type":"OffBook Investment funds"
            // type":"Options liquidation index"
            // type":"Closing Reference index"
            // type":"Automatic indicative index"
            // type":"Real-time index"
            // type":"Official opening index"
            // type":"Valuation Trade"
            //           111111111122
            // 0123456789012345678901
            if (null != entry && entry.StartsWith(@"type"":""", StringComparison.OrdinalIgnoreCase) && '\"' == entry[entry.Length - 1])
            {
                entry = entry.Substring(7, entry.Length - 8);
                if (entry.StartsWith("OffBook", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("found [OffBook ...] trade [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    tradeId = -2;
                }
                else if (entry.StartsWith("Automatic indicative index", StringComparison.OrdinalIgnoreCase)
                    || entry.StartsWith("Options liquidation index", StringComparison.OrdinalIgnoreCase)
                    || entry.StartsWith("Closing Reference index", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("found [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    tradeId = -2;
                }
                else if (entry.StartsWith("Valuation Trade", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("found [{0}] in [{1}], file {2}, skipping", entry, str, Path.GetFileName(context.DownloadedPath));
                    tradeId = -2;
                }
            }
            return true;
        }

        /// <summary>
        /// Imports a downloaded Euronext intraday json file into a stack containing trades.
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="hasVolume">If ticks have non-zero volume.</param>
        /// <returns>A stack containing imported trade instances.</returns>
        private static Stack<Trade> ImportJson(EuronextInstrumentContext context, out int jdn, out bool hasVolume)
        {
            jdn = 0;
            hasVolume = true;
            string s = File.ReadAllText(context.DownloadedPath, Encoding.UTF8);
            int i = s.IndexOf(",\"date\":\"", StringComparison.Ordinal);
            if (i < 0)
            {
                Trace.TraceError("no date data found in json file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            // ,"date":"12\/07\/2019",  =>  12\/07\/2019
            int year, month, day;
            ExtractDate(s.Substring(i + 9, 12), out year, out month, out day);

            i = s.IndexOf("\"rows\":[{", StringComparison.Ordinal);
            if (i < 0)
            {
                Trace.TraceError("no intraday data found in json file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }

            // bool sortId = true;
            var tickList = new List<Payload>(4096);
            var trade = new Trade();
            s = s.Substring(i + 9);
            while ((i = s.IndexOf("},{", StringComparison.Ordinal)) >= 0 || (i = s.IndexOf("}],", StringComparison.Ordinal)) >= 0)
            {
                bool hasNext = s.IndexOf("},{", StringComparison.Ordinal) >= 0;
                string z = s.Substring(0, i);
                if (z.StartsWith(@"""code""", StringComparison.OrdinalIgnoreCase))
                {
                    // s = s.Substring(i + 3);
                    break;
                }

                int tradeId;
                if (!ParseJson(z, context, year, month, day, ref trade, out jdn, ref hasVolume, out tradeId))
                    return null;
                if (-2 == tradeId)
                {
                    if (!hasNext)
                        break;
                    s = s.Substring(i + 3);
                    continue;
                }
                // if (-1 == tradeId || 0 == tradeId)
                //    sortId = false;
                tickList.Add(new Payload { TradeId = tradeId, Trade = trade });
                if (!hasNext)
                    break;
                s = s.Substring(i + 3);
            }
            /*
            if (sortId)
                tickList.Sort((p1, p2) => p1.TradeId.CompareTo(p2.TradeId));
            else
                tickList.Sort((p1, p2) => p1.Trade.Ticks.CompareTo(p2.Trade.Ticks));
            */
            int count = tickList.Count;
            var tickStack = new Stack<Trade>(count);
            for (int j = 0; j < count; ++j)
                tickStack.Push(tickList[j].Trade);
            // for (int j = --count; j >= 0; --j)
            //     tickStack.Push(tickList[j].Trade);
            return tickStack;
        }
        #endregion

        #region Merge
        private static readonly object MergeLock = new object();

        /// <summary>
        /// Merges the downloaded csv or js file with the h5 repository file.
        /// </summary>
        /// <param name="repositoryRootPath">The repository root path.</param>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and a path to the downloaded file.</param>
        /// <returns>True if merged, false otherwise.</returns>
        private static bool Merge(string repositoryRootPath, EuronextInstrumentContext context)
        {
            int jdn = JulianDayNumber.FromYyyymmdd(context.Yyyymmdd);
            bool hasVolume;
            Stack<Trade> tickStack = context.DownloadedPath.EndsWith(".json") ? ImportJson(context, out jdn, out hasVolume) :
                (context.DownloadedPath.EndsWith(".js") ?
                ImportJs(context, out jdn, out hasVolume) : context.DownloadedPath.EndsWith(".csvh") ?
                ImportCsvh(context, jdn, out hasVolume) : ImportCsv(context, out jdn, out hasVolume));
            if (null == tickStack || 1 > tickStack.Count)
                return false;
            string instrumentPath = context.H5InstrumentPath;
            string filePath = string.Concat(repositoryRootPath, context.H5FilePath);
            Debug.WriteLine("file path [{0}], instrument path [{1}]", filePath, instrumentPath);
            EuronextInstrumentContext.VerifyFile(filePath);
            bool merged = true;
            bool notFixed = true;
            var tradeList = new List<Trade>();
            var tradePriceOnlyList = new List<TradePriceOnly>();
            if (hasVolume)
            {
                tradeList.Capacity = tickStack.Count;
                if (EuronextIntradayUpdate.Properties.Settings.Default.CorrectBrokenTimestamps)
                {
                    var tradePrevious = new Trade();
                    bool tradeActivated = false;
                    foreach (var t in tickStack)
                    {
                        if (tradeActivated)
                        {
                            if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                            {
                                Trade t2 = t;
                                t2.dateTimeTicks = tradePrevious.dateTimeTicks;
                                Trace.TraceError(
                                    "Fixed decreasing timestamp: prev [{0}]({1}), this [{2}]({3}) -> [{4}]({5}), [{6}]:[{7}]",
                                    tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t,
                                    t.TimeStamp.Replace(".0000000", ""), t2,
                                    t2.TimeStamp.Replace(".0000000", ""), filePath, instrumentPath);
                                notFixed = false;
                                tradePrevious = t2;
                                tradeList.Add(t2);
                            }
                            else
                            {
                                tradePrevious = t;
                                tradeList.Add(t);
                            }
                        }
                        else
                        {
                            tradeActivated = true;
                            tradePrevious = t;
                            tradeList.Add(t);
                        }
                    }
                }
                else
                    tradeList.AddRange(tickStack);
            }
            else
            {
                var tradePriceOnly = new TradePriceOnly();
                tradePriceOnlyList.Capacity = tickStack.Count;
                if (EuronextIntradayUpdate.Properties.Settings.Default.CorrectBrokenTimestamps)
                {
                    var tradePrevious = new TradePriceOnly();
                    bool tradeActivated = false;
                    foreach (var t in tickStack)
                    {
                        tradePriceOnly.dateTimeTicks = t.dateTimeTicks;
                        tradePriceOnly.price = t.price;
                        if (tradeActivated)
                        {
                            if (t.dateTimeTicks < tradePrevious.dateTimeTicks)
                            {
                                TradePriceOnly t2 = tradePriceOnly;
                                tradePriceOnly.dateTimeTicks = tradePrevious.dateTimeTicks;
                                Trace.TraceError(
                                    "Fixed decreasing timestamp: prev [{0}]({1}), this [{2}]({3}) -> [{4}]({5}), [{6}]:[{7}]",
                                    tradePrevious, tradePrevious.TimeStamp.Replace(".0000000", ""), t2,
                                    t2.TimeStamp.Replace(".0000000", ""), tradePriceOnly,
                                    tradePriceOnly.TimeStamp.Replace(".0000000", ""), filePath,
                                    instrumentPath);
                                notFixed = false;
                                tradePriceOnlyList.Add(tradePriceOnly);
                            }
                        }
                        else
                            tradeActivated = true;

                        tradePrevious = tradePriceOnly;
                        tradePriceOnlyList.Add(tradePriceOnly);
                    }
                }
                else
                {
                    foreach (var t in tickStack)
                    {
                        tradePriceOnly.dateTimeTicks = t.dateTimeTicks;
                        tradePriceOnly.price = t.price;
                        tradePriceOnlyList.Add(tradePriceOnly);
                    }
                }
            }

            lock (MergeLock) // Multi-threaded access causes heap corruption in HDF5 library.
            {
                try
                {
                    using (Repository repository = Repository.OpenReadWrite(filePath, true,
                        EuronextIntradayUpdate.Properties.Settings.Default.Hdf5CorkTheCache))
                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    {
                        if (hasVolume)
                        {
                            using (TradeData tradeData = instrument.OpenTrade(true))
                            {
                                tradeData.SpreadDuplicateTimeTicks(tradeList, false);
                                if (!notFixed && EuronextIntradayUpdate.Properties.Settings.Default.ProhibitAdditionIfCorrectedTimestamps)
                                {
                                    tradeList.Clear();
                                    merged = false;
                                    Trace.TraceError("Prohibited addition of trade list, [{0}]:[{1}]", filePath,
                                        instrumentPath);
                                }
                                else if (!tradeData.Add(tradeList,
                                    EuronextIntradayUpdate.Properties.Settings.Default.Hdf5UpdateDuplicateTicks
                                        ? DuplicateTimeTicks.Update
                                        : DuplicateTimeTicks.Skip,
                                    EuronextIntradayUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                {
                                    merged = false;
                                    Trace.TraceError("Failed to add trade list, [{0}]:[{1}]", filePath, instrumentPath);
                                }
                            }
                        }
                        else
                        {
                            using (TradePriceOnlyData tradePriceOnlyData = instrument.OpenTradePriceOnly(true))
                            {
                                tradePriceOnlyData.SpreadDuplicateTimeTicks(tradePriceOnlyList, false);
                                if (!notFixed && EuronextIntradayUpdate.Properties.Settings.Default.ProhibitAdditionIfCorrectedTimestamps)
                                {
                                    tradePriceOnlyList.Clear();
                                    merged = false;
                                    Trace.TraceError("Prohibited addition of tradePriceOnly list, [{0}]:[{1}]",
                                        filePath, instrumentPath);
                                }
                                else if (!tradePriceOnlyData.Add(tradePriceOnlyList,
                                    EuronextIntradayUpdate.Properties.Settings.Default.Hdf5UpdateDuplicateTicks
                                        ? DuplicateTimeTicks.Update
                                        : DuplicateTimeTicks.Skip,
                                    EuronextIntradayUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                {
                                    merged = false;
                                    Trace.TraceError("Failed to add tradePriceOnly list, [{0}]:[{1}]", filePath, instrumentPath);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    merged = false;
                    Trace.TraceError("Exception: [{0}]", ex.Message);
                }
            }

            return merged;
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads intraday data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a trailing separator.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        private static bool Download(EuronextInstrumentContext context, string downloadDir)
        {
            string uri, referer;
            string securityType = context.SecurityType.ToLowerInvariant();
            if (securityType == "index")
            {
                const string indexUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string indexRefererFormat = "https://live.euronext.com/en/product/indices/{0}-{1}/quotes";

                uri = string.Format(indexUriFormat, context.Isin, context.Mic);
                referer = string.Format(indexRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "stock")
            {
                const string stockUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string stockRefererFormat = "https://live.euronext.com/en/product/equities/{0}-{1}/quotes";

                uri = string.Format(stockUriFormat, context.Isin, context.Mic);
                referer = string.Format(stockRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "etv")
            {
                const string etvUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string etvRefererFormat = "https://live.euronext.com/en/product/etvs/{0}-{1}/quotes";

                uri = string.Format(etvUriFormat, context.Isin, context.Mic);
                referer = string.Format(etvRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "etf")
            {
                const string etfUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string etfRefererFormat = "https://live.euronext.com/en/product/etfs/{0}-{1}/quotes";

                uri = string.Format(etfUriFormat, context.Isin, context.Mic);
                referer = string.Format(etfRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "inav")
            {
                const string inavUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string inavRefererFormat = "https://live.euronext.com/en/product/indices/{0}-{1}/quotes";

                uri = string.Format(inavUriFormat, context.Isin, context.Mic);
                referer = string.Format(inavRefererFormat, context.Isin, context.Mic);
            }
            else //if (securityType == "fund")
            {
                const string fundUriFormat = "https://live.euronext.com/en/ajax/getIntradayPriceFilteredData/{0}-{1}";

                const string fundRefererFormat = "https://live.euronext.com/en/product/funds/{0}-{1}/quotes";

                uri = string.Format(fundUriFormat, context.Isin, context.Mic);
                referer = string.Format(fundRefererFormat, context.Isin, context.Mic);
            }
            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoi.json", context.Mic, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;
            Dictionary<string, string> postDictionary = new Dictionary<string, string>
            {
                {"startTime", "08:00"},
                { "endTime", "20:00"},
                { "nbitems", "100000"},
                { "timezone", "CET"},
                { "date", LastWorkingDay}
            };
            if (!Downloader.DownloadPost(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, postDictionary, referer, Settings.Default.UserAgent, "application/json, text/javascript, */*"))
                    return false;
            try
            {
                UnpackGzip(s);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to unpack-gzip downloaded data: {0}", ex.Message);
                return false;
            }
            return true;
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
            catch (Exception /*ex*/)
            {
                //Trace.TraceError("Failed to gunzip downloaded data: {0}", ex.Message);
                if (File.Exists(filePathNew))
                    File.Delete(filePathNew);
            }

            if (File.Exists(filePathNew))
            {
                File.Delete(filePath);
                File.Move(filePathNew, filePath);
            }
        }
        #endregion

        #region Zip
        /// <summary>
        /// Makes a zip file from a directory of downloaded files and deletes this directory.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> to get the download repository suffix and the datestamp in YYYYMMDD format.</param>
        private static void Zip(EuronextInstrumentContext context)
        {
            string directory = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, Intraday, context.DownloadRepositorySuffix);
            string separator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            string parent = directory;
            if (directory.EndsWith(separator))
                parent = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                parent = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            parent = string.Concat(Directory.GetParent(parent).FullName, separator);
            Packager.ZipJsDirectory(string.Concat(parent, context.Yyyymmdd, "enx_eoi.zip"), directory, true);
        }
        #endregion

        #region Serialization
        private static void SerializeTo(List<EuronextExecutor.Instrument> instance, string fileName)
        {
            var dcs = new DataContractSerializer(typeof(List<EuronextExecutor.Instrument>), null, 65536, false, true, null);
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                dcs.WriteObject(fs, instance);
                fs.Close();
            }
        }

        private static List<EuronextExecutor.Instrument> DeserializeFrom(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            var ser = new DataContractSerializer(typeof(List<EuronextExecutor.Instrument>), null, 65536, false, true, null);
            var instance = (List<EuronextExecutor.Instrument>)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
            File.Delete(fileName);
            return instance;
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs a daily update task.
        /// </summary>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int UpdateTask()
        {
            const string approvedStorageFile = "EuronextIntradayUpdate.approved.storage";
            const string discoveredStorageFile = "EuronextIntradayUpdate.discovered.storage";
            var notDownloadedListLock = new object();
            List<EuronextExecutor.Instrument> approvedStorage, discoveredStorage;
            ApprovedNotDownloadedList.Clear();
            DiscoveredNotDownloadedList.Clear();
            List<List<EuronextExecutor.Instrument>> approvedList, discoveredList;

            if (File.Exists(approvedStorageFile) || File.Exists(discoveredStorageFile))
            {
                if (File.Exists(approvedStorageFile))
                {
                    approvedStorage = DeserializeFrom(approvedStorageFile);
                    Trace.TraceInformation("Deserialized from approved storage file: {0}, {1}", approvedStorageFile, DateTime.Now);
                }
                else
                    approvedStorage = new List<EuronextExecutor.Instrument>();
                if (File.Exists(discoveredStorageFile))
                {
                    discoveredStorage = DeserializeFrom(discoveredStorageFile);
                    Trace.TraceInformation("Deserialized from discovered storage file: {0}, {1}", discoveredStorageFile, DateTime.Now);
                }
                else
                    discoveredStorage = new List<EuronextExecutor.Instrument>();
                approvedList = EuronextExecutor.Split(approvedStorage, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(discoveredStorage, EuronextInstrumentContext.WorkerThreads);
            }
            else
            {
                Trace.TraceInformation("Preparing: {0}", DateTime.Now);
                approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            }

            downloadedApprovedInstruments = 0;
            mergedApprovedInstruments = 0;
            approvedInstruments = 0;
            downloadedDiscoveredInstruments = 0;
            mergedDiscoveredInstruments = 0;
            discoveredInstruments = 0;
            string downloadDir = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, Intraday);

            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved instruments: {0}", DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        ApprovedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix)))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                    if (Merge(EuronextInstrumentContext.IntradayRepositoryPath, esc))
                        Interlocked.Increment(ref mergedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        ApprovedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered instruments: {0}", DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        DiscoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix)))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                    if (Merge(EuronextInstrumentContext.IntradayDiscoveredRepositoryPath, esc))
                        Interlocked.Increment(ref mergedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        DiscoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < ApprovedNotDownloadedList.Count || 0 < DiscoveredNotDownloadedList.Count))
            {
                if (0 < ApprovedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instrument (pass {1}): {2}", ApprovedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(ApprovedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    ApprovedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                ApprovedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix)))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                            if (Merge(EuronextInstrumentContext.IntradayRepositoryPath, esc))
                                Interlocked.Increment(ref mergedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                ApprovedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                if (0 < DiscoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instrument (pass {1}): {2}", DiscoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(DiscoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    DiscoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, delegate(EuronextInstrumentContext esc, EuronextExecutor.ConsecutiveFailInfo cfi)
                    {
                        if (cfi.LimitReached)
                        {
                            lock(notDownloadedListLock)
                            {
                                DiscoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix)))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                            if (Merge(EuronextInstrumentContext.IntradayDiscoveredRepositoryPath, esc))
                                Interlocked.Increment(ref mergedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock(notDownloadedListLock)
                            {
                                DiscoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} approved   instrument: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments, mergedApprovedInstruments);
            Trace.TraceInformation("Intraday {0} discovered instrument: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments, mergedDiscoveredInstruments);
            Trace.TraceInformation("Intraday {0} both                 : total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments, mergedApprovedInstruments + mergedDiscoveredInstruments);
            if (0 < ApprovedNotDownloadedList.Count || 0 < DiscoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < ApprovedNotDownloadedList.Count)
                {
                    approvedStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} approved instrument: {1}", ApprovedNotDownloadedList.Count, DateTime.Now);
                    ApprovedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        approvedStorage.Add(new EuronextExecutor.Instrument { Isin = esc.Isin, Mep = esc.Mep, Mic = esc.Mic, Name = esc.Name, Symbol = esc.Symbol, SecurityType = esc.SecurityType, MillisecondsSince1970 = esc.MillisecondsSince1970, File = esc.RelativePath });
                    });
                    SerializeTo(approvedStorage, approvedStorageFile);
                    Trace.TraceInformation("Serialized to approved storage file: {0}", approvedStorageFile);
                }
                if (0 < DiscoveredNotDownloadedList.Count)
                {
                    discoveredStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} discovered instrument: {1}", DiscoveredNotDownloadedList.Count, DateTime.Now);
                    DiscoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        discoveredStorage.Add(new EuronextExecutor.Instrument { Isin = esc.Isin, Mep = esc.Mep, Mic = esc.Mic, Name = esc.Name, Symbol = esc.Symbol, SecurityType = esc.SecurityType, MillisecondsSince1970 = esc.MillisecondsSince1970, File = esc.RelativePath });
                    });
                    SerializeTo(discoveredStorage, discoveredStorageFile);
                    Trace.TraceInformation("Serialized to discovered storage file: {0}", discoveredStorageFile);
                }
            }
            else
            {
                Zip(context);
                Trace.TraceInformation("Zipped downloaded files.");
                if (File.Exists(approvedStorageFile))
                {
                    File.Delete(approvedStorageFile);
                    Trace.TraceInformation("Deleted approved storage file: {0}", approvedStorageFile);
                }
                if (File.Exists(discoveredStorageFile))
                {
                    File.Delete(discoveredStorageFile);
                    Trace.TraceInformation("Deleted discovered storage file: {0}", discoveredStorageFile);
                }
            }
            return ApprovedNotDownloadedList.Count + DiscoveredNotDownloadedList.Count;
        }
        #endregion

        #region DownloadTask
        /// <summary>
        /// Performs a download task.
        /// </summary>
        /// <param name="downloadPath">The download path.</param>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int DownloadTask(string downloadPath)
        {
            if (string.IsNullOrEmpty(downloadPath))
                downloadPath = "";
            else
            {
                if (!downloadPath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) && !downloadPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                    downloadPath = string.Concat(downloadPath, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
            }

            const string approvedStorageFile = "EuronextIntradayUpdate.approved.storage";
            const string discoveredStorageFile = "EuronextIntradayUpdate.discovered.storage";
            var notDownloadedListLock = new object();
            List<EuronextExecutor.Instrument> approvedStorage, discoveredStorage;
            ApprovedNotDownloadedList.Clear();
            DiscoveredNotDownloadedList.Clear();
            List<List<EuronextExecutor.Instrument>> approvedList, discoveredList;

            if (File.Exists(approvedStorageFile) || File.Exists(discoveredStorageFile))
            {
                if (File.Exists(approvedStorageFile))
                {
                    approvedStorage = DeserializeFrom(approvedStorageFile);
                    Trace.TraceInformation("Deserialized from approved storage file: {0}, {1}", approvedStorageFile, DateTime.Now);
                }
                else
                    approvedStorage = new List<EuronextExecutor.Instrument>();
                if (File.Exists(discoveredStorageFile))
                {
                    discoveredStorage = DeserializeFrom(discoveredStorageFile);
                    Trace.TraceInformation("Deserialized from discovered storage file: {0}, {1}", discoveredStorageFile, DateTime.Now);
                }
                else
                    discoveredStorage = new List<EuronextExecutor.Instrument>();
                approvedList = EuronextExecutor.Split(approvedStorage, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(discoveredStorage, EuronextInstrumentContext.WorkerThreads);
            }
            else
            {
                Trace.TraceInformation("Preparing: {0}", DateTime.Now);
                approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            }
            downloadedApprovedInstruments = 0;
            approvedInstruments = 0;
            downloadedDiscoveredInstruments = 0;
            discoveredInstruments = 0;
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved instruments to {0}: {1}", downloadPath, DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        ApprovedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        ApprovedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered instruments to {0}: {1}", downloadPath, DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        DiscoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        DiscoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < ApprovedNotDownloadedList.Count || 0 < DiscoveredNotDownloadedList.Count))
            {
                if (0 < ApprovedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instrument (pass {1}): {2}", ApprovedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(ApprovedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    ApprovedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                ApprovedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                ApprovedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                if (0 < DiscoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instrument (pass {1}): {2}", DiscoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(DiscoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    DiscoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock(notDownloadedListLock)
                            {
                                DiscoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock(notDownloadedListLock)
                            {
                                DiscoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} approved   instrument: total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments);
            Trace.TraceInformation("Intraday {0} discovered instrument: total {1}, downloaded {2}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments);
            Trace.TraceInformation("Intraday {0} both                 : total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments);
            if (0 < ApprovedNotDownloadedList.Count || 0 < DiscoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < ApprovedNotDownloadedList.Count)
                {
                    approvedStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} approved instrument: {1}", ApprovedNotDownloadedList.Count, DateTime.Now);
                    ApprovedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        approvedStorage.Add(new EuronextExecutor.Instrument { Isin = esc.Isin, Mep = esc.Mep, Mic = esc.Mic, Name = esc.Name, Symbol = esc.Symbol, SecurityType = esc.SecurityType, MillisecondsSince1970 = esc.MillisecondsSince1970, File = esc.RelativePath });
                    });
                    SerializeTo(approvedStorage, approvedStorageFile);
                    Trace.TraceInformation("Serialized to approved storage file: {0}", approvedStorageFile);
                }
                if (0 < DiscoveredNotDownloadedList.Count)
                {
                    discoveredStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} discovered instrument: {1}", DiscoveredNotDownloadedList.Count, DateTime.Now);
                    DiscoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        discoveredStorage.Add(new EuronextExecutor.Instrument { Isin = esc.Isin, Mep = esc.Mep, Mic = esc.Mic, Name = esc.Name, Symbol = esc.Symbol, SecurityType = esc.SecurityType, MillisecondsSince1970 = esc.MillisecondsSince1970, File = esc.RelativePath });
                    });
                    SerializeTo(discoveredStorage, discoveredStorageFile);
                    Trace.TraceInformation("Serialized to discovered storage file: {0}", discoveredStorageFile);
                }
            }
            else
            {
                if (File.Exists(approvedStorageFile))
                {
                    File.Delete(approvedStorageFile);
                    Trace.TraceInformation("Deleted approved storage file: {0}", approvedStorageFile);
                }
                if (File.Exists(discoveredStorageFile))
                {
                    File.Delete(discoveredStorageFile);
                    Trace.TraceInformation("Deleted discovered storage file: {0}", discoveredStorageFile);
                }
            }
            return ApprovedNotDownloadedList.Count + DiscoveredNotDownloadedList.Count;
        }
        #endregion

        #region ImportTask
        /// <summary>
        /// Performs an import task.
        /// </summary>
        /// <param name="importPath">A path to an import directory or an import file.</param>
        /// <param name="yyyymmdd">A target date.</param>
        /// <returns>The number of orphaned instruments.</returns>
        public static int ImportTask(string importPath, string yyyymmdd)
        {
            Trace.TraceInformation("Scanning {0} and {1}: {2}", EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.DiscoveredIndexPath, DateTime.Now);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryApproved = EuronextExecutor.ScanIndex(EuronextInstrumentContext.ApprovedIndexPath);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryDiscovered = EuronextExecutor.ScanIndex(EuronextInstrumentContext.DiscoveredIndexPath);
            Trace.TraceInformation("Splitting {0}: {1}", importPath, DateTime.Now);
            List<string> orphaned;
            List<List<string>> list = EuronextExecutor.Split(importPath, dictionaryApproved, dictionaryDiscovered, EuronextInstrumentContext.WorkerThreads, out orphaned);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Merging: {0}", DateTime.Now);
            long totalInstruments = 0, mergedInstruments = 0;
            EuronextExecutor.Iterate(list, dictionaryApproved, dictionaryDiscovered, yyyymmdd, (xml, esc) =>
            {
                Interlocked.Increment(ref totalInstruments);
                if (Merge(xml.Substring(0, xml.IndexOf(esc.RelativePath, StringComparison.Ordinal)), esc))
                    Interlocked.Increment(ref mergedInstruments);
            }, true);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} imported instrument: total {1}, merged {2}", yyyymmdd, totalInstruments, mergedInstruments);
            orphaned.ForEach(file => Trace.TraceInformation("Orphaned import file [{0}], skipped", file));
            return orphaned.Count;
        }
        #endregion
    }
}
