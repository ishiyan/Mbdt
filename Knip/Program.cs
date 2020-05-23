using System;
using System.IO;

namespace Knip
{
    static class Program
    {
        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                {
                        action(entry);
                }
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
            {
                    action(root);
            }
        }

        private static int pos, len;

        private static void Collect(string sourceFileName)
        {
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    string s;
                    if (Properties.Settings.Default.YearLen == 4 && (line.StartsWith("19") || line.StartsWith("20")))
                    {
                        s = line.Substring(Properties.Settings.Default.YearPos, Properties.Settings.Default.YearLen).Trim();
                    }
                    else if (Properties.Settings.Default.YearLen == 2 && (line.StartsWith("8") || line.StartsWith("9") || line.StartsWith("0") || line.StartsWith("1") || line.StartsWith("7") || line.StartsWith("6") || line.StartsWith("5") || line.StartsWith("4") || line.StartsWith("3") || line.StartsWith("2")))
                    {
                        s = line.Substring(Properties.Settings.Default.YearPos, Properties.Settings.Default.YearLen).Trim();
                        if (s.StartsWith("0") || s.StartsWith("1"))
                            s = string.Concat("20", s);
                        else
                            s = string.Concat("19", s);
                    }
                    else
                        continue;
                    int i = int.Parse(s);
                    DateTime dt;
                    if (0 > Properties.Settings.Default.DoyPos)
                    {
                        s = line.Substring(Properties.Settings.Default.DayPos, Properties.Settings.Default.DayLen).Trim();
                        int j = int.Parse(s);
                        s = line.Substring(Properties.Settings.Default.MonthPos, Properties.Settings.Default.MonthLen).Trim();
                        int k = int.Parse(s);
                        dt = new DateTime(i, k, j, 0, 0, 0);
                    }
                    else
                    {
                        dt = new DateTime(i, 1, 1, 0, 0, 0);
                        s = line.Substring(Properties.Settings.Default.DoyPos, Properties.Settings.Default.DoyLen).Trim();
                        i = int.Parse(s) - 1;
                        dt = dt.AddDays(i);
                    }
                    if (0 <= Properties.Settings.Default.HourPos)
                    {
                        s = line.Substring(Properties.Settings.Default.HourPos, Properties.Settings.Default.HourLen).Trim();
                        i = int.Parse(s);
                        dt = dt.AddHours(i);
                    }
                    if (0 <= Properties.Settings.Default.MinutePos)
                    {
                        s = line.Substring(Properties.Settings.Default.MinutePos, Properties.Settings.Default.MinuteLen).Trim();
                        i = int.Parse(s);
                        dt = dt.AddMinutes(i);
                    }
                    double d;
                    if (0 <= Properties.Settings.Default.SecondPos)
                    {
                        s = line.Substring(Properties.Settings.Default.SecondPos, Properties.Settings.Default.SecondLen).Trim();
                        d = double.Parse(s);
                        dt = dt.AddSeconds(d);
                    }
                    s = line.Substring(pos, len).Trim();
                    if (s.StartsWith(Properties.Settings.Default.NaN1))
                        s = "NaN";
                    if (s.StartsWith(Properties.Settings.Default.NaN2))
                        s = "NaN";
                    if (s.StartsWith(Properties.Settings.Default.NaN3))
                        s = "NaN";
                    d = s.StartsWith("NaN") ? double.NaN : double.Parse(s);
                    if (Properties.Settings.Default.OutputTicks)
                        Console.WriteLine("{0};{1}", dt.Ticks, d);
                    else
                        Console.WriteLine("{0};{1};{2};{3};{4};{5};{6}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, d);
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 3)
                Console.WriteLine("Arguments: pos len dir_or_file_name");
            else
            {
                pos = int.Parse(args[0]);
                len = int.Parse(args[1]);
                TraverseTree(args[2], Collect);
            }
        }
    }
}
