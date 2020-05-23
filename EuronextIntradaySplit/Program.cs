using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EuronextIntradaySplit
{
    static class Program
    {
        private static string ParseJs(string s, ref DateTime dt)
        {
            string[] splitted = Regex.Split(s, @",""");
            if (7 > splitted.Length)
                return s;
            string entry = splitted[4];
            // dateAndTime":"29\/08\/2012 09:00:02"
            //           11111111112222222222333333
            // 012345678901234567890123456789012345
            if (!entry.StartsWith(@"dateAndTime"":""") || 36 != entry.Length || '\\' != entry[16] || '/' != entry[17] || '\\' != entry[20] || '/' != entry[21] || ' ' != entry[26] || ':' != entry[29] || ':' != entry[32])
                return s;
            int day = 10 * (entry[14] - '0') + (entry[15] - '0');
            int month = 10 * (entry[18] - '0') + (entry[19] - '0');
            int year = 1000 * (entry[22] - '0') + 100 * (entry[23] - '0') + 10 * (entry[24] - '0') + (entry[25] - '0');
            int hour = 10 * (entry[27] - '0') + (entry[28] - '0');
            int minute = 10 * (entry[30] - '0') + (entry[31] - '0');
            int second = 10 * (entry[33] - '0') + (entry[34] - '0');
            var dtNew = new DateTime(year, month, day, hour, minute, second);
            if (dt > dtNew)
                s = "----" + s;
            dt = dtNew;
            return s;
        }

        private static void Split(string sourceFileName)
        {
            string s = File.ReadAllText(sourceFileName, Encoding.UTF8);
            int i = s.IndexOf("[{", StringComparison.Ordinal);
            if (i < 0)
            {
                Trace.TraceError("no intraday data found in js file {0}, skipping", Path.GetFileName(sourceFileName));
                return;
            }
            var list = new List<string>(4096);
            var dt = new DateTime(0L);
            string[] splitted = s.Split(new[]{"},{"}, StringSplitOptions.None);
            if (splitted.Length == 0)
            {
                Trace.TraceError("no intraday data found in js file {0}, skipping", Path.GetFileName(sourceFileName));
                return;
            }
            list.Add(splitted[0].Substring(0, i + 1));
            if (splitted.Length > 1)
                list.Add(ParseJs(splitted[0].Substring(i + 1)+"},", ref dt));
            else if (splitted.Length == 1)
                list.Add(ParseJs(splitted[0].Substring(i + 1), ref dt));
            for (int j = 1; j < splitted.Length - 1; ++j)
                list.Add(ParseJs("{" + splitted[j] + "},", ref dt));
            if (splitted.Length > 1)
                list.Add(ParseJs("{" + splitted[splitted.Length - 1], ref dt));
            string tempFileName = sourceFileName + ".splitted";
            try
            {
                using (var stream = new StreamWriter(tempFileName, false, Encoding.UTF8))
                    list.ForEach(stream.WriteLine);
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

        private static void Copy(string sourceFileName, string sourceJsDir, string destJsDir)
        {
            string pathOld = sourceJsDir + @"\" + sourceFileName + "_" + destJsDir + "_eoi.js";
            string pathNew = destJsDir + @"\" + sourceFileName + "_" + destJsDir + "_eoi.js";
            File.Copy(pathOld, pathNew);
            Split(pathNew);
        }

        private static void CopyAndSplit(string illegalTicksFileName, string sourceJsDir, string destJsDir)
        {
            if (illegalTicksFileName == "*")
            {
                string[] entries = Directory.GetFiles(sourceJsDir);
                foreach (string entry in entries)
                {
                    string f = Path.GetFileName(entry);
                    string pathOld = sourceJsDir + @"\" + f;
                    string pathNew = destJsDir + @"\" + f;
                    File.Copy(pathOld, pathNew);
                    Split(pathNew);
                }
                return;
            }
            var dict = new Dictionary<string, string>(128);
            string [] s = File.ReadAllLines(illegalTicksFileName, Encoding.UTF8);
            foreach (var t in s)
            {
                int i = t.IndexOf("]:[/", StringComparison.Ordinal);
                string w = t.Substring(i + 4).TrimEnd(']');
                if (!dict.ContainsKey(w))
                    dict.Add(w, w);
            }
            foreach (var k in dict.Keys)
            {
                Copy(k, sourceJsDir, destJsDir);
            }
        }

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

        static void Main(string[] args)
        {
            if (args.Length == 3)
                CopyAndSplit(args[0], args[1], args[2]);
            else if (args.Length == 1)
                TraverseTree(args[0], Split);
            else
            {
                Console.WriteLine("Arguments: illegal_ticks_file_name source_js_dir dest_js_dir");
                Console.WriteLine("or");
                Console.WriteLine("Arguments: js_dir");
            }
        }
    }
}
