using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Globalization;

using Mbh5;

namespace mbdt.GaincapitalFxUpdate
{
    internal static class GaincapitalFxImport
    {
        private static readonly Dictionary<string, Repository> fileDictionary = new Dictionary<string, Repository>(128);

        internal static bool DoCleanup()
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

        internal static bool DoImport(string directoryOrFile, int debugTraceLevel)
        {
            bool status = true;
            try
            {
                Import(directoryOrFile, debugTraceLevel);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: [{0}], skipping", ex.Message);
                status = false;
            }
            return status;
        }

        private static void Import(string directoryOrFile, int debugTraceLevel)
        {
            if (!Directory.Exists(Properties.Settings.Default.RepositoryPath))
                Directory.CreateDirectory(Properties.Settings.Default.RepositoryPath);
            var quoteList = new List<QuotePriceOnly>(1024);
            bool alwaysCloseFile = Properties.Settings.Default.AlwaysCloseFile;
            TraverseTree(directoryOrFile, s =>
            {
                var fi = new FileInfo(s);
                if (0 == fi.Length)
                    Trace.TraceInformation("Zero length file [{0}], skipping", fi.FullName);
                else if (s.EndsWith(".zip"))
                {
                    string pair = ParseFile(s, quoteList);
                    int count = quoteList.Count;
                    if (null != pair && count > 0)
                    {
                        string h5 = string.Concat(Properties.Settings.Default.RepositoryPath, pair, ".h5");
                        string ds = string.Concat(Properties.Settings.Default.RootPath, pair);
                        Trace.TraceInformation("Importing [{0}] parsed ticks to [{1}:{2}]", count, h5, ds);
                        Repository repository;
                        if (!fileDictionary.TryGetValue(h5, out repository))
                        {
                            repository = Repository.OpenReadWrite(h5, true, Properties.Settings.Default.CorkTheCache);
                            if (null != repository && !alwaysCloseFile)
                                fileDictionary.Add(h5, repository);
                        }
                        if (null == repository)
                            Trace.TraceError("Failed to open [{0}]", h5);
                        else
                        {
                            Instrument instrument = repository.Open(ds, true);
                            if (null == instrument)
                                Trace.TraceError("Failed to open [{0}] in [{1}]", ds, h5);
                            else
                            {
                                QuotePriceOnlyData data = instrument.OpenQuotePriceOnly(true);
                                if (null == data)
                                    Trace.TraceError("Failed to open quote data in [{0}] in [{1}]", ds, h5);
                                else
                                {
                                    Trace.TraceInformation("Spreading [{0}] [{1}] parsed ticks", count, pair);
                                    data.SpreadDuplicateTimeTicks(quoteList, Properties.Settings.Default.VerboseSpreadTicks);
                                    Trace.TraceInformation("Adding [{0}] [{1}] parsed ticks", count, pair);
                                    if (!data.Add(quoteList, Properties.Settings.Default.UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip, Properties.Settings.Default.VerboseAddTicks))
                                        Trace.TraceError("Error importing [{0}] parsed ticks to [{1}:{2}]", count, h5, ds);
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
                    if (0 < count && 0 < debugTraceLevel)
                    {
                        if (1 == debugTraceLevel)
                        {
                            QuotePriceOnly q = quoteList[0];
                            Trace.WriteLine($"{new DateTime(q.dateTimeTicks)} ms {new DateTime(q.dateTimeTicks).Millisecond}, ask {q.askPrice}, bid {q.bidPrice}");
                            q = quoteList[count - 1];
                            Trace.WriteLine($"... {count - 2} ...");
                            Trace.WriteLine($"{new DateTime(q.dateTimeTicks)} ms {new DateTime(q.dateTimeTicks).Millisecond}, ask {q.askPrice}, bid {q.bidPrice}");
                        }
                        else if (2 <= debugTraceLevel)
                        {
                            foreach (var v in quoteList)
                            {
                                Trace.WriteLine($"{new DateTime(v.dateTimeTicks)} ms {new DateTime(v.dateTimeTicks).Millisecond}, ask {v.askPrice}, bid {v.bidPrice}");
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
                    action(entry);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
                action(root);
        }

        private static DateTime TimeToTicks(string input)
        {
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(input.Substring(11, 2).Trim(), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(14, 2).Trim(), CultureInfo.InvariantCulture);
            int second = int.Parse(input.Substring(17, 2).Trim(), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, hour, minute, second);
        }

        private static string ParseFile(string zipFile, List<QuotePriceOnly> list)
        {
            list.Clear();
            if (!File.Exists(zipFile))
            {
                Trace.TraceError(String.Concat("Zip file does not exist: ", zipFile));
                return null;
            }
            var fileInfo = new FileInfo(zipFile);
            if (0 == fileInfo.Length)
            {
                Trace.TraceError(String.Concat("Zip file has zero length: ", zipFile));
                return null;
            }
            ZipStorer zip = ZipStorer.Open(zipFile, FileAccess.Read);
            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
            bool newFormat = false, formatDetected = false;
            var mbQuote = new QuotePriceOnly();
            string pair = null;
            var sortedIdList = new SortedList<long, QuotePriceOnly>(1024);
            var sortedTimeList = new SortedList<DateTime, QuotePriceOnly>(1024);
            foreach (var v in dir)
            {
                Trace.TraceInformation("Extracting [{0}] from [{1}]", v.FilenameInZip, zipFile);
                bool timeSort;
                using (var stream = new MemoryStream(1024 * 256))
                {
                    zip.ExtractFile(v, stream);
                    stream.Position = 0;
                    bool lidSeq = false;
                    timeSort = false;
                    long lid = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        Trace.TraceInformation("Parsing [{0}]", v.FilenameInZip);
                        string line;
                        while (null != (line = reader.ReadLine()))
                        {
                            if (0 == line.Length)
                                continue;
                            string [] splitted = line.Split(',');
                            if (6 != splitted.Length && 5 != splitted.Length)
                                Trace.TraceError("Illegal line with [{0}] fields, 6 expected: [{1}]", splitted.Length, line);
                            else
                            {
                                if (!formatDetected)
                                {
                                    if (splitted[0].StartsWith("lTid"))
                                    {
                                        formatDetected = true;
                                        newFormat = true;
                                        continue;
                                    }
                                    if (splitted[1].Length == 1 && splitted[1][0] == 'D')
                                    {
                                        formatDetected = true;
                                        newFormat = true;
                                    }
                                    else if (5 == splitted.Length)
                                        formatDetected = true;
                                    else if (splitted[5].StartsWith("D") || splitted[5].StartsWith("\"D\""))
                                        formatDetected = true;
                                    else
                                    {
                                        Trace.TraceError("Unable to detect the format, cannot locate the D field: [{0}]", line);
                                        continue;
                                    }
                                }
                                DateTime dt;
                                double ask;
                                double bid;
                                if (newFormat && splitted[3].Contains(':'))
                                {
                                    dt = TimeToTicks(splitted[3].Replace("\"", ""));
                                    bid = double.Parse(splitted[4].Replace("\"", ""), CultureInfo.InvariantCulture);
                                    ask = double.Parse(splitted[5].Replace("\"", ""), CultureInfo.InvariantCulture);
                                    if (null == pair)
                                        pair = splitted[2].Replace("\"", "").Replace("/", "");
                                }
                                else
                                {
                                    dt = TimeToTicks(splitted[2].Replace("\"", ""));
                                    bid = double.Parse(splitted[3].Replace("\"", ""), CultureInfo.InvariantCulture);
                                    ask = double.Parse(splitted[4].Replace("\"", ""), CultureInfo.InvariantCulture);
                                    if (null == pair)
                                        pair = splitted[1].Replace("\"", "").Replace("/", "");
                                }
                                if (splitted[0].StartsWith("TIS"))
                                    timeSort = true;
                                if (timeSort)
                                {
                                    mbQuote.askPrice = ask;
                                    mbQuote.bidPrice = bid;
                                    while (sortedTimeList.ContainsKey(dt))
                                    {
                                        dt = dt.AddTicks(1);
                                    }
                                    mbQuote.dateTimeTicks = dt.Ticks;
                                    sortedTimeList.Add(dt, mbQuote);
                                }
                                else
                                {
                                    if (lidSeq)
                                        lid++;
                                    else
                                    {
                                        string s = splitted[0].Replace("\"", "");
                                        if (s.StartsWith("SEQ"))
                                        {
                                            lidSeq = true;
                                            lid++;
                                        }
                                        else
                                            lid = long.Parse(s, CultureInfo.InvariantCulture);
                                    }
                                    mbQuote.dateTimeTicks = dt.Ticks;
                                    mbQuote.askPrice = ask;
                                    mbQuote.bidPrice = bid;
                                    if (sortedIdList.ContainsKey(lid))
                                    {
                                        Trace.TraceError("Duplicate lid [{0}], skipping line: [{1}]", lid, line);
                                    }
                                    else
                                        sortedIdList.Add(lid, mbQuote);
                                }
                            }
                        }
                    }
                }
                if (Properties.Settings.Default.DropDuplicateRecords)
                {
                    var qPrev = new QuotePriceOnly();
                    bool qPrevActivated = false;
                    if (timeSort)
                    {
                        foreach (var q in sortedTimeList.Values)
                        {
                            if (qPrevActivated)
                            {
                                if (qPrev.dateTimeTicks != q.dateTimeTicks ||
                                    Math.Abs(qPrev.askPrice - q.askPrice) > 1e-8 ||
                                    Math.Abs(qPrev.bidPrice - q.bidPrice) > 1e-8)
                                {
                                    list.Add(q);
                                }
                            }
                            else
                            {
                                qPrevActivated = true;
                                list.Add(q);
                            }
                            qPrev = q;
                        }
                    }
                    else
                    {
                        foreach (var q in sortedIdList.Values)
                        {
                            if (qPrevActivated)
                            {
                                if (qPrev.dateTimeTicks != q.dateTimeTicks ||
                                    Math.Abs(qPrev.askPrice - q.askPrice) > 1e-8 ||
                                    Math.Abs(qPrev.bidPrice - q.bidPrice) > 1e-8)
                                {
                                    list.Add(q);
                                }
                            }
                            else
                            {
                                qPrevActivated = true;
                                list.Add(q);
                            }
                            qPrev = q;
                        }
                    }
                }
                else
                {
                    list.AddRange(timeSort ? sortedTimeList.Values : sortedIdList.Values);
                }
                sortedIdList.Clear();
                sortedTimeList.Clear();
            }
            Trace.TraceInformation("Finished [{0}]", zipFile);
            zip.Close();
            zip.Dispose();
            return pair;
        }
    }
}
