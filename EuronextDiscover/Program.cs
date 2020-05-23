using System;
using System.Diagnostics;

namespace mbdt.EuronextDiscover
{
    static class Program
    {
        static void Main()
        {
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            try
            {
                EuronextDiscover.UpdateTask(Properties.Settings.Default.DownloadRepositoryPath);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: {0}, stack trace: {1}", ex.Message, ex.StackTrace);
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
