using System;
using System.Diagnostics;

namespace mbdt.EuronextAudit
{
    static class Program
    {
        static void Main()
        {
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            EuronextAudit.AuditTask();
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
