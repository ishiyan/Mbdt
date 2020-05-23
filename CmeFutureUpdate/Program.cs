using System;
using System.Diagnostics;

using Mbh5;

namespace CmeFutureUpdate
{
    internal static class Program
    {
        private enum Command
        {
            Download,
            Import,
            ImportText,
            Update
        }

        static void Main(string[] args)
        {
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            var command = Command.Update;
            string symbol = null, yyyymmdd = null, code = null, file = null;
            if (0 < args.Length)
            {
                if ("download" == args[0])
                    command = Command.Download;
                else if ("import" == args[0])
                {
                    command = Command.Import;
                    if (2 < args.Length)
                    {
                        symbol = args[1];
                        yyyymmdd = args[2];
                    }
                    else
                    {
                        Trace.TraceError("Import command requires a future symbol as a second argument and a target yyyymmdd as a third argument");
                        return;
                    }
                }
                else if ("importtext" == args[0])
                {
                    command = Command.ImportText;
                    if (4 < args.Length)
                    {
                        symbol = args[1];
                        code = args[2];
                        file = args[3];
                        yyyymmdd = args[4];
                    }
                    else
                    {
                        Trace.TraceError("ImportText command requires a future symbol as a 2nd argument, a code as a 3rd argument, a filename as a 4th argument and a target yyyymmdd as a 5th argument");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Arguments: {download} | {import symbol yyyymmdd} | {importtext symbol code file.txt yyyymmdd}");
                    return;
                }
            }
            if (Command.Download == command)
                Trace.TraceInformation("Command: download");
            else if (Command.Import == command)
                Trace.TraceInformation("Command: import symbol {0}, target date {1}", symbol, yyyymmdd);
            else if (Command.ImportText == command)
                Trace.TraceInformation("Command: importtext symbol {0}, code {1}. file {2}, target date {3}", symbol, code, file, yyyymmdd);
            Trace.TraceInformation("=======================================================================================");
            Trace.TraceInformation("Started: {0}", DateTime.Now);
            try
            {
                var instance = new CmeFutureUpdate();
                if (Command.Update == command)
                    instance.Update();
                else if (Command.Download == command)
                    instance.Update(false);
                else if (Command.Import == command)
                    instance.Import(symbol, yyyymmdd);
                else if (Command.ImportText == command)
                    instance.ImportText(symbol, code, file, yyyymmdd);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception: [{0}]", e.Message);
            }
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
