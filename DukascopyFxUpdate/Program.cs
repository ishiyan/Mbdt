using System;
using System.Diagnostics;

using Mbh5;

namespace mbdt.DukascopyFxUpdate
{
    internal static class Program
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
            Environment.ExitCode = 0;
            Command command;
            string downloadPath = Properties.Settings.Default.DownloadDir;
            string importPath = null;
            string argument = (0 < args.Length ? args[0] : "update");
            if ("download".Equals(argument))
            {
                command = Command.Download;
                if (1 < args.Length)
                {
                    downloadPath = args[1];
                    Trace.TraceInformation("Download directory: [{0}]", downloadPath);
                }
                else
                    Trace.TraceInformation("Download directory is not specified, the default value [{0}] will be used", downloadPath);
            }
            else if ("import".Equals(argument))
            {
                command = Command.Import;
                if (1 < args.Length)
                {
                    importPath = args[1];
                    Trace.TraceInformation("Import directory or file: [{0}]", importPath);
                }
                else
                {
                    Trace.TraceError("Import directory or file are not specified");
                    Console.WriteLine("Import directory or file are not specified");
                    Console.WriteLine("Arguments: {update downloadDirPath} | {download downloadDirPath} | {import importDirOrFilePath}");
                    Environment.ExitCode = 1;
                    return;
                }
            }
            else if ("update".Equals(argument))
            {
                command = Command.Update;
                if (1 < args.Length)
                {
                    downloadPath = args[1];
                    Trace.TraceInformation("Download directory: [{0}]", downloadPath);
                }
                else
                    Trace.TraceInformation("Download directory is not specified, the default value [{0}] will be used", downloadPath);
            }
            else
            {
                Console.WriteLine("Arguments: {update downloadDirPath {symbol}} | {download downloadDirPath {symbol}} | {import importDirOrFilePath {symbol}}");
                return;
            }

            DukascopyFxContext.DownloadRetries = Properties.Settings.Default.DownloadRetries;
            DukascopyFxContext.DownloadTimeout = Properties.Settings.Default.DownloadTimeout;
            DukascopyFxContext.RepositoryPath = Properties.Settings.Default.RepositoryPath;
            DukascopyFxContext.DownloadDir = downloadPath;
            DukascopyFxContext.DownloadOverwrite = Properties.Settings.Default.DownloadOverwriteExisting;
            DukascopyFxContext.DownloadLookbackDays = Properties.Settings.Default.DownloadLookbackDays;
            DukascopyFxContext.DownloadLookbackMonths = Properties.Settings.Default.DownloadLookbackMonths;
            // DukascopyFxContext.WorkerThreadDelayMilliseconds = Properties.Settings.Default.WorkerThreadDelayMilliseconds;
            DukascopyFxContext.Symbols = Properties.Settings.Default.Symbols.Split(',');
            if (3 <= args.Length)
                DukascopyFxContext.Symbols = args[2].Split(',');
            DukascopyFxImport.IsOldFormat = Properties.Settings.Default.OldFormat;

            DateTime startDate = Properties.Settings.Default.StartDate;
            if (startDate.Year < 2000)
                startDate = DateTime.Now;
            Trace.TraceInformation("=======================================================================================");
            if (Command.Download == command)
                Trace.TraceInformation("Command: download to [{0}], days {1}, months {2}", downloadPath, DukascopyFxContext.DownloadLookbackDays, DukascopyFxContext.DownloadLookbackMonths);
            else if (Command.Import == command)
                Trace.TraceInformation("Command: import from [{0}]", importPath);
            else
                Trace.TraceInformation("Command: update: download to [{0}], days {1}, months {2}", downloadPath, DukascopyFxContext.DownloadLookbackDays, DukascopyFxContext.DownloadLookbackMonths);
            Trace.TraceInformation("Started: {0}", DateTime.Now);

            bool importCandles = Properties.Settings.Default.ImportCandles;
            int debugTraceLevel = Properties.Settings.Default.DebugTraceLevel;
            if (Command.Update == command)
            {
                if (!DukascopyFxDownload.DownloadMonthsFromDate(startDate, importCandles, debugTraceLevel))
                    Environment.ExitCode = 1;
                if (!DukascopyFxDownload.DownloadDaysFromDate(startDate, true, importCandles, debugTraceLevel))
                    Environment.ExitCode = 1;
            }
            else if (Command.Download == command)
            {
                if (!DukascopyFxDownload.DownloadMonthsFromDate(startDate, false, 0))
                    Environment.ExitCode = 1;
                if (!DukascopyFxDownload.DownloadDaysFromDate(startDate, false, false, 0))
                    Environment.ExitCode = 1;
            }
            else if (Command.Import == command)
            {
                if (!DukascopyFxImport.DoImport(importPath, importCandles, debugTraceLevel))
                    Environment.ExitCode = 1;
            }
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }
    }
}
