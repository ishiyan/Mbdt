using System;
using System.IO;

namespace mbdt.EuronextCollectIllegalTimestamps
{
    internal static class Program
    {
        private static void Collect(string sourceFileName)
        {
            string destFileName = string.Concat(sourceFileName, ".illegal_ticks");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("Mbh5: Non-increasing input ticks:"))
                        {
                            destFile.WriteLine(line);
                            int idx1 = line.IndexOf("[t:", StringComparison.Ordinal);
                            int idx2 = line.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            string s = line.Substring(idx1 + 3, idx2 - idx1 - 3);
                            long t;
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt1 = new DateTime(t);
                            s = line.Substring(idx2 + 4);
                            idx1 = s.IndexOf("[t:", StringComparison.Ordinal);
                            idx2 = s.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            s = s.Substring(idx1 + 3, idx2 - idx1 - 3);
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt2 = new DateTime(t);
                            destFile.WriteLine("[{0}] -> [{1}]", dt1, dt2);
                        }
                        else if (line.Contains("Fixed decreasing timestamp"))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Mbh5: Failed to add "))
                        {
                        }
                        else if (line.Contains("Mbh5: Duplicate ticks: "))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Failed to add "))
                        {
                            destFile.WriteLine(line);
                            destFile.WriteLine("------------------------");
                        }
                    }
                }
            }
        }

        private static void Collect2(string sourceFileName)
        {
            // ReSharper disable once StringLiteralTypo
            string destFileName = string.Concat(sourceFileName, ".illegal_ticks_nofund");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("Mbh5: Non-increasing input ticks:"))
                        {
                            destFile.WriteLine(line);
                            int idx1 = line.IndexOf("[t:", StringComparison.Ordinal);
                            int idx2 = line.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            string s = line.Substring(idx1 + 3, idx2 - idx1 - 3);
                            long t;
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt1 = new DateTime(t);
                            s = line.Substring(idx2 + 4);
                            idx1 = s.IndexOf("[t:", StringComparison.Ordinal);
                            idx2 = s.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            s = s.Substring(idx1 + 3, idx2 - idx1 - 3);
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt2 = new DateTime(t);
                            destFile.WriteLine("[{0}] -> [{1}]", dt1, dt2);
                        }
                        else if (line.Contains("Fixed decreasing timestamp"))
                        {
                            if (!line.Contains("/funds/"))
                                destFile.WriteLine(line);
                        }
                        else if (line.Contains("Mbh5: Failed to add "))
                        {
                        }
                        else if (line.Contains("Mbh5: Duplicate ticks: "))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Failed to add "))
                        {
                            destFile.WriteLine(line);
                            destFile.WriteLine("------------------------");
                        }
                    }
                }
            }
        }

        private static void Collect3(string sourceFileName)
        {
            string destFileName = string.Concat(sourceFileName, ".illegal_ticks_fund");
            using (var destFile = new StreamWriter(destFileName))
            {
                using (var sourceFile = new StreamReader(sourceFileName))
                {
                    string line;
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        if (line.Contains("Mbh5: Non-increasing input ticks:"))
                        {
                            destFile.WriteLine(line);
                            int idx1 = line.IndexOf("[t:", StringComparison.Ordinal);
                            int idx2 = line.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            string s = line.Substring(idx1 + 3, idx2 - idx1 - 3);
                            long t;
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt1 = new DateTime(t);
                            s = line.Substring(idx2 + 4);
                            idx1 = s.IndexOf("[t:", StringComparison.Ordinal);
                            idx2 = s.IndexOf(", ", StringComparison.Ordinal);
                            if (idx1 < 0 || idx2 < 0 || idx1 >= idx2)
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, cannot find [{0}]/[{1}] patterns: [{2}]", "[t:", ", ", line);
                                continue;
                            }
                            s = s.Substring(idx1 + 3, idx2 - idx1 - 3);
                            if (!long.TryParse(s, out t))
                            {
                                destFile.WriteLine("[{0}]", dt1);
                                destFile.WriteLine("***ERROR*** invalid line, failed to parse ticks [{0}]: [{1}]", s, line);
                                continue;
                            }
                            var dt2 = new DateTime(t);
                            destFile.WriteLine("[{0}] -> [{1}]", dt1, dt2);
                        }
                        else if (line.Contains("Fixed decreasing timestamp"))
                        {
                            if (line.Contains("/funds/"))
                                destFile.WriteLine(line);
                        }
                        else if (line.Contains("Mbh5: Failed to add "))
                        {
                        }
                        else if (line.Contains("Mbh5: Duplicate ticks: "))
                        {
                            destFile.WriteLine(line);
                        }
                        else if (line.Contains("Failed to add "))
                        {
                            destFile.WriteLine(line);
                            destFile.WriteLine("------------------------");
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: Euronext_intraday_update_log_file");
            else
            {
                Collect(args[0]);
                Collect2(args[0]);
                Collect3(args[0]);
            }
        }
    }
}
