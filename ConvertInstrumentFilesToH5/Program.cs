using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mbdt.ConvertInstrumentFiles
{
    class Program
    {
        private static void TraverseTree(string root, Action<string, bool> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                    action(entry, true);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
            {
                if (root.EndsWith(".xml"))
                    action(root, true);
                Console.WriteLine("Skipping file [{0}]", root);
            }
            else
                Console.WriteLine("Directory or file [{0}] is not found", root);
        }

        private static void Convert(string sourceFileName, bool shorten)
        {
            string destFileName = sourceFileName.Replace(".xml", ".h5");
            Console.WriteLine("[{0}] -> [{1}]", sourceFileName, destFileName);
        }

        static string rootPrefix = "/";
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Arguments: [dir_or_file_name] [prefix_root] [instrument_type]");
                Console.WriteLine("     [dir_or_file_name] - the path todirectory to start recursion");
                Console.WriteLine("     [prefix_root] must - contains forward slashes, ends with a forward slach");
                Console.WriteLine("     [instrument_type]  - one of: {euronext, ecb, ...}");
            }
            else
            {
                rootPrefix = args[1];
                if (!rootPrefix.EndsWith("/"))
                    rootPrefix += "/";
                if (!rootPrefix.StartsWith("/"))
                    rootPrefix = "/" + rootPrefix;
                Console.WriteLine("root           [{0}]", args[0]);
                Console.WriteLine("rootPrefix     [{0}]", rootPrefix);
                Console.WriteLine("instrumentType [{0}]", args[2]);

                TraverseTree(args[0], (s, f) => Convert(s, f));
            }
        }
    }
}
