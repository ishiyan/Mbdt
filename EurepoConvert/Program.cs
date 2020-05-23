using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace EurepoConvert
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
            var thisRateList = new List<Rates>();
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (line.StartsWith(";"))
                        continue;
                    string[] splitted = line.Split(';');
                    var rates = new Rates {DateTime = DateTime.Parse(splitted[0], CultureInfo.InvariantCulture)};
                    if (11 == splitted.Length)
                    {
                        rates.Eurepo[0] = splitted[1];
                        rates.Eurepo[1] = splitted[2];
                        rates.Eurepo[2] = splitted[3];
                        rates.Eurepo[3] = splitted[4];
                        rates.Eurepo[4] = splitted[5];
                        rates.Eurepo[5] = splitted[6];
                        rates.Eurepo[6] = splitted[7];
                        rates.Eurepo[7] = splitted[8];
                        rates.Eurepo[8] = splitted[9];
                        rates.Eurepo[9] = splitted[10];
                    }
                    else
                        Trace.TraceError("file {0}: illegal line [{1}]", sourceFileName, line);
                    thisRateList.Add(rates);
                }
            }
            if (thisRateList[0].DateTime.Month == 12)
                thisRateList.Reverse();
            rateList.AddRange(thisRateList);
        }

        private class Rates
        {
            internal DateTime DateTime;
            internal readonly List<string> Eurepo = new List<string>(10);
            internal Rates()
            {
                for (int i = 0; i < 10; i++)
                    Eurepo.Add(null);
            }
        }

        private static readonly List<Rates> rateList = new List<Rates>();
        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();

        static void Main(string[] args)
        {
            //DateTime d = new DateTime(632986272000000000);
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

                var nameList = new List<string> {"EUREPOTN", "EUREPO1W", "EUREPO2W", "EUREPO3W", "EUREPO1M", "EUREPO2M", "EUREPO3M", "EUREPO6M", "EUREPO9M", "EUREPO12M"};
                TraverseTree(args[0], Collect);
                for (int i = 0; i < 10; i++)
                {
                    Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, nameList[i]), true);
                    ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    instrumentDictionary.Add(nameList[i], instrument);
                    dataDictionary.Add(nameList[i], scalarData);
                    scalarList.Clear();
                    foreach (var r in rateList)
                    {
                        if (null != r.Eurepo[i])
                        {
                            scalar.dateTimeTicks = r.DateTime.Ticks;
                            scalar.value = double.Parse(r.Eurepo[i], CultureInfo.InvariantCulture);
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
