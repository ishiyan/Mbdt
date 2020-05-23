using System;
using System.Diagnostics;

using Mbh5;

namespace mbdt.DukascopyUpdate
{
    class Program
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
            Command command = Command.Update;
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
                    Trace.TraceError("Import directory or file are not specified", importPath);
                    Console.WriteLine("Import directory or file are not specified", importPath);
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
            DateTime startDate = Properties.Settings.Default.StartDate;
            if (startDate.Year < 2000)
                startDate = DateTime.Now;
            Trace.TraceInformation("=======================================================================================");
            if (Command.Download == command)
                Trace.TraceInformation("Command: download to [{0}], days {1}", downloadPath, Properties.Settings.Default.DownloadLookbackDays);
            else if (Command.Import == command)
                Trace.TraceInformation("Command: import from [{0}]", importPath);
            else
                Trace.TraceInformation("Command: update: download to [{0}], days {1}", downloadPath, Properties.Settings.Default.DownloadLookbackDays);
            Trace.TraceInformation("Started: {0}", DateTime.Now);

            int debugTraceLevel = Properties.Settings.Default.DebugTraceLevel;
            if (Command.Update == command)
            {
                if (!DukascopyDownload.DownloadSymbols(startDate, true, debugTraceLevel))
                    Environment.ExitCode = 1;
            }
            else if (Command.Download == command)
            {
                if (!DukascopyDownload.DownloadSymbols(startDate, false, 0))
                    Environment.ExitCode = 1;
            }
            else if (Command.Import == command)
            {
                if (!DukascopyImport.DoImport(importPath, debugTraceLevel))
                    Environment.ExitCode = 1;
            }
            if (!DukascopyImport.DoCleanup())
                Environment.ExitCode = 1;
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }
    }
}
