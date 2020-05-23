using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mbdt.InstrumentFileXmlStatistics
{
    class Program
    {
        private static void TraverseTree(string root, bool shorten, Action<string, bool> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                    action(entry, shorten);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, shorten, action);
            }
            else if (File.Exists(root))
                action(root, shorten);
        }

        private static void Convert(string sourceFileName, bool shorten)
        {
            string destFileName = string.Concat(sourceFileName, ".converted");
            string line;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                using (var destFile = new StreamWriter(destFileName))
                {
                    while (null != (line = sourceFile.ReadLine()))
                    {
                        line = line.Replace("<securities>", "<instruments>");
                        line = line.Replace("</securities>", "</instruments>");
                        line = line.Replace("</security>", "</instrument>");
                        line = line.Replace("<security ", "<instrument vendor=\"Fibbs\" ");
                        //if (shorten)
                        //{
                        //    line = line.Replace("<quote ", "<q ");
                        //    line = line.Replace("<tick ", "<t ");
                        //    line = line.Replace("</quote>", "</q>");
                        //    line = line.Replace(" date=\"", " d=\"");
                        //    line = line.Replace(" jdn=\"", " j=\"");
                        //    line = line.Replace(" price=\"", " p=\"");
                        //    line = line.Replace(" sec=\"", " s=\"");
                        //    line = line.Replace(" time=\"", " t=\"");
                        //    line = line.Replace(" volume=\"", " v=\"");
                        //    line = line.Replace(" open=\"", " o=\"");
                        //    line = line.Replace(" high=\"", " h=\"");
                        //    line = line.Replace(" low=\"", " l=\"");
                        //    line = line.Replace(" close=\"", " c=\"");
                        //}
                        //else
                        //{
                        //    line = line.Replace("<q ", "<quote ");
                        //    line = line.Replace("<t ", "<tick ");
                        //    line = line.Replace("</q>", "</quote>");
                        //    line = line.Replace(" d=\"", " date=\"");
                        //    line = line.Replace(" j=\"", " jdn=\"");
                        //    line = line.Replace(" p=\"", " price=\"");
                        //    line = line.Replace(" s=\"", " sec=\"");
                        //    line = line.Replace(" t=\"", " time=\"");
                        //    line = line.Replace(" v=\"", " volume=\"");
                        //    line = line.Replace(" o=\"", " open=\"");
                        //    line = line.Replace(" h=\"", " high=\"");
                        //    line = line.Replace(" l=\"", " low=\"");
                        //    line = line.Replace(" c=\"", " close=\"");
                        //}
                        destFile.WriteLine(line);
                    }
                }
            }
            File.Replace(destFileName, sourceFileName, null);
        }

        static void Main(string[] args)
        {
            //if (args.Length < 2 || (!"shorten".Equals(args[0]) && !"widen".Equals(args[0])))
            //    Console.WriteLine("Arguments: shorten|widen dir_or_file_name");
            //else
            //    TraverseTree(args[1], "shorten".Equals(args[0]), (s, f) => Convert(s, f));
            if (args.Length < 1)
                Console.WriteLine("Argument: dir_or_file_name");
            else
                TraverseTree(args[0], true, (s, f) => Convert(s, f));
        }
    }
}
