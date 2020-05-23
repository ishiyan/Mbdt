using System;
using System.IO;
using System.Diagnostics;

using Mbh5;

namespace EcbDailyUpdate
{
    static class Program
    {
        static void Main()
        {
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            Trace.TraceInformation("Started: {0}", DateTime.Now);
            try
            {
                var fileInfo = new FileInfo(Properties.Settings.Default.RepositoryFile);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                new EcbDailyUpdate().Import();
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception: [{0}]", e.Message);
            }
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
