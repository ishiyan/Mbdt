using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using mbdt.Utils;
using Mbh5;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext endofday history utilities.
    /// </summary>
    static class EuronextEodHistory
    {
        #region Constants
        private const string History = "history";
        #endregion

        #region SplitLine
        private static string[] SplitLine(string line)
        {
            line = line.Replace("Â ", "");
            line = line.Replace("Â", "");
            line = line.Replace(" ", "");
            line = line.Replace("á", "");
            line = line.Replace(",", "");
            return line.Split(';');
        }
        #endregion

        #region Convert
        private static double Convert2(string s, string name, int lineNumber, string line, EuronextInstrumentContext context)
        {
            if (string.IsNullOrEmpty(s) || "-" == s)
                return 0;
            double value;
            try
            {
                value = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid endofday history csv2 {0}, line {1} [{2}] file {3}", name, lineNumber, line, Path.GetFileName(context.DownloadedPath));
                value = 0;
            }
            return value;
        }

        private static double Convert2H(string s, string name, string cell, string path, out bool failToConvert)
        {
            failToConvert = false;
            if (string.IsNullOrEmpty(s) || "-" == s || "0" == s)
                return 0;
            double value;
            try
            {
                value = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("invalid endofday history csv2h {0} [{1}], cell [{2}], file {3}", name, s, cell, Path.GetFileName(path));
                value = 0;
                failToConvert = true;
            }
            return value;
        }
        #endregion

        #region Extract
        private static string Extract(string text, string prefix, string suffix)
        {
            int i = text.IndexOf(prefix, StringComparison.Ordinal);
            if (i >= 0)
            {
                string s = text.Substring(i + prefix.Length);
                i = s.IndexOf(suffix, StringComparison.Ordinal);
                if (i > 0)
                {
                    s = s.Substring(0, i).Trim();
                    if (s == "-")
                        s = "";
                    return s;
                }
            }
            return "";
        }
        private static string Extract(string text, string prefix1, string prefix2, string suffix2)
        {
            int i = text.IndexOf(prefix1, StringComparison.Ordinal);
            if (i >= 0)
            {
                string s = text.Substring(i + prefix1.Length);
                i = s.IndexOf(prefix2, StringComparison.Ordinal);
                if (i > 0)
                {
                    s = s.Substring(i + prefix2.Length);
                    i = s.IndexOf(suffix2, StringComparison.Ordinal);
                    if (i > 0)
                    {
                        s = s.Substring(0, i).Trim();
                        if (s == "-")
                            s = "";
                        return s;
                    }
                }
            }
            return "";
        }
        #endregion

        #region ImportCsv2

        private static void Correct(Ohlcv ohlcv, string downloadedPath)
        {
            string prefix = downloadedPath.EndsWith(".csv2h") ? "Csv2h" : "Csv2";
            if (ohlcv.Close > double.Epsilon)
            {
                if (ohlcv.Open < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                {
                    // All three are zero, only close is present.
                    Trace.TraceError(
                        "{6} date [{0}]: open [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                        ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close,
                        Path.GetFileName(downloadedPath), prefix);
                    ohlcv.open = ohlcv.Close;
                    ohlcv.high = ohlcv.Close;
                    ohlcv.low = ohlcv.Close;
                }
                else if (Math.Abs(ohlcv.Open - ohlcv.Close) < double.Epsilon && ohlcv.High < double.Epsilon && ohlcv.Low < double.Epsilon)
                {
                    // Close equals open, high and low are zero.
                    Trace.TraceError(
                        "{6} date [{0}]: open equals close [{1}], high [{2}], low [{3}] are zeroes: replacing with close [{4}], file {5}",
                        ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close,
                        Path.GetFileName(downloadedPath), prefix);
                    ohlcv.high = ohlcv.Close;
                    ohlcv.low = ohlcv.Close;
                }
                else if (ohlcv.Open < double.Epsilon && Math.Abs(ohlcv.High - ohlcv.Close) < double.Epsilon &&
                         Math.Abs(ohlcv.Low - ohlcv.Close) < double.Epsilon)
                {
                    // All three are the same, only open is zero.
                    Trace.TraceError(
                        "{6} date [{0}]: close [{4}], high [{2}], low [{3}] are the same, open [{1}] is zero: replacing with close [{4}], file {5}",
                        ohlcv.DateStamp, ohlcv.Open, ohlcv.High, ohlcv.Low, ohlcv.Close,
                        Path.GetFileName(downloadedPath), prefix);
                    ohlcv.open = ohlcv.Close;
                }
            }

            if (ohlcv.Open < double.Epsilon || ohlcv.High < double.Epsilon || ohlcv.Low < double.Epsilon || ohlcv.Close < double.Epsilon)
                Trace.TraceError("{0} date [{1}]: found zero price: open [{2}] high [{3}] low [{4}] close [{5}], file {6}", prefix, ohlcv.DateStamp, ohlcv.Open,
                    ohlcv.High, ohlcv.Low, ohlcv.Close, Path.GetFileName(downloadedPath));
        }

        /// <summary>
        /// Imports a downloaded Euronext endofday history csv file into a list of ohlcvs.
        /// </summary>
        /// <param name="context">A EuronextInstrumentContext.</param>
        /// <param name="hasVolume">If ohlcvs have non-zero volume.</param>
        /// <returns>A list containing imported ohlcv instances.</returns>
        private static List<Ohlcv> ImportCsv2(EuronextInstrumentContext context, out bool hasVolume)
        {
            hasVolume = false;
            var ohlcvList = new List<Ohlcv>(1024);
            using (var csvStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                var ohlcv = new Ohlcv();
                const string errorFormat = "invalid endofday history csv2{0}, line {1} [{2}] file {3}, skipping";
                int lineNumber = 5;
                csvStreamReader.ReadLine();        // "Historical Data"
                csvStreamReader.ReadLine();        // "From 1998-01-01  to 2019-07-19"
                csvStreamReader.ReadLine();        // NL0012866412
                csvStreamReader.ReadLine();        // Date;Open;High;Low;Close;"Number of Shares";"Number of Trades";Turnover
                string line = csvStreamReader.ReadLine();
                // with volume:    19/07/2019;24.70;25.55;24.69;25.30;670,978;2,860;16,940,041
                // without volume: 19/07/2019;574.41;575.97;570.30;571.84;0;0;1,818,177,909
                if (!string.IsNullOrEmpty(line))
                {
                    do
                    {
                        string[] splitted = SplitLine(line);
                        if (6 <= splitted.Length)
                        {
                            string s = splitted[0];
                            if (s.Length != 10)
                                Trace.TraceError(errorFormat, " date", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            else
                            {
                                int year = 1000 * (s[6] - '0') + 100 * (s[7] - '0') + 10 * (s[8] - '0') + 1 * (s[9] - '0');
                                int month = 10 * (s[3] - '0') + 1 * (s[4] - '0');
                                int day = 10 * (s[0] - '0') + 1 * (s[1] - '0');
                                DateTime dt = new DateTime(year, month, day);

                                double open = Convert2(splitted[1], "open", lineNumber, line, context);
                                double high = Convert2(splitted[2], "high", lineNumber, line, context);
                                double low = Convert2(splitted[3], "low", lineNumber, line, context);
                                double close = Convert2(splitted[4], "close", lineNumber, line, context);
                                double volume = Convert2(splitted[5], "volume", lineNumber, line, context);
                                /*if (Math.Abs(open) < double.Epsilon)
                                    open = PickOne(close, high, low);
                                if (Math.Abs(close) < double.Epsilon)
                                    close = PickOne(open, high, low);
                                if (Math.Abs(high) < double.Epsilon)
                                    high = PickOne(open, close, low);
                                if (Math.Abs(low) < double.Epsilon)
                                    low = PickOne(open, close, high);*/
                                if (0 < volume)
                                    hasVolume = true;
                                ohlcv.dateTimeTicks = dt.Ticks;
                                ohlcv.open = open;
                                ohlcv.high = high;
                                ohlcv.low = low;
                                ohlcv.close = close;
                                ohlcv.volume = volume;
                                Correct(ohlcv, context.DownloadedPath);
                                // Starting from october 2019 the order in csv file is reversed, so ne need to insert(0)
                                // ohlcvList.Insert(0, ohlcv);
                                ohlcvList.Add(ohlcv);
                            }
                        }
                        else
                            Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvStreamReader.ReadLine()));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return ohlcvList;
                }
            }
            if (1 > ohlcvList.Count)
                Trace.TraceError("no historical data found in csv2 file {0}, skipping", Path.GetFileName(context.DownloadedPath));
            return ohlcvList;
        }

        private static List<Ohlcv> ImportCsv2H(EuronextInstrumentContext context, out bool hasVolume)
        {
            string downloadedPath = context.DownloadedPath + "h";
            hasVolume = false;
            var ohlcvList = new List<Ohlcv>(1024);
            string allText = File.ReadAllText(downloadedPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(allText))
            {
                Trace.TraceError("invalid endofday history csv2h: empty content, file {0}, giving up", Path.GetFileName(downloadedPath));
                return ohlcvList;
            }

            string value = Extract(allText, "<table ", "<tbody", "</tbody>");
            if (string.IsNullOrWhiteSpace(value))
            {
                Trace.TraceError("invalid endofday history csv2h: cannot extract <tboby ... </tbody>, file {0}, giving up", Path.GetFileName(downloadedPath));
                return ohlcvList;
            }

            string[] splitted = Regex.Split(value, @"</tr>");
            if (splitted.Length < 1)
            {
                Trace.TraceError("no historical data found in csv2h file {0}, skipping adjusted update", Path.GetFileName(context.DownloadedPath));
                return ohlcvList;
            }

            var ohlcv = new Ohlcv();
            for (int k = 0; k < splitted.Length - 1; ++k)
            {
                string line = splitted[k];
                string[] cells = Regex.Split(line, @"</td>");
                if (cells.Length < 6)
                {
                    Trace.TraceError("invalid endofday history csv2h: expected > 6 splitted cells, got {0}, row [{1}], file {2}, skipping row", cells.Length, line, Path.GetFileName(downloadedPath));
                    continue;
                }

                // class="even"> ... < td class="historical-time"><span>16/07/2019</span>
                value = Extract(cells[0], "<span>", "</span>").Trim();
                if (value.Length != 10 || value[2] != '/' || value[5] != '/')
                {
                    Trace.TraceError("invalid endofday history csv2h: date [{0}] should have 10 characters and '/' on 2 and 6 indices, row [{1}], file {2}, skipping row", value, cells[0], Path.GetFileName(downloadedPath));
                    continue;
                }
                int day = 10 * (value[0] - '0') + (value[1] - '0');
                int month = 10 * (value[3] - '0') + (value[4] - '0');
                int year = 1000 * (value[6] - '0') + 100 * (value[7] - '0') + 10 * (value[8] - '0') + (value[9] - '0');
                ohlcv.dateTimeTicks = new DateTime(year, month, day).Ticks;

                // ... <td class="historical-open">568.33
                int i = cells[1].IndexOf(">", StringComparison.Ordinal);
                value = i < 0 ? null : cells[1].Substring(i + 1).Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(value))
                {
                    Trace.TraceError("invalid endofday history csv2h: cannot extract opening price from row [{0}], file {1}, skipping row", cells[1], Path.GetFileName(downloadedPath));
                    continue;
                }
                bool failToConvert;
                ohlcv.open = Convert2H(value, "opening price", cells[1], downloadedPath, out failToConvert);
                if (failToConvert)
                    continue;

                // ... <td class="historical-high">571.82
                i = cells[2].IndexOf(">", StringComparison.Ordinal);
                value = i < 0 ? null : cells[2].Substring(i + 1).Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(value))
                {
                    Trace.TraceError("invalid endofday history csv2h: cannot extract highest price from row [{0}], file {1}, skipping row", cells[2], Path.GetFileName(downloadedPath));
                    continue;
                }
                ohlcv.high = Convert2H(value, "highest price", cells[2], downloadedPath, out failToConvert);
                if (failToConvert)
                    continue;

                // ... <td class="historical-low">567.34
                i = cells[3].IndexOf(">", StringComparison.Ordinal);
                value = i < 0 ? null : cells[3].Substring(i + 1).Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(value))
                {
                    Trace.TraceError("invalid endofday history csv2h: cannot extract lowest price from row [{0}], file {1}, skipping row", cells[3], Path.GetFileName(downloadedPath));
                    continue;
                }
                ohlcv.low = Convert2H(value, "lowest price", cells[3], downloadedPath, out failToConvert);
                if (failToConvert)
                    continue;

                // ... <td class="historical-close">571.21
                i = cells[4].IndexOf(">", StringComparison.Ordinal);
                value = i < 0 ? null : cells[4].Substring(i + 1).Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(value))
                {
                    Trace.TraceError("invalid endofday history csv2h: cannot extract closing price from row [{0}], file {1}, skipping row", cells[4], Path.GetFileName(downloadedPath));
                    continue;
                }
                ohlcv.close = Convert2H(value, "closing price", cells[4], downloadedPath, out failToConvert);
                if (failToConvert)
                    continue;

                // ... <td class="historical-volume">670,978
                // ... <td class="historical-volume">0
                i = cells[5].IndexOf(">", StringComparison.Ordinal);
                value = i < 0 ? null : cells[5].Substring(i + 1).Trim().Replace(",", "");
                if (string.IsNullOrWhiteSpace(value))
                {
                    Trace.TraceError("invalid endofday history csv2h: cannot extract volume from row [{0}], file {1}, skipping row", cells[5], Path.GetFileName(downloadedPath));
                    continue;
                }
                ohlcv.volume = Convert2H(value, "volume", cells[5], downloadedPath, out failToConvert);
                if (failToConvert)
                    continue;
                if (0 < ohlcv.volume)
                    hasVolume = true;

                Correct(ohlcv, downloadedPath);
                // Starting from october 2019 the order in csv file is reversed, so ne need to insert(0)
                // ohlcvList.Insert(0, ohlcv);
                ohlcvList.Add(ohlcv);
            }
            if (1 > ohlcvList.Count)
                Trace.TraceError("no historical data found in csv2h file {0}, skipping adjusted update", Path.GetFileName(downloadedPath));
            return ohlcvList;
        }
        #endregion

        private static bool DetermineFactor(Ohlcv ohlcvOld, Ohlcv ohlcvNew, out double factor)
        {
            factor = 1;
            // double ko = ohlcvNew.Open / ohlcvOld.Open;
            // double kh = ohlcvNew.High / ohlcvOld.High;
            // double kl = ohlcvNew.Low / ohlcvOld.Low;
            // double kc = ohlcvNew.Close / ohlcvOld.Close;
            // double kv = ohlcvOld.Volume / ohlcvNew.Volume;
            // factor = (ko + kh + kl + kc + kv) / 5;
            factor = ohlcvNew.Close / ohlcvOld.Close;
            // return factor > 0.001 && factor < 0.999;
            return (factor >= 0.01 && factor < 0.95) || (factor >= 1.05 && factor < 101);
        }

        private static bool DetermineFactor(OhlcvPriceOnly ohlcvOld, OhlcvPriceOnly ohlcvNew, out double factor)
        {
            factor = 1;
            // double ko = ohlcvNew.Open / ohlcvOld.Open;
            // double kh = ohlcvNew.High / ohlcvOld.High;
            // double kl = ohlcvNew.Low / ohlcvOld.Low;
            // double kc = ohlcvNew.Close / ohlcvOld.Close;
            // factor = (ko + kh + kl + kc) / 4;
            factor = ohlcvNew.Close / ohlcvOld.Close;
            // return factor > 0.001 && factor < 0.999;
            return (factor >= 0.01 && factor < 0.95) || (factor >= 1.05 && factor < 101);
        }

        #region Merge
        private static readonly object MergeLock = new object();

        /// <summary>
        /// Merges the downloaded csv2 file with the h5 repository file.
        /// </summary>
        /// <param name="repositoryRootPath">The repository root path.</param>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and a path to the csv2 file.</param>
        /// <returns>True if merged, false otherwise.</returns>
        private static bool Merge(string repositoryRootPath, EuronextInstrumentContext context)
        {
            bool hasVolume, hasVolumeAdjusted;
            List<Ohlcv> ohlcvList = ImportCsv2(context, out hasVolume);
            List<Ohlcv> ohlcvListAdjusted = ImportCsv2H(context, out hasVolumeAdjusted);

            if ((null == ohlcvList || 1 > ohlcvList.Count) && (null == ohlcvListAdjusted || 1 > ohlcvListAdjusted.Count))
                return false;
            string instrumentPath = context.H5InstrumentPath;
            string filePath = string.Concat(repositoryRootPath, context.H5FilePath);
            if (hasVolume != hasVolumeAdjusted)
            {
                Trace.TraceError("Logical inconsistency: not-adjusted volume={0}, adjusted volume={1}, [{2}]:[{3}]", hasVolume, hasVolumeAdjusted, filePath, instrumentPath);
                return false;
            }

            Trace.TraceInformation("Merging [{0}]:[{1}]", filePath, instrumentPath);
            EuronextInstrumentContext.VerifyFile(filePath);
            bool merged = true, mergedAdjusted = true;

            lock (MergeLock) // Multi-threaded access causes heap corruption in HDF5 library.
            {
                try
                {
                    using (Repository repository = Repository.OpenReadWrite(filePath, true, EuronextHistoryUpdate.Properties.Settings.Default.Hdf5CorkTheCache))
                    using (Instrument instrument = repository.Open(instrumentPath, true))
                    {
                        if (hasVolume)
                        {
                            if (ohlcvList != null && ohlcvList.Count > 0)
                            {
                                using (OhlcvData ohlcvData = instrument.OpenOhlcv(OhlcvKind.Default, DataTimeFrame.Day1, true))
                                {
                                    long lastTicks = ohlcvData.LastTicks;
                                    List<Ohlcv> listAfter = ohlcvList.Where(o => o.Ticks > lastTicks).ToList();

                                    /*for (int i = 1; i < listAfter.Count; ++i)
                                    {
                                        if (listAfter[i].Ticks < listAfter[i - 1].Ticks)
                                        {
                                            Trace.TraceError("Decreasing time ticks in [{0}]:[{1}]: {2} -> {3}", filePath, instrumentPath, listAfter[i - 1].DateStamp, listAfter[i].DateStamp);
                                        }
                                    }*/

                                    if (listAfter.Count > 0 && !ohlcvData.Add(listAfter, DuplicateTimeTicks.Skip,
                                            EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                    {
                                        merged = false;
                                        Trace.TraceError("Failed to add ohlcv list to not-adjusted data, [{0}]:[{1}]", filePath, instrumentPath);
                                    }
                                }
                            }

                            if (ohlcvListAdjusted != null && ohlcvListAdjusted.Count > 0)
                            {
                                using (OhlcvData ohlcvData = instrument.OpenOhlcvAdjusted(OhlcvKind.Default, DataTimeFrame.Day1, true))
                                {
                                    long firstTicks = ohlcvData.FirstTicks;
                                    long count = ohlcvData.Count;
                                    if (count < 1 || ohlcvListAdjusted[0].Ticks <= firstTicks)
                                    {
                                        if (!ohlcvData.Add(ohlcvListAdjusted, DuplicateTimeTicks.Update,
                                            EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                        {
                                            mergedAdjusted = false;
                                            Trace.TraceError("Failed to add adjusted ohlcv list to adjusted data, [{0}]:[{1}]", filePath, instrumentPath);
                                        }
                                    }
                                    else
                                    {
                                        List<Ohlcv> existingList = new List<Ohlcv>();
                                        if (!ohlcvData.Fetch(existingList, new DateTime(firstTicks - 1), new DateTime(ohlcvListAdjusted[0].Ticks + 1)))
                                        {
                                            mergedAdjusted = false;
                                            Trace.TraceError("Failed to fetch adjusted data, [{0}]:[{1}]", filePath, instrumentPath);
                                        }
                                        else
                                        {
                                            if (existingList[existingList.Count - 1].Ticks != ohlcvListAdjusted[0].Ticks)
                                                throw new ArgumentException(string.Format("Data ticks logical inconsistency: existing {0} != new {1}, [{2}]:[{3}]", existingList[existingList.Count - 1].Ticks, ohlcvListAdjusted[0].Ticks, filePath, instrumentPath));

                                            double factor;
                                            if (DetermineFactor(existingList[existingList.Count - 1], ohlcvListAdjusted[0], out factor))
                                            {
                                                Trace.TraceInformation("Determined adjustment factor [new = {0} * old] at date {1}, [{2}]:[{3}]", factor, new DateTime(ohlcvListAdjusted[0].Ticks), filePath, instrumentPath);
                                                var q = new Ohlcv();
                                                for (int i = existingList.Count - 2; i >= 0; --i)
                                                {
                                                    var e = existingList[i];
                                                    q.dateTimeTicks = e.Ticks;
                                                    q.open = e.Open * factor;
                                                    q.high = e.High * factor;
                                                    q.low = e.Low * factor;
                                                    q.close = e.Close * factor;
                                                    q.volume = e.Volume / factor;
                                                    ohlcvListAdjusted.Insert(0, q);
                                                }
                                                if (!ohlcvData.Add(ohlcvListAdjusted, DuplicateTimeTicks.Update,
                                                    EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                                {
                                                    merged = false;
                                                    Trace.TraceError("Failed to add adjusted ohlcv list to adjusted data, [{0}]:[{1}]", filePath, instrumentPath);
                                                }
                                            }
                                            else
                                            {
                                                long lastTicks = ohlcvData.LastTicks;
                                                List<Ohlcv> listAfter = ohlcvList != null ? ohlcvList.Where(o => o.Ticks > lastTicks).ToList() : new List<Ohlcv>();
                                                if (listAfter.Count > 0 && !ohlcvData.Add(listAfter, DuplicateTimeTicks.Skip,
                                                        EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                                {
                                                    merged = false;
                                                    Trace.TraceError("Failed to add adjusted ohlcv list to adjusted data, [{0}]:[{1}]", filePath, instrumentPath);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var ohlcvPriceOnly = new OhlcvPriceOnly();

                            if (ohlcvList != null && ohlcvList.Count > 0)
                            {
                                var ohlcvPriceOnlyList = new List<OhlcvPriceOnly>(ohlcvList.Count);
                                foreach (var t in ohlcvList)
                                {
                                    ohlcvPriceOnly.dateTimeTicks = t.Ticks;
                                    ohlcvPriceOnly.open = t.open;
                                    ohlcvPriceOnly.high = t.high;
                                    ohlcvPriceOnly.low = t.low;
                                    ohlcvPriceOnly.close = t.close;
                                    ohlcvPriceOnlyList.Add(ohlcvPriceOnly);
                                }

                                using (OhlcvPriceOnlyData ohlcvPriceOnlyData = instrument.OpenOhlcvPriceOnly(OhlcvKind.Default, DataTimeFrame.Day1, true))
                                {
                                    long lastTicks = ohlcvPriceOnlyData.LastTicks;
                                    List<OhlcvPriceOnly> listAfter = ohlcvPriceOnlyList.Where(o => o.Ticks > lastTicks).ToList();
                                    if (listAfter.Count > 0 && !ohlcvPriceOnlyData.Add(listAfter, DuplicateTimeTicks.Skip,
                                            EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                    {
                                        merged = false;
                                        Trace.TraceError("Failed to add not-adjusted ohlc list to price-only data, [{0}]:[{1}]", filePath, instrumentPath);
                                    }
                                }
                            }

                            if (ohlcvListAdjusted != null && ohlcvListAdjusted.Count > 0)
                            {
                                var ohlcvPriceOnlyListAdjusted = new List<OhlcvPriceOnly>(ohlcvListAdjusted.Count);
                                foreach (var t in ohlcvListAdjusted)
                                {
                                    ohlcvPriceOnly.dateTimeTicks = t.Ticks;
                                    ohlcvPriceOnly.open = t.open;
                                    ohlcvPriceOnly.high = t.high;
                                    ohlcvPriceOnly.low = t.low;
                                    ohlcvPriceOnly.close = t.close;
                                    ohlcvPriceOnlyListAdjusted.Add(ohlcvPriceOnly);
                                }

                                using (OhlcvPriceOnlyData ohlcvPriceOnlyData = instrument.OpenOhlcvAdjustedPriceOnly(OhlcvKind.Default, DataTimeFrame.Day1, true))
                                {
                                    long firstTicks = ohlcvPriceOnlyData.FirstTicks;
                                    long count = ohlcvPriceOnlyData.Count;
                                    if (count < 1 || ohlcvPriceOnlyListAdjusted[0].Ticks <= firstTicks)
                                    {
                                        if (!ohlcvPriceOnlyData.Add(ohlcvPriceOnlyListAdjusted, DuplicateTimeTicks.Update,
                                            EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                        {
                                            mergedAdjusted = false;
                                            Trace.TraceError("Failed to add adjusted ohlc list to adjusted price-only data, [{0}]:[{1}]", filePath, instrumentPath);
                                        }
                                    }
                                    else
                                    {
                                        List<OhlcvPriceOnly> existingList = new List<OhlcvPriceOnly>();
                                        if (!ohlcvPriceOnlyData.Fetch(existingList, new DateTime(firstTicks - 1), new DateTime(ohlcvPriceOnlyListAdjusted[0].Ticks + 1)))
                                        {
                                            mergedAdjusted = false;
                                            Trace.TraceError("Failed to fetch adjusted price-only data, [{0}]:[{1}]", filePath, instrumentPath);
                                        }
                                        else
                                        {
                                            if (existingList[existingList.Count - 1].Ticks != ohlcvPriceOnlyListAdjusted[0].Ticks)
                                                throw new ArgumentException(string.Format("Adjusted data ticks logical inconsistency: existing {0} != new {1}, [{2}]:[{3}]", existingList[existingList.Count - 1].Ticks, ohlcvPriceOnlyListAdjusted[0].Ticks, filePath, instrumentPath));

                                            double factor;
                                            if (DetermineFactor(existingList[existingList.Count - 1], ohlcvPriceOnlyListAdjusted[0], out factor))
                                            {
                                                Trace.TraceInformation("Determined adjustment factor price-only [new = {0} * old] at date {1}, [{2}]:[{3}]", factor, new DateTime(ohlcvPriceOnlyListAdjusted[0].Ticks), filePath, instrumentPath);
                                                for (int i = existingList.Count - 2; i >= 0; --i)
                                                {
                                                    var e = existingList[i];
                                                    ohlcvPriceOnly.dateTimeTicks = e.Ticks;
                                                    ohlcvPriceOnly.open = e.Open * factor;
                                                    ohlcvPriceOnly.high = e.High * factor;
                                                    ohlcvPriceOnly.low = e.Low * factor;
                                                    ohlcvPriceOnly.close = e.Close * factor;
                                                    ohlcvPriceOnlyListAdjusted.Insert(0, ohlcvPriceOnly);
                                                }
                                                if (!ohlcvPriceOnlyData.Add(ohlcvPriceOnlyListAdjusted, DuplicateTimeTicks.Update,
                                                    EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                                {
                                                    merged = false;
                                                    Trace.TraceError("Failed to add adjusted ohlc list to adjusted price-only data, [{0}]:[{1}]", filePath, instrumentPath);
                                                }
                                            }
                                            else
                                            {
                                                long lastTicks = ohlcvPriceOnlyData.LastTicks;
                                                List<OhlcvPriceOnly> listAfter = ohlcvPriceOnlyListAdjusted.Where(o => o.Ticks > lastTicks).ToList();
                                                if (listAfter.Count > 0 && !ohlcvPriceOnlyData.Add(listAfter, DuplicateTimeTicks.Skip,
                                                        EuronextHistoryUpdate.Properties.Settings.Default.Hdf5VerboseAdd))
                                                {
                                                    merged = false;
                                                    Trace.TraceError("Failed to add adjusted ohlc list to adjusted price-only data, [{0}]:[{1}]", filePath, instrumentPath);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    merged = false;
                    mergedAdjusted = false;
                    Trace.TraceError("Exception: [{0}]", ex.Message);
                }
            }
            return merged && mergedAdjusted;
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads endofday history data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a trailing separator.</param>
        /// <param name="days">The number of last history days to download or 0 to download all available history data.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        // ReSharper disable once UnusedParameter.Local
        private static bool Download(EuronextInstrumentContext context, string downloadDir, int days)
        {
            string uri, referer;
            string securityType = context.SecurityType.ToLowerInvariant();
            if (securityType == "index")
            {
                const string indexUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string indexRefererFormat = "https://live.euronext.com/en/product/indices/{0}-{1}/quotes";

                uri = string.Format(indexUriFormat, context.Isin, context.Mic);
                referer = string.Format(indexRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "stock")
            {
                const string stockUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string stockRefererFormat = "https://live.euronext.com/en/product/equities/{0}-{1}/quotes";

                uri = string.Format(stockUriFormat, context.Isin, context.Mic);
                referer = string.Format(stockRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "etv")
            {
                const string etvUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string etvRefererFormat = "https://live.euronext.com/en/product/etvs/{0}-{1}/quotes";

                uri = string.Format(etvUriFormat, context.Isin, context.Mic);
                referer = string.Format(etvRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "etf")
            {
                const string etfUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string etfRefererFormat = "https://live.euronext.com/en/product/etfs/{0}-{1}/quotes";

                uri = string.Format(etfUriFormat, context.Isin, context.Mic);
                referer = string.Format(etfRefererFormat, context.Isin, context.Mic);
            }
            else if (securityType == "inav")
            {
                const string inavUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string inavRefererFormat = "https://live.euronext.com/en/product/indices/{0}-{1}/quotes";

                uri = string.Format(inavUriFormat, context.Isin, context.Mic);
                referer = string.Format(inavRefererFormat, context.Isin, context.Mic);
            }
            else //if (securityType == "fund")
            {
                const string fundUriFormat = "https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/{0}-{1}";

                const string fundRefererFormat = "https://live.euronext.com/en/product/funds/{0}-{1}/quotes";

                uri = string.Format(fundUriFormat, context.Isin, context.Mic);
                referer = string.Format(fundRefererFormat, context.Isin, context.Mic);
            }

            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoh.csv2", context.Mic, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;

            Dictionary<string, string> postDictionary = new Dictionary<string, string>
            {
                { "format", "csv" },
                { "decimal_separator", "." },
                { "date_form", "d/m/Y" },
                { "op", "" }
            };

            // This is unadjusted history.
            // For example, Postman
            // POST https://live.euronext.com/en/ajax/AwlHistoricalPrice/getFullDownloadAjax/NL0012866412-XAMS
            // body: format=csv&decimal_separator=.&date_form=d/m/Y&op=
            // Returns
            // 07/05/2018; 31.70; 31.90; 30.98; 31.44; 503,155; 3,195; 15,811,267
            // 04/05/2018; 31.94; 31.94; 30.66; 31.30; 576,901; 4,395; 17,975,555
            // 03/05/2018; 62.00; 62.00; 59.80; 60.85; 445,062; 4,398; 27,125,753
            // 02/05/2018; 58.40; 61.85; 57.25; 61.65; 1,031,472; 7,189; 62,503,556
            // BESI had a 1:2 split on 04/05/2018
            if (!Downloader.DownloadPost(uri, s, EuronextInstrumentContext.HistoryDownloadMinimalLength, EuronextInstrumentContext.HistoryDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, postDictionary, referer, null, "*/*"))
                return false;
            UnpackGzip(s);

            // Now, download adjusted history.
            // POST https://live.euronext.com/en/ajax/getHistoricalPricePopup/NL0012866412-XAMS
            // body: nbSession=4000
            // Returns
            // <tr class="even">
            //   <td class="historical-time"><span>04/05/2018</span></td>
            //   <td class="historical-open">31.94</td>
            //   <td class="historical-high">31.94</td>
            //   <td class="historical-low">30.66</td>
            //   <td class="historical-close">31.30</td>
            //   <td class="historical-volume">576,901</td>
            //   <td class="historical-turnover">17,975,555</td>
            // </tr>
            // <tr class="odd">
            //   <td class="historical-time"><span>03/05/2018</span></td>
            //   <td class="historical-open">31.00</td>
            //   <td class="historical-high">31.00</td>
            //   <td class="historical-low">29.90</td>
            //   <td class="historical-close">30.425</td>
            //   <td class="historical-volume">890,124</td>
            //   <td class="historical-turnover">27,125,753</td>
            // </tr>
            uri = string.Format("https://live.euronext.com/en/ajax/getHistoricalPricePopup/{0}-{1}", context.Isin, context.Mic);
            s += "h"; // .csv2h
            EuronextInstrumentContext.VerifyFile(s);
            postDictionary.Clear();
            postDictionary.Add("nbSession", "4000");
            if (!Downloader.DownloadPost(uri, s, EuronextInstrumentContext.HistoryDownloadMinimalLength, EuronextInstrumentContext.HistoryDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, postDictionary, referer, null, "*/*"))
                return false;
            UnpackGzip(s);

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
            string directory = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, History, context.DownloadRepositorySuffix);
            string separator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            string parent = directory;
            if (directory.EndsWith(separator))
                parent = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                parent = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            parent = string.Concat(Directory.GetParent(parent).FullName, separator);
            Packager.ZipCsv2Directory(string.Concat(parent, context.Yyyymmdd, "enx_eoh.zip"), directory, true);
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs a daily update task.
        /// </summary>
        /// <param name="days">The number of history days to download.</param>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int UpdateTask(int days)
        {
            var notDownloadedListLock = new object();
            var approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            var discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, mergedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, mergedDiscoveredInstruments = 0, discoveredInstruments = 0;
            string downloadDir = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, History);

            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved instruments: {0}", DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                    if (Merge(EuronextInstrumentContext.EndofdayRepositoryPath, esc))
                        Interlocked.Increment(ref mergedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered instruments: {0}", DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                    if (Merge(EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc))
                        Interlocked.Increment(ref mergedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(approvedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    approvedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                            if (Merge(EuronextInstrumentContext.EndofdayRepositoryPath, esc))
                                Interlocked.Increment(ref mergedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(discoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    discoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                            if (Merge(EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc))
                                Interlocked.Increment(ref mergedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc => Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd));
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc => Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.js", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd));
                }
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments, mergedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments, mergedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments, mergedApprovedInstruments + mergedDiscoveredInstruments);
            Zip(context);
            Trace.TraceInformation("Zipped downloaded files.");
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region DownloadTask
        /// <summary>
        /// Performs a download task.
        /// </summary>
        /// <param name="downloadPath">The download path.</param>
        /// <param name="days">The number of history days to download.</param>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int DownloadTask(string downloadPath, int days)
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

            var notDownloadedListLock = new object();
            var approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            var discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, discoveredInstruments = 0;
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved to {0}: {1}", downloadPath, DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered to {0}: {1}", downloadPath, DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(approvedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    approvedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(discoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    discoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc => Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd));
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc => Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mic, esc.Symbol, esc.Isin, esc.Yyyymmdd));
                }
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments);
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region ImportTask
        /// <summary>
        /// Performs an import task.
        /// </summary>
        /// <param name="importPath">A path to an import directory or an import file.</param>
        /// <returns>The number of orphaned instruments.</returns>
        public static int ImportTask(string importPath)
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
            const string yyyymmdd = "";
            EuronextExecutor.Iterate(list, dictionaryApproved, dictionaryDiscovered, yyyymmdd, (xml, esc) =>
            {
                if (!esc.DownloadedPath.EndsWith(".csv2h", StringComparison.OrdinalIgnoreCase))
                {
                    Interlocked.Increment(ref totalInstruments);
                    if (Merge(xml.Substring(0, xml.IndexOf(esc.RelativePath, StringComparison.Ordinal)), esc))
                        Interlocked.Increment(ref mergedInstruments);
                }
            }, false);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History imported instruments: total {0}, merged {1}", totalInstruments, mergedInstruments);
            orphaned.ForEach(file => Trace.TraceInformation("Orphaned import file [{0}], skipped", file));
            return orphaned.Count;
        }
        #endregion
    }
}
