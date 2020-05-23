using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using Mbh5;

namespace LiborConvert
{
    static class Program
    {
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

        private static bool IsWhiteSpaceLine(string line)
        {
            return line.All(v => ' ' == v);
        }

        private static void Collect(string sourceFileName)
        {
            var dateTimeList = new List<DateTime>(32);
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                DateTime dt, dtPrev = new DateTime(0L);
                string line = sourceFile.ReadLine();
                if (null == line)
                    return;
                string[] splitted = line.Split(';');
                if ("date" != splitted[0])
                {
                    Trace.TraceError(string.Format("{0}: the first line does not conatin the date line: [{1}]", sourceFileName, line));
                    return;
                }
                int dateCount = splitted.Length;
                if (32 < dateCount)
                {
                    Trace.TraceError(string.Format("{0}: the first line has more than 31 dates: [{1}]", sourceFileName, line));
                    return;
                }
                for (int i = 1; i < dateCount; i++)
                {
                    if (!DateTime.TryParse(splitted[i], out dt))
                    {
                        Trace.TraceError(string.Format("{0}: the first line has an invalid date [{1}]: [{2}]", sourceFileName, splitted[i], line));
                        return;
                    }
                    if (dt <= dtPrev)
                    {
                        Trace.TraceInformation(string.Format("{0}: the first line does not conatin dates in ascending order: prev [{1}], next [{2}]: [{3}]", sourceFileName, dtPrev, dt, line));
                        return;
                    }
                    dateTimeList.Add(dt);
                    dtPrev = dt;
                }
                string currency = null;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (line.StartsWith(";"))
                        continue;
                    splitted = line.Split(';');
                    int splittedCount = splitted.Length;
                    string s = splitted[0];
                    switch (s)
                    {
                        case "s/n-o/n": s = "on"; goto validPeriod;
                        case "1w": case "2w": case "3w": case "4w":
                        case "1m": case "2m": case "3m": case "4m": case "5m": case "6m":
                        case "7m": case "8m": case "9m": case "10m": case "11m": case "12m":
                    validPeriod:
                            if (null == currency)
                            {
                                Trace.TraceError(string.Format("{0}: rates without currency: [{1}]", sourceFileName, line));
                                return;
                            }
                            s = string.Concat(currency, s);
                            SortedDictionary<DateTime, double> rate;
                            if (LiborRates.ContainsKey(s))
                                rate = LiborRates[s];
                            else
                            {
                                rate = new SortedDictionary<DateTime, double>();
                                LiborRates.Add(s, rate);
                            }
                            for (int i = 1; i < dateCount; i++)
                            {
                                dt = dateTimeList[i - 1];
                                double r;
                                if (splittedCount <= i
                                    || string.IsNullOrEmpty(splitted[i])
                                    || "N/A" == splitted[i].ToUpper()
                                    || "NA" == splitted[i].ToUpper()
                                    || "NO RATE" == splitted[i].ToUpper()
                                    || "NO FIX" == splitted[i].ToUpper()
                                    || "NO FIXING" == splitted[i].ToUpper()
                                    || "HOL" == splitted[i].ToUpper()
                                    || IsWhiteSpaceLine(splitted[i]))
                                    r = double.NaN;
                                else if (!double.TryParse(splitted[i], NumberStyles.Any, CultureInfo.InvariantCulture, out r))
                                {
                                    Trace.TraceError(string.Format("{0}: invalid rate value {1} = [{2}]: [{3}]", sourceFileName, i, splitted[i], line));
                                    return;
                                }
                                if (rate.ContainsKey(dt))
                                {
                                    if (!double.IsNaN(r))
                                    {
                                        double r2 = rate[dt];
                                        if (double.IsNaN(r2))
                                            rate[dt] = r;
                                        else if (Math.Abs(r - r2) > 1e-8)
                                            Trace.TraceError(string.Format("{0}: date {1} rate value new = [{2}], existing = [{3}], [{4}]", sourceFileName, dt, r, r2, line));
                                    }
                                }
                                else
                                    rate.Add(dt, r);
                            }
                            break;
                        default:
                            currency = s;
                            if (3 != currency.Length)
                            {
                                Trace.TraceError(string.Format("{0}: invalid currency: [{1}]", sourceFileName, line));
                                return;
                            }
                            break;
                    }
                }
            }
        }

        static readonly Dictionary<string, SortedDictionary<DateTime, double>> LiborRates =
            new Dictionary<string, SortedDictionary<DateTime, double>>();

        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: dir_or_file_name");
            else
            {
                Repository repository = null;
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Repository.InterceptErrorStack();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                Trace.TraceInformation("=======================================================================================");
                Trace.TraceInformation("Started: {0}", DateTime.Now);
                try
                {
                    string str = Properties.Settings.Default.RepositoryFile;
                    var fileInfo = new FileInfo(str);
                    string directoryName = fileInfo.DirectoryName;
                    if (string.IsNullOrEmpty(directoryName))
                    {
                        Trace.TraceError("Failed to obtain a directory name from repository file {0}", str);
                        return;
                    }
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache);
                    Trace.TraceInformation("Traversing...");
                    TraverseTree(args[0], Collect);
                    foreach (var kvp in LiborRates)
                    {
                        string name = kvp.Key; SortedDictionary<DateTime, double> rate = kvp.Value;
                        scalarList.Clear();
                        ScalarData scalarData;
                        if (!dataDictionary.TryGetValue(name, out scalarData))
                        {
                            Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, name), true);
                            // set hdf5 comment here???
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                            instrumentDictionary.Add(name, instrument);
                            dataDictionary.Add(name, scalarData);
                        }
                        if (null != scalarData)
                        {
                            foreach (var drp in rate)
                            {
                                scalar.dateTimeTicks = drp.Key.Ticks;
                                scalar.value = drp.Value;
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception: [{0}]", e.Message);
                }
                finally
                {
                    foreach (var kvp in dataDictionary)
                    {
                        ScalarData sd = kvp.Value;
                        sd.Flush();
                        sd.Close();
                    }
                    foreach (var kvp in instrumentDictionary)
                    {
                        kvp.Value.Close();
                    }
                    if (null != repository)
                        repository.Close();
                }
                Trace.TraceInformation("Finished: {0}", DateTime.Now);
            }
        }
    }
}
