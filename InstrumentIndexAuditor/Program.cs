using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mbdt.InstrumentIndexAuditor
{
    class Program
    {
        public static void TraverseTree(string root, Action<string> action)
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

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Arguments: dir_or_file_name");
            else
                TraverseTree(args[0], s =>
                {
                    new InstrumentIndexAuditor().Audit(s, true).ForEach(x => Console.WriteLine(x));
                });
        }
    }
}
