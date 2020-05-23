using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Mbh5;

namespace Knap
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

        private static readonly SortedList<DateTime, double> list = new SortedList<DateTime, double>(4096);

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
                    if (7 != splitted.Length)
                    {
                        line =
                            $"file {sourceFileName}: illegal line [{line}], 7 values expected, {splitted.Length} got";
                        Console.WriteLine(line);
                        throw new InvalidDataException(line);
                    }
                    var dt = new DateTime(
                        int.Parse(splitted[0], CultureInfo.InvariantCulture),
                        int.Parse(splitted[1], CultureInfo.InvariantCulture),
                        int.Parse(splitted[2], CultureInfo.InvariantCulture),
                        int.Parse(splitted[3], CultureInfo.InvariantCulture),
                        int.Parse(splitted[4], CultureInfo.InvariantCulture),
                        0);
                    dt = dt.AddSeconds(double.Parse(splitted[5], CultureInfo.InvariantCulture));
                    double d = double.Parse(splitted[6], CultureInfo.InvariantCulture);
                    if (list.ContainsKey(dt))
                    {
                        line = $"file {sourceFileName}: illegal line [{line}], duplicate date [{dt}]";
                        Console.WriteLine(line);
                        if (Properties.Settings.Default.AbortOnDuplicateDateTime)
                            throw new InvalidDataException(line);
                    }
                    else
                        list.Add(dt, d);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 4)
                Console.WriteLine("Argument: h5_file instrument_path [1d|1h|4m|2m|1m] input_file_or_dir");
            else
            {
                DataTimeFrame dtf;
                string s = args[2];
                if (s.StartsWith("1m"))
                    dtf = DataTimeFrame.Minute1;
                else if (s.StartsWith("2m"))
                    dtf = DataTimeFrame.Minute2;
                else if (s.StartsWith("4m"))
                    dtf = DataTimeFrame.Minute4;
                else if (s.StartsWith("5m"))
                    dtf = DataTimeFrame.Minute5;
                else if (s.StartsWith("1h"))
                    dtf = DataTimeFrame.Hour1;
                else if (s.StartsWith("1d"))
                    dtf = DataTimeFrame.Day1;
                else
                {
                    Console.WriteLine("Invalid time frame [{0}]: [1d|1h|4m|2m|1m] expected", s);
                    return;
                }
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Repository.InterceptErrorStack();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                string h5File = args[0];
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                {
                    using (Instrument instrument = repository.Open(args[1], true))
                    {
                        using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, dtf, true))
                        {
                            TraverseTree(args[3], Collect);
                            foreach (var r in list)
                            {
                                scalar.dateTimeTicks = r.Key.Ticks;
                                scalar.value = r.Value;
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, Properties.Settings.Default.UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip, true);
                        }
                    }
                }
            }
        }
    }
}
