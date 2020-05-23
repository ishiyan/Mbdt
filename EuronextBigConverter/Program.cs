using System;
using System.Diagnostics;

namespace mbdt.EuronextBigConverter
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            EuronextBigConverter.Task(args[0]);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
