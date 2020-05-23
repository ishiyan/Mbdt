using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using Mbh5;

namespace ImportCsv
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

        private static readonly SortedList<DateTime, double> ListDouble = new SortedList<DateTime, double>(4096);
        private static readonly SortedList<DateTime, Ohlcv> ListOhlcv = new SortedList<DateTime, Ohlcv>(4096);
        private static readonly SortedList<DateTime, OhlcvPriceOnly> ListOhlcvPriceOnly = new SortedList<DateTime, OhlcvPriceOnly>(4096);
        private static bool isOhlcv, isOhlc, isScalar;
        private static string dateTimeFormat;

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

                    if (2 > splitted.Length)
                    {
                        line =
                            $"file {sourceFileName}: illegal line [{line}], at least 2 values expected, {splitted.Length} got";
                        Console.WriteLine(line);
                        throw new InvalidDataException(line);
                    }
                    DateTime dt = DateTime.ParseExact(splitted[0], dateTimeFormat, CultureInfo.InvariantCulture);
                    if (ListDouble.ContainsKey(dt))
                    {
                        line = $"file {sourceFileName}: illegal line [{line}], duplicate date [{dt}]";
                        Console.WriteLine(line);
                        if (Properties.Settings.Default.AbortOnDuplicateDateTime)
                            throw new InvalidDataException(line);
                    }
                    if (isOhlcv)
                    {
                        if (6 > splitted.Length)
                        {
                            line =
                                $"file {sourceFileName}: illegal line [{line}], at least 6 values expected, {splitted.Length} got";
                            Console.WriteLine(line);
                            throw new InvalidDataException(line);
                        }
                        var ohlcv = new Ohlcv {dateTimeTicks = dt.Ticks, open = double.Parse(splitted[1], CultureInfo.InvariantCulture), high = double.Parse(splitted[2], CultureInfo.InvariantCulture), low = double.Parse(splitted[3], CultureInfo.InvariantCulture), close = double.Parse(splitted[4], CultureInfo.InvariantCulture), volume = double.Parse(splitted[5], CultureInfo.InvariantCulture)};
                        ListOhlcv.Add(dt, ohlcv);
                    }
                    else if (isOhlc)
                    {
                        if (5 > splitted.Length)
                        {
                            line =
                                $"file {sourceFileName}: illegal line [{line}], at least 5 values expected, {splitted.Length} got";
                            Console.WriteLine(line);
                            throw new InvalidDataException(line);
                        }
                        var ohlcvPriceOnly = new OhlcvPriceOnly { dateTimeTicks = dt.Ticks, open = double.Parse(splitted[1], CultureInfo.InvariantCulture), high = double.Parse(splitted[2], CultureInfo.InvariantCulture), low = double.Parse(splitted[3], CultureInfo.InvariantCulture), close = double.Parse(splitted[4], CultureInfo.InvariantCulture) };
                        ListOhlcvPriceOnly.Add(dt, ohlcvPriceOnly);
                    }
                    else if (isScalar)
                    {
                        double d = double.Parse(splitted[1], CultureInfo.InvariantCulture);
                        ListDouble.Add(dt, d);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 6)
                Console.WriteLine("Argument: h5_file instrument_path [1d|1h|4m|2m|1m] [dd/mm/yy] [ohlcv|ohlc|scalar] input_file_or_dir");
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

                s = args[4];
                if (s.StartsWith("ohlcv"))
                    isOhlcv = true;
                else if (s.StartsWith("ohlc"))
                    isOhlc = true;
                else if (s.StartsWith("scalar"))
                    isScalar = true;
                else
                {
                    Console.WriteLine("Invalid entity type [{0}]: [ohlcv|ohlc|scalar] expected", s);
                    return;
                }

                dateTimeFormat = args[3];

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
                        if (isOhlcv)
                        {
                            using (OhlcvData ohlcvData = instrument.OpenOhlcv(OhlcvKind.Default, dtf, true))
                            {
                                TraverseTree(args[5], Collect);
                                var ohlcvList = ListOhlcv.Select(r => r.Value).ToList();
                                ohlcvData.Add(ohlcvList, Properties.Settings.Default.UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip, true);
                            }
                        }
                        else if (isOhlc)
                        {
                            using (OhlcvPriceOnlyData ohlcvPriceOnlyData = instrument.OpenOhlcvPriceOnly(OhlcvKind.Default, dtf, true))
                            {
                                TraverseTree(args[5], Collect);
                                var listOhlcvPriceOnlyList = ListOhlcvPriceOnly.Select(r => r.Value).ToList();
                                ohlcvPriceOnlyData.Add(listOhlcvPriceOnlyList, Properties.Settings.Default.UpdateDuplicateTicks ? DuplicateTimeTicks.Update : DuplicateTimeTicks.Skip, true);
                            }
                        }
                        else if (isScalar)
                        {
                            var scalar = new Scalar();
                            var scalarList = new List<Scalar>();
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, dtf, true))
                            {
                                TraverseTree(args[5], Collect);
                                foreach (var r in ListDouble)
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
}
