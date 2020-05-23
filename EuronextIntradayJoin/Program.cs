using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EuronextIntradayJoin
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

        private static void Join(string sourceFileName)
        {
            string s = File.ReadAllText(sourceFileName, Encoding.UTF8);
            s = s.Replace("\r\n", "").Replace("\n\r", "").Replace("\r", "").Replace("\n", "").Replace("----", "");
            string tempFileName = sourceFileName + ".joined";
            try
            {
                File.WriteAllText(tempFileName, s);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to write splitted file: {0}", ex.Message);
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
            if (File.Exists(tempFileName))
            {
                File.Delete(sourceFileName);
                File.Move(tempFileName, sourceFileName);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
                Console.WriteLine("Arguments: dir_or_file_name");
            else
                TraverseTree(args[0], Join);
        }
    }
}
