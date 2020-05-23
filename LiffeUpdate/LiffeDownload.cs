using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

namespace mbdt.LiffeUpdate
{
    internal static class LiffeDownload
    {
        private static readonly List<string> categoryList = new List<string>
        {
            "ds{0}so", "ds{0}sf", "ds{0}io", "ds{0}if", "ds{0}fo", "ds{0}ff", "ds{0}xo", "ds{0}xf",
            "p_ds{0}so", "p_ds{0}sf", "p_ds{0}io", "p_ds{0}if", "p_ds{0}fo", "p_ds{0}ff", "p_ds{0}xo", "p_ds{0}xf",
            "b_ds{0}so", "b_ds{0}sf", "b_ds{0}io", "b_ds{0}if", "b_ds{0}fo", "b_ds{0}ff", "b_ds{0}xo", "b_ds{0}xf",
            "z_ds{0}so", "z_ds{0}sf", "z_ds{0}io", "z_ds{0}if", "z_ds{0}fo", "z_ds{0}ff", "z_ds{0}xo", "z_ds{0}xf"
        };

        internal static bool DownloadDays(DateTime dateTime, bool import, int debugTraceLevel)
        {
            if (dateTime.Hour < 18)
                dateTime = dateTime.AddDays(-1);
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("{0} {1} days back", import ? "updating" : "downloading", Properties.Settings.Default.DownloadLookbackDays);
            for (int i = 0; i < Properties.Settings.Default.DownloadLookbackDays; i++)
            {
                if (!DownloadDay(dateTime, import, debugTraceLevel))
                    ok = false;
                dateTime = dateTime.AddDays(-1);
            }
            return ok;
        }

        private static bool DownloadDay(DateTime dateTime, bool import, int debugTraceLevel)
        {
            string downloadDir = string.Concat(Properties.Settings.Default.DownloadDir, "\\", dateTime.ToString("yyyyMMdd"));
            string stamp = dateTime.ToString("yyMMdd");
            bool ok = true;
            foreach (string format in categoryList)
            {
                string file = string.Format(format, stamp);
                string url = string.Format("http://www.liffe.com/data/{0}.csv", file);
                file = string.Format("{0}\\{1}.csv", downloadDir, file);
                bool alreadyExists;
                if (!Download(url, file, out alreadyExists))
                    ok = false;
                else if (import && !alreadyExists)
                {
                    if (!LiffeImport.DoImport(file, debugTraceLevel))
                        ok = false;
                }
            }
            return ok;
        }

        private static bool Download(string uri, string filePath, out bool alreadyExists)
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
            var r = new Random();
            while (0 < retries)
            {
                int t = r.Next(Properties.Settings.Default.SleepBeforeDownloadMax);
                if (t < Properties.Settings.Default.SleepBeforeDownloadMin)
                    t = Properties.Settings.Default.SleepBeforeDownloadMin;
                Thread.Sleep(t);
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current 
                    // security context in which the application is running. For a client-side 
                    // application, these are usually the Windows credentials 
                    // (user name, password, and domain) of the user running the application. 
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    //webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Referer = Properties.Settings.Default.Referer;
                    webRequest.UserAgent = Properties.Settings.Default.UserAgent;
                    webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
                    webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.KeepAlive = Properties.Settings.Default.DownloadKeepAlive;
                    WebResponse webResponse = webRequest.GetResponse();
                    long bytesReceived = 0;
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
