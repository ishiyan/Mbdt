using System;
using System.Diagnostics;

namespace mbdt.EuronextJsonExport
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Arguments: [file_name.xml] [file_nale.json]");
                Console.WriteLine("     [file_name.xml] - the input xml file containing instruments");
                Console.WriteLine("     [file_nale.json] - the input json file");
                return;
            }

            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            EuronextJsonExport.JsonExportTask(args[0], args[1]);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
