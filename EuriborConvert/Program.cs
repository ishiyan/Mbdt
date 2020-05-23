using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace EuriborConvert
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

        private static void Collect(string sourceFileName)
        {
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (line.StartsWith(";"))
                        continue;
                    string[] splitted = line.Split(';');
                    var rates = new Rates {DateTime = DateTime.Parse(splitted[0], CultureInfo.InvariantCulture)};
                    rates.Euribor[0] = splitted[1];
                    if (16 == splitted.Length)
                    {
                        if (!string.IsNullOrEmpty(splitted[2]))
                            rates.Euribor[1] = splitted[2];
                        if (!string.IsNullOrEmpty(splitted[3]))
                            rates.Euribor[2] = splitted[3];
                        rates.Euribor[3] = splitted[4];
                        rates.Euribor[4] = splitted[5];
                        rates.Euribor[5] = splitted[6];
                        rates.Euribor[6] = splitted[7];
                        rates.Euribor[7] = splitted[8];
                        rates.Euribor[8] = splitted[9];
                        rates.Euribor[9] = splitted[10];
                        rates.Euribor[10] = splitted[11];
                        rates.Euribor[11] = splitted[12];
                        rates.Euribor[12] = splitted[13];
                        rates.Euribor[13] = splitted[14];
                        rates.Euribor[14] = splitted[15];
                    }
                    else if (14 == splitted.Length)
                    {
                        rates.Euribor[3] = splitted[2];
                        rates.Euribor[4] = splitted[3];
                        rates.Euribor[5] = splitted[4];
                        rates.Euribor[6] = splitted[5];
                        rates.Euribor[7] = splitted[6];
                        rates.Euribor[8] = splitted[7];
                        rates.Euribor[9] = splitted[8];
                        rates.Euribor[10] = splitted[9];
                        rates.Euribor[11] = splitted[10];
                        rates.Euribor[12] = splitted[11];
                        rates.Euribor[13] = splitted[12];
                        rates.Euribor[14] = splitted[13];
                    }
                    else
                        Trace.TraceError("file {0}: illegal line [{1}]", sourceFileName, line);
                    rateList.Add(rates);
                }
            }
        }

        private class Rates
        {
            internal DateTime DateTime;
            internal readonly List<string> Euribor = new List<string>(16);
            internal Rates()
            {
                for (int i = 0; i < 15; i++)
                    Euribor.Add(null);
            }
        }

        private static readonly List<Rates> rateList = new List<Rates>();
        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();

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
                Repository repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache);

                var nameList = new List<string> {"EURIBOR1W", "EURIBOR2W", "EURIBOR3W", "EURIBOR1M", "EURIBOR2M", "EURIBOR3M", "EURIBOR4M", "EURIBOR5M", "EURIBOR6M", "EURIBOR7M", "EURIBOR8M", "EURIBOR9M", "EURIBOR10M", "EURIBOR11M", "EURIBOR12M"};
                TraverseTree(args[0], Collect);
                for (int i = 0; i < 15; i++)
                {
                    Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, nameList[i]), true);
                    // set hdf5 comment here???
                    ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    instrumentDictionary.Add(nameList[i], instrument);
                    dataDictionary.Add(nameList[i], scalarData);
                    scalarList.Clear();
                    foreach (var r in rateList)
                    {
                        if (null != r.Euribor[i])
                        {
                            scalar.dateTimeTicks = r.DateTime.Ticks;
                            scalar.value = double.Parse(r.Euribor[i], CultureInfo.InvariantCulture);
                            scalarList.Add(scalar);
                        }
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                }
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
        }
    }
}
