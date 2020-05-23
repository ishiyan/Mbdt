using System;
using System.IO;

namespace Fix15sec
{
    static class Program
    {
        private static void Fix(string sourceFileName)
        {
            bool fixing = false;
            string[] hours = { "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19" };
            string[] mins = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59" };
            string[] secs = { "00", "15", "30", "45"};
            int hour = 0, min = 0, sec = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (!fixing)
                    {
                        Console.WriteLine(line);
                        if (line.StartsWith("Date"))
                        {
                            fixing = true;
                            line = sourceFile.ReadLine();
                            if (null == line)
                                throw new ApplicationException("Unexpected end of file.");
                            Console.WriteLine(line);
                            if (line.Substring(0, 2).StartsWith("19"))
                                hour = 10;
                            else if (line.Substring(0, 2).StartsWith("18"))
                                hour = 9;
                            else if (line.Substring(0, 2).StartsWith("17"))
                                hour = 8;
                            else
                            {
                                Console.WriteLine("Error: unsupported hour");
                                return;
                            }
                            min = int.Parse(line.Substring(3, 2));
                            if (line.Substring(6, 2).StartsWith("45"))
                                sec = 3;
                            else if (line.Substring(6, 2).StartsWith("30"))
                                sec = 2;
                            else if (line.Substring(6, 2).StartsWith("15"))
                                sec = 1;
                            else if (line.Substring(6, 2).StartsWith("00"))
                                sec = 0;
                            else
                            {
                                Console.WriteLine("Error: unsupported seconds");
                                return;
                            }
                        }
                    }
                    else
                    {
                        sec--;
                        if (0 > sec)
                        {
                            sec = 3;
                            min--;
                        }
                        if (0 > min)
                        {
                            min = 59;
                            hour--;
                        }
                        Console.WriteLine("{0}:{1}:{2}{3}", hours[hour], mins[min], secs[sec], line.Substring(8));
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Arguments: file_name");
            else
                Fix(args[0]);
        }
    }
}
