using System;
using System.Diagnostics;

namespace mbdt.RannForexDownload
{
    static class Program
    {
        static void Main()
        {
            Environment.ExitCode = 0;
            int daysBack = Properties.Settings.Default.DaysBack;
            DateTime startDate = Properties.Settings.Default.StartDate;
            if (startDate.Year < 2000)
                startDate = DateTime.Now.AddDays(-1);
            Trace.TraceInformation("=======================================================================================");
            Trace.TraceInformation("Download to [{0}] {1} days back starting from [{2}]", Properties.Settings.Default.DownloadDir, daysBack, startDate.ToShortDateString());
            if (!RannForexDownload.Download(startDate, daysBack))
                Environment.ExitCode = 1;
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }
    }
}
