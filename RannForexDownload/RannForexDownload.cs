using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Globalization;

namespace mbdt.RannForexDownload
{
    internal static class RannForexDownload
    {
        internal static bool Download(DateTime dateTime, int daysBack)
        {
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("downloading [{0}]", dateTime);
            for (int i = 0; i < daysBack; i++)
            {
                if (i > 0)
                    dateTime = dateTime.AddDays(-1);
                if (!DownloadAllSymbols(dateTime))
                    ok = false;
            }
            return ok;
        }

        private static bool DownloadAllSymbols(DateTime dateTime)
        {
            bool ok = true;
            string downloadable;
            foreach (var v in Properties.Settings.Default.Symbols.Split(';'))
            {
                downloadable = ComposeDownloadable(v, dateTime);
                bool alreadyExists;
                if (!Download(ComposeUrl(v, dateTime), downloadable, Properties.Settings.Default.Referer, out alreadyExists))
                {
                    if (!alreadyExists)
                        ok = false;
                }
            }
            return ok;
        }

        private static string ComposeDownloadable(string symbol, DateTime dateTime)
        {
            string dow = "workdays";
            if (dateTime.DayOfWeek == DayOfWeek.Sunday)
                dow = "sundays";
            else if (dateTime.DayOfWeek == DayOfWeek.Saturday)
                dow = "saturdays";
            string ymdStamp = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return string.Format("{0}\\{1}\\{2}\\{3}\\{4}.zip", Properties.Settings.Default.DownloadDir, dateTime.Year, dow, symbol, ymdStamp);
        }

        private static string ComposeUrl(string symbol, DateTime dateTime)
        {
            string ymdStamp = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return string.Format("https://rannforex.com/static/ticks_archive/{0}.rann_{1}.csv.zip", symbol, ymdStamp);
        }

        private static bool Download(string uri, string filePath, string referer, out bool alreadyExists)
        {
            Debug.WriteLine(string.Concat("downloading ", filePath, " from ", uri));
            alreadyExists = false;
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (null != directoryInfo && !directoryInfo.Exists)
                directoryInfo.Create();
            if (!Properties.Settings.Default.DownloadOverwriteExisting)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > 0)
                    {
                        Trace.TraceInformation("file {0} with length {1} already exists, skipping", filePath, fileInfo.Length);
                        alreadyExists = true;
                        return true;
                    }
                    Trace.TraceInformation("file {0} already exists but length is zero, overwriting", filePath, fileInfo.Length);
                }
            }
            int retries = Properties.Settings.Default.DownloadRetries;
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            while (0 < retries)
            {
                Thread.Sleep(1000);
                long bytesReceived = 0;
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
                    webRequest.Referer = referer;
                    webRequest.UserAgent = Properties.Settings.Default.UserAgent;
                    webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
                    WebResponse webResponse = webRequest.GetResponse();
                    using (var sourceStream = webResponse.GetResponseStream())
                    {
                        if (null != sourceStream)
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
