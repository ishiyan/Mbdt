using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

namespace mbdt.DukascopyFxUpdate
{
    internal static class DukascopyFxDownload
    {
        internal static bool DownloadDaysFromDate(DateTime dateTime, bool import, bool importCandles, int debugTraceLevel)
        {
            bool ok = true;
            for (int i = 0; i < DukascopyFxContext.DownloadLookbackDays; i++)
            {
                dateTime = dateTime.AddDays(-1);
                if (!DownloadDays(dateTime, import, importCandles, debugTraceLevel))
                    ok = false;
            }
            return ok;
        }

        internal static bool DownloadMonthsFromDate(DateTime dateTime, bool import, int debugTraceLevel)
        {
            bool ok = true;
            for (int i = 0; i < DukascopyFxContext.DownloadLookbackMonths; i++)
            {
                dateTime = dateTime.AddMonths(-1);
                if (!DownloadMonths(dateTime, import, debugTraceLevel))
                    ok = false;
            }
            return ok;
        }

        private static bool DownloadDays(DateTime dateTime, bool import, bool importCandles, int debugTraceLevel)
        {
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("{0} day {1:yyyy-MM-dd}", import ? "updating" : "downloading", dateTime);
            foreach (var v in DukascopyFxContext.Symbols)
            {
                DukascopyFxContext ctx = DownloadDays(v, dateTime, import, importCandles, debugTraceLevel);
                if (!ctx.Ok)
                    ok = false;
            }
            return ok;
        }

        private static bool DownloadMonths(DateTime dateTime, bool import, int debugTraceLevel)
        {
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("{0} month {1:yyyy-MM}", import ? "updating" : "downloading", dateTime);
            foreach (var v in DukascopyFxContext.Symbols)
            {
                DukascopyFxContext ctx = DownloadMonths(v, dateTime, import, debugTraceLevel);
                if (!ctx.Ok)
                    ok = false;
            }
            return ok;
        }

        private static DukascopyFxContext DownloadDays(string symbol, DateTime dateTime, bool import, bool importCandles, int debugTraceLevel)
        {
            var ctx = new DukascopyFxContext(symbol, dateTime) {Ok = true};
            bool alreadyExists;
            string name;
            for (int i = 0; i < DukascopyFxContext.DayBinFiles.Length; ++i )
            {
                name = string.Concat(ctx.DayDirectoryDownloadBase, DukascopyFxContext.DayBinFiles[i]);
                if (!Download(string.Concat(ctx.DayUriPrefix, DukascopyFxContext.DayBi5Files[i]), name, out alreadyExists))
                    ctx.Ok = false;
                else if (import && importCandles && !alreadyExists)
                {
                    if (!DukascopyFxImport.DoImport(name, true, debugTraceLevel))
                        ctx.Ok = false;
                }
            }
            for (int i = 0; i < DukascopyFxContext.TickBinFiles.Length; ++i)
            {
                name = string.Concat(ctx.DayDirectoryDownloadBase, DukascopyFxContext.TickBinFiles[i]);
                if (!Download(string.Concat(ctx.DayUriPrefix, DukascopyFxContext.TickBi5Files[i]), name, out alreadyExists))
                    ctx.Ok = false;
                else if (import && !alreadyExists)
                {
                    if (!DukascopyFxImport.DoImport(name, false, debugTraceLevel))
                        ctx.Ok = false;
                }
            }
            return ctx;
        }

        private static DukascopyFxContext DownloadMonths(string symbol, DateTime dateTime, bool import, int debugTraceLevel)
        {
            var ctx = new DukascopyFxContext(symbol, dateTime) {Ok = true};
            for (int i = 0; i < DukascopyFxContext.MonthBinFiles.Length; ++i)
            {
                string name = string.Concat(ctx.MonthDirectoryDownloadBase, DukascopyFxContext.MonthBinFiles[i]);
                if (!Download(string.Concat(ctx.MonthUriPrefix, DukascopyFxContext.MonthBi5Files[i]), name, out bool alreadyExists))
                    ctx.Ok = false;
                else if (import && !alreadyExists)
                {
                    if (!DukascopyFxImport.DoImport(name, true, debugTraceLevel))
                        ctx.Ok = false;
                }
            }
            return ctx;
        }

        private static bool Download(string uri, string filePath, out bool alreadyExists)
        {
            Trace.TraceInformation(string.Concat("downloading ", filePath, " from ", uri));
            alreadyExists = false;
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (null != directoryInfo && !directoryInfo.Exists)
                directoryInfo.Create();
            if (!DukascopyFxContext.DownloadOverwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > 0)
                    {
                        Trace.TraceInformation("file {0} with length {1} already exists, skipping", filePath, fileInfo.Length);
                        alreadyExists = true;
                        return true;
                    }
                    Trace.TraceInformation("file {0} already exists but length {1} is zero, overwriting", filePath, fileInfo.Length);
                }
            }
            int retries = DukascopyFxContext.DownloadRetries;
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            while (0 < retries)
            {
                Thread.Sleep(1000);
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current 
                    // security context in which the application is running. For a client-side 
                    // application, these are usually the Windows credentials 
                    // (user name, password, and domain) of the user running the application. 
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Referer = DukascopyFxContext.Referrer;
                    webRequest.UserAgent = Properties.Settings.Default.UserAgent;
                    webRequest.Timeout = DukascopyFxContext.DownloadTimeout;
                    WebResponse webResponse = webRequest.GetResponse();
                    long bytesReceived = 0;
                    using (var sourceStream = webResponse.GetResponseStream())
                    {
                        if (sourceStream != null)
                        {
                            using (var targetStream = new StreamWriter(filePath, false))
                            {
                                int bytesRead;
                                while (0 < (bytesRead = sourceStream.Read(buffer, 0, bufferSize)))
                                    targetStream.BaseStream.Write(buffer, 0, bytesRead);
                                bytesReceived = targetStream.BaseStream.Length;
                            }
                        }
                    }
                    if (bytesReceived >= 0)
                        retries = 0;
                    else
                    {
                        if (1 < retries)
                            Trace.TraceError("file {0}: downloaded length {1} is less than zero, retrying", filePath, bytesReceived);
                        else
                        {
                            Trace.TraceError("file {0}: downloaded length {1} is less than zero, giving up", filePath, bytesReceived);
                            File.Delete(filePath);
                        }
                        retries--;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(1 < retries ? "file {0}: download failed [{1}], retrying ({2})" : "file {0}: download failed [{1}], giving up ({2})", filePath, e.Message, retries);
                    retries--;
                }
            }
            return File.Exists(filePath);
        }
    }
}
