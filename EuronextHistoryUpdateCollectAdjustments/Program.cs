using System;
using System.IO;

namespace mbdt.EuronextHistoryUpdateCollectAdjustments
{
    static class Program
    {
        private static void Collect(string sourceFileName)
        {
            string destFileName = string.Concat(sourceFileName, ".adjustments");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("Determined adjustment factor"))
                        {
                            destFile.WriteLine(line);
                        }
                    }
                }
            }
        }

        private static void Collect2(string sourceFileName)
        {
            string destFileName = string.Concat(sourceFileName, ".replaced");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("are zeroes: replacing with close"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("is zero: replacing with close"))
                        {
                            destFile.WriteLine(line);
                        }
                    }
                }
            }
        }

        private static void Collect3(string sourceFileName)
        {
            string destFileName = string.Concat(sourceFileName, ".errors");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("Failed to add"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Failed to fetch"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Logical inconsistency"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("found zero price"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Exception"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("invalid endofday history csv2 header, line 5 [] file"))
                        {
                        }
                        else if (line.Contains("invalid endofday history"))
                        {
                            destFile.WriteLine(line);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: Euronext_history_update_log_file");
            else
            {
                Collect(args[0]);
                Collect2(args[0]);
                Collect3(args[0]);
            }
        }
    }
}
