using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Text;
using Mbh5;

namespace SidcSsnConvert
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

        private static void CollectDat(string sourceFileName)
        {
            using (var streamReader = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = streamReader.ReadLine()))
                {
                    Debug.WriteLine($">[{line}]");
                    if (line.Length < 37)
                        Trace.TraceError("illegal line [{0}], length < 37", line);

                    string s = line.Substring(0, 10);
                    if (s[5] == ' ')
                    {
                        var sb = new StringBuilder(s) {[5] = '0'};
                        s = sb.ToString();
                    }
                    DateTime dt;
                    try
                    {
                        dt = DateTime.ParseExact(s, "yyyy MM dd", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        Trace.TraceError("cannot parse date-time [{0}] in line [{1}], skipping the line", s, line);
                        continue;
                    }

                    s = line.Substring(20, 5).Trim();
                    int rt;
                    try
                    {
                        rt = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        Trace.TraceError("cannot parse total sunspot number [{0}] in line [{1}], skipping the line", s,
                            line);
                        continue;
                    }

                    if (line.Length > 37)
                    {
                        s = line.Substring(24, 5).Trim();
                        int rn;
                        try
                        {
                            rn = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError(
                                "cannot parse north hemisphere sunspot number [{0}] in line [{1}], skipping the line",
                                s, line);
                            continue;
                        }

                        s = line.Substring(24, 5).Trim();
                        int rs;
                        try
                        {
                            rs = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError(
                                "cannot parse south hemisphere sunspot number [{0}] in line [{1}], skipping the line",
                                s, line);
                            continue;
                        }

                        RnList[dt] = rn < 0 ? double.NaN : rn;
                        RsList[dt] = rs < 0 ? double.NaN : rs;
                    }

                    RtList[dt] = rt < 0 ? double.NaN : rt;
                }
            }
        }

        private static readonly SortedList<DateTime, double> RtList = new SortedList<DateTime, double>();
        private static readonly SortedList<DateTime, double> RsList = new SortedList<DateTime, double>();
        private static readonly SortedList<DateTime, double> RnList = new SortedList<DateTime, double>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: file_name or directory_name");
            else
            {
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);

                TraverseTree(args[0], CollectDat);

                Repository.InterceptErrorStack();
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                {
                    foreach (var r in RtList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RtList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RtInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            }
                        }
                    }

                    scalarList.Clear();
                    foreach (var r in RnList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RnList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RnInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            }
                        }
                    }

                    scalarList.Clear();
                    foreach (var r in RsList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RsList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RsInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
