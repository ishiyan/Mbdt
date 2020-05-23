using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using Mbh5;
using System.Globalization;

namespace mbdt.LiffeUpdate
{
    internal static class LiffeImport
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

        static internal bool DoImport(string directoryOrFile, int debugTraceLevel)
        {
            bool status = true;
            try
            {
                Import(directoryOrFile, debugTraceLevel);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: [{0}]", ex.Message);
                status = false;
            }
            return status;
        }

        static private void Import(string directoryOrFile, int debugTraceLevel)
        {
            if (!Directory.Exists(Properties.Settings.Default.RepositoryPath))
                Directory.CreateDirectory(Properties.Settings.Default.RepositoryPath);
            var ohlcvList = new List<Ohlcv>(1024);
            bool alwaysCloseFile = Properties.Settings.Default.AlwaysCloseFile;
            TraverseTree(directoryOrFile, s =>
            {
                var fi = new FileInfo(s);
                if (0 == fi.Length)
                    Trace.TraceInformation("Zero length file [{0}], skipping", fi.FullName);
                else if (s.EndsWith(".csv"))
                {
                    string[] parts = fi.Name.Split('_');
                    string name = parts[0], interval = parts[2];
                    var timeFrame = DataTimeFrame.Aperiodic;
                    if (interval.StartsWith("1D"))
                        timeFrame = DataTimeFrame.Day1;
                    else if (interval.StartsWith("3600"))
                        timeFrame = DataTimeFrame.Hour1;
                    else if (interval.StartsWith("600"))
                        timeFrame = DataTimeFrame.Minute10;
                    else if (interval.StartsWith("60"))
                        timeFrame = DataTimeFrame.Minute1;
                    if (DataTimeFrame.Aperiodic == timeFrame)
                        Trace.TraceWarning("Unknown filename [{0}], skipping", fi.FullName);
                    else
                    {
                        ohlcvList.Clear();
                        if (null != name)
                        {
                            Trace.TraceInformation("Parsing [{0}] ohlcvs from [{1}]", name, fi.FullName);
                            bool hasVolume;
                            ParseFile(s, ohlcvList, out hasVolume);
                            string h5FilePath = string.Concat(Properties.Settings.Default.RepositoryPath, "\\", name, ".h5");
                            string h5InstrumentPath = string.Concat("/Liffe/", name);
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
                                        Trace.TraceError("Failed to open [{0}] in [{1}]", name, h5FilePath);
                                    else
                                    {
                                        if (hasVolume)
                                        {
                                            OhlcvData data = instrument.OpenOhlcv(OhlcvKind.Default, timeFrame, true);
                                            if (null == data)
                                                Trace.TraceError("Failed to open ohlcv data in [{0}] in [{1}]", name, h5FilePath);
                                            else
                                            {
                                                data.SpreadDuplicateTimeTicks(ohlcvList, true);
                                                if (!data.Add(ohlcvList, DuplicateTimeTicks.Skip, true))
                                                    Trace.TraceError("Error importing [{0}] parsed ohlcvs to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                                data.Flush();
                                                if (alwaysCloseFile)
                                                    data.Close();
                                            }
                                        }
                                        else
                                        {
                                            var ohlcvPriceOnly = new OhlcvPriceOnly();
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
                                            OhlcvPriceOnlyData data = instrument.OpenOhlcvPriceOnly(OhlcvKind.Default, DataTimeFrame.Day1, true);
                                            if (null == data)
                                                Trace.TraceError("Failed to open ohlcv price only data in [{0}] in [{1}]", name, h5FilePath);
                                            else
                                            {
                                                data.SpreadDuplicateTimeTicks(ohlcvPriceOnlyList, true);
                                                if (!data.Add(ohlcvPriceOnlyList, DuplicateTimeTicks.Skip, true))
                                                    Trace.TraceError("Error importing [{0}] parsed price only ohlcvs to [{1}]:[{2}]", count, h5FilePath, h5InstrumentPath);
                                                data.Flush();
                                                if (alwaysCloseFile)
                                                    data.Close();
                                            }
                                        }
                                        if (alwaysCloseFile)
                                            instrument.Close();
                                    }
                                    if (alwaysCloseFile)
                                        repository.Close();
                                }
                            }
                            else
                                Trace.TraceWarning("No [{0}] ohlcvs parsed from [{1}]", name, fi.FullName);
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
                    action(entry);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
                action(root);
        }

        static private double ConvertDouble(string s, string name, int lineNumber, string line)
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
                Trace.TraceError("Invalid csv {0}, line {1} [{2}]", name, lineNumber, line);
                value = 0;
            }
            return value;
        }

        static private void ParseFile(string file, List<Ohlcv> list, out bool hasVolume)
        {
            hasVolume = false;
            using (var csvStreamReader = new StreamReader(file, Encoding.UTF8))
            {
                const string errorFormat = "Invalid csv, line {0} [{1}] file {2}, skipping";
                //DATE;TIME;VOLUME;OPEN;CLOSE;MIN;MAX
                csvStreamReader.ReadLine();
                //02-12-2010;16:16:01;6;15.89;15.91;15.89;15.91
                string line = csvStreamReader.ReadLine();
                if (null == line)
                    return;
                int lineNumber = 1;
                var ohlcv = new Ohlcv();
                do
                {
                    string[] splitted = line.Split(';');
                    if (7 == splitted.Length)
                    {
                        int day = int.Parse(line.Substring(0, 2));
                        int month = int.Parse(line.Substring(3, 2));
                        int year = int.Parse(line.Substring(6, 4));
                        int hour = int.Parse(line.Substring(11, 2));
                        int minute = int.Parse(line.Substring(14, 2));
                        int second = int.Parse(line.Substring(17, 2));
                        var dt = new DateTime(year, month, day, hour, minute, second);
                        double volume = ConvertDouble(splitted[2], "volume", lineNumber, line);
                        double open = ConvertDouble(splitted[3], "open", lineNumber, line);
                        double close = ConvertDouble(splitted[4], "close", lineNumber, line);
                        double low = ConvertDouble(splitted[5], "low", lineNumber, line);
                        double high = ConvertDouble(splitted[6], "high", lineNumber, line);
                        if (0 < volume)
                            hasVolume = true;
                        ohlcv.dateTimeTicks = dt.Ticks;
                        ohlcv.open = open;
                        ohlcv.high = high;
                        ohlcv.low = low;
                        ohlcv.close = close;
                        ohlcv.volume = volume;
                        list.Add(ohlcv);
                    }
                    else
                    {
                        Trace.TraceError(errorFormat, lineNumber, "", Path.GetFileName(file));
                    }
                    lineNumber++;
                } while (null != (line = csvStreamReader.ReadLine()));
            }
        }
    }
}
