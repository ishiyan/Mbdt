using System;
using System.Diagnostics;

namespace mbdt.EuronextJsonExport
{
    static class Program
    {
        static void Main()
        {
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            // EuronextJsonExport.JsonExportTask(Properties.Settings.Default.xmlToExport);
            EuronextJsonExport.JsonExportTask("import.xml");
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
