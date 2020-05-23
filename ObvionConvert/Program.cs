using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace ObvionConvert
{
    internal static class Program
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

        private static void Collect(string sourceFileName)
        {
            var list = new List<Rates>();
            int productCount = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (line.StartsWith(";"))
                        continue;
                    string[] splitted = line.Split(';');
                    Rates rates;
                    if (0 == productCount)
                    {
                        productCount = splitted.Length;
                        for (int i = 1; i < productCount; i++)
                        {
                            rates = new Rates();
                            list.Add(rates);
                            rates.Path = splitted[i];
                        }
                    }
                    else
                    {
                        DateTime dt = DateTime.Parse(splitted[0], CultureInfo.InvariantCulture);
                        for (int i = 1; i < productCount; i++)
                        {
                            rates = list[i - 1];
                            if (rates.List.ContainsKey(dt))
                            {
                                Trace.TraceError("Duplicate date [{0}]: [{1}]", splitted[0], line);
                            }
                            if (!string.IsNullOrEmpty(splitted[i]))
                            {
                                double d = double.Parse(splitted[i], CultureInfo.InvariantCulture);
                                rates.List.Add(dt, d);
                            }
                        }

                    }
                }
            }
            RateList.AddRange(list);
        }

        private class Rates
        {
            internal string Path;
            internal readonly SortedList<DateTime, double> List = new SortedList<DateTime, double>(1024);
        }

        private static readonly List<Rates> RateList = new List<Rates>();
        private static readonly Dictionary<string, ScalarData> DataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> InstrumentDictionary = new Dictionary<string, Instrument>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: dir_or_file_name");
            else
            {
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Repository.InterceptErrorStack();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                string str = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(str);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (Repository repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache))
                {
                    TraverseTree(args[0], Collect);
                    foreach (var l in RateList)
                    {
                        using (Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, l.Path), true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, l.Path.Contains("Var") ? DataTimeFrame.Month1 : DataTimeFrame.Day1, true))
                            {
                                InstrumentDictionary.Add(l.Path, instrument);
                                DataDictionary.Add(l.Path, scalarData);
                                scalarList.Clear();
                                foreach (var r in l.List)
                                {
                                    scalar.dateTimeTicks = r.Key.Ticks;
                                    scalar.value = r.Value;
                                    scalarList.Add(scalar);
                                }
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }
                    foreach (var kvp in DataDictionary)
                    {
                        ScalarData sd = kvp.Value;
                        sd.Flush();
                        sd.Close();
                    }
                    foreach (var kvp in InstrumentDictionary)
                    {
                        kvp.Value.Close();
                    }
                }
            }
        }
    }
}
