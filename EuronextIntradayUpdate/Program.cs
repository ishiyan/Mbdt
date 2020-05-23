using System;
using System.Diagnostics;

using mbdt.Euronext;
using Mbh5;

namespace mbdt.EuronextIntradayUpdate
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
            var command = Command.Update;
            string commandPath = null, commandOption = null;
            if (0 < args.Length)
            {
                if ("download" == args[0])
                {
                    command = Command.Download;
                    if (1 < args.Length)
                    {
                        commandPath = args[1];
                    }
                    else
                    {
                        Trace.TraceError("Download command requires a download path as a second argument");
                        return;
                    }
                }
                else if ("import" == args[0])
                {
                    command = Command.Import;
                    if (2 < args.Length)
                    {
                        commandPath = args[1];
                        commandOption = args[2];
                    }
                    else
                    {
                        Trace.TraceError("Import command requires an import path as a second argument and a target yyyymmdd as a third argument");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Arguments: {download toDirPath} | {import fromDirOrFilePath yyyymmdd}");
                    return;
                }
            }
            EuronextInstrumentContext.DownloadRetries = Properties.Settings.Default.DownloadRetries;
            EuronextInstrumentContext.DownloadTimeout = Properties.Settings.Default.DownloadTimeout;
            EuronextInstrumentContext.IntradayRepositoryPath = Properties.Settings.Default.IntradayRepositoryPath;
            EuronextInstrumentContext.IntradayDiscoveredRepositoryPath = Properties.Settings.Default.IntradayDiscoveredRepositoryPath;
            EuronextInstrumentContext.IntradayDownloadOverwriteExisting = Properties.Settings.Default.IntradayDownloadOverwriteExisting;
            EuronextInstrumentContext.IntradayDownloadMinimalLength = Properties.Settings.Default.IntradayDownloadMinimalLength;
            EuronextInstrumentContext.DownloadRepositoryPath = Properties.Settings.Default.DownloadRepositoryPath;
            EuronextInstrumentContext.DownloadPasses = Properties.Settings.Default.DownloadPasses;
            EuronextInstrumentContext.ApprovedIndexPath = Properties.Settings.Default.ApprovedIndexPath;
            EuronextInstrumentContext.DiscoveredIndexPath = Properties.Settings.Default.DiscoveredIndexPath;
            EuronextInstrumentContext.WorkerThreads = Properties.Settings.Default.NumberOfWorkerThreads;
            EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails = Properties.Settings.Default.DownloadMaximumLimitConsecutiveFails;
            EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds = Properties.Settings.Default.WorkerThreadDelayMilliseconds;
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

            Trace.TraceInformation("=======================================================================================");
            if (Command.Download == command)
                Trace.TraceInformation("Command: download to {0}", commandPath);
            else if (Command.Import == command)
                Trace.TraceInformation("Command: import from {0}, target date {1}", commandPath, commandOption);
            Trace.TraceInformation("Started: {0}", DateTime.Now);

            if (Command.Update == command)
                Environment.ExitCode = EuronextEodIntraday.UpdateTask();
            else if (Command.Download == command)
                Environment.ExitCode = EuronextEodIntraday.DownloadTask(commandPath);
            else if (Command.Import == command)
                Environment.ExitCode = EuronextEodIntraday.ImportTask(commandPath, commandOption);
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }
    }
}
