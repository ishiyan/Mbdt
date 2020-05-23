using System;
using System.Globalization;
using System.IO;

namespace RenameLog
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (1 > args.Length)
            {
                Console.WriteLine("Please specify the name of a log file as the first argument.");
            }
            else
            {
                const string directory = "logs";
                string source = args[0];
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                DateTime dateTime = DateTime.Now;
                string target = string.Format("{0}{1}{2}_{3}-{4}-{5}_{6}-{7}-{8}.log",
                    directory, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), Path.GetFileNameWithoutExtension(source), dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                if (File.Exists(source))
                {
                    try
                    {
                        File.Copy(source, target);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to copy [{0}] to [{1}]: exception [{2}]]", source, target, e.Message);
                    }
                    try
                    {
                        File.Delete(source);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to delete [{0}]: exception [{1}]]", source, e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("The file [{0}] does not exist", source);
                }
            }
        }
    }
}
