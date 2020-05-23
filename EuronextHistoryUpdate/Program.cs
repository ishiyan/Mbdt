using System;
using System.Diagnostics;

using mbdt.Euronext;
using Mbh5;

namespace mbdt.EuronextHistoryUpdate
{
    static class Program
    {
        private enum Command
        {
            Download,
            Import,
            Update
        }

        static void Main(string[] args)
        {
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Environment.ExitCode = 0;
            Command command = Command.Update;
            string commandPath = null;
            if (0 < args.Length)
            {
                if ("download" == args[0])
                {
                    command = Command.Download;
                    if (1 < args.Length)
                        commandPath = args[1];
                    else
                    {
                        Trace.TraceError("Download command requires a download path as a second argument");
                        return;
                    }
                }
                else if ("import" == args[0])
                {
                    command = Command.Import;
                    if (1 < args.Length)
                        commandPath = args[1];
                    else
                    {
                        Trace.TraceError("Import command requires an import path as a second argument");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Arguments: {download toDirPath} | {import fromDirOrFilePath}");
                    return;
                }
            }
            EuronextInstrumentContext.DownloadRetries = Properties.Settings.Default.DownloadRetries;
            EuronextInstrumentContext.DownloadTimeout = Properties.Settings.Default.DownloadTimeout;
            EuronextInstrumentContext.EndofdayRepositoryPath = Properties.Settings.Default.EndofdayRepositoryPath;
            EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath = Properties.Settings.Default.EndofdayDiscoveredRepositoryPath;
            EuronextInstrumentContext.HistoryDownloadOverwriteExisting = Properties.Settings.Default.HistoryDownloadOverwriteExisting;
            EuronextInstrumentContext.HistoryDownloadMinimalLength = Properties.Settings.Default.HistoryDownloadMinimalLength;
            EuronextInstrumentContext.DownloadRepositoryPath = Properties.Settings.Default.DownloadRepositoryPath;
            EuronextInstrumentContext.DownloadPasses = Properties.Settings.Default.DownloadPasses;
            EuronextInstrumentContext.ApprovedIndexPath = Properties.Settings.Default.ApprovedIndexPath;
            EuronextInstrumentContext.DiscoveredIndexPath = Properties.Settings.Default.DiscoveredIndexPath;
            EuronextInstrumentContext.WorkerThreads = Properties.Settings.Default.NumberOfWorkerThreads;
            EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails = Properties.Settings.Default.DownloadMaximumLimitConsecutiveFails;
            EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds = Properties.Settings.Default.WorkerThreadDelayMilliseconds;
            EuronextInstrumentContext.IsHistoryAdjusted = Properties.Settings.Default.Adjusted;
            EuronextExecutor.StartDateDaysBack = Properties.Settings.Default.StartDateDaysBack;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ExcludeMics))
            {
                string[] splitted = Properties.Settings.Default.ExcludeMics.Split(',');
                foreach (var mic in splitted)
                {
                    if (!string.IsNullOrEmpty(mic))
                        EuronextExecutor.ExcludeMics.Add(mic);
                }
            }

            int days = Properties.Settings.Default.HistoryDownloadDays;
            Trace.TraceInformation("=======================================================================================");
            if (Command.Download == command)
                Trace.TraceInformation("Command: download to {0}, days {1}", commandPath, days);
            else if (Command.Import == command)
                Trace.TraceInformation("Command: import from {0}", commandPath);
            Trace.TraceInformation("Started: {0}", DateTime.Now);

            if (Command.Update == command)
                Environment.ExitCode = EuronextEodHistory.UpdateTask(days);
            else if (Command.Download == command)
                Environment.ExitCode = EuronextEodHistory.DownloadTask(commandPath, days);
            else if (Command.Import == command)
                Environment.ExitCode = EuronextEodHistory.ImportTask(commandPath);
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }
    }
}
