using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

namespace mbdt.GaincapitalFxUpdate
{
    internal static class GaincapitalFxDownload
    {
        internal static bool Download(DateTime dateTime, bool import, int debugTraceLevel)
        {
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("{0} [{1}]", import ? "updating" : "downloading", dateTime);
            int year = dateTime.Year, month = dateTime.Month, day = dateTime.Day;
            if (day < 8)
            {
                month--;
                if (month < 1)
                {
                    month = 12;
                    year--;
                }
                if (!DownloadAllPairs(year, month, 4, import, debugTraceLevel))
                    ok = false;
                if (!DownloadAllPairs(year, month, 5, import, debugTraceLevel))
                    ok = false;
            }
            else if (day < 15)
            {
                if (!DownloadAllPairs(year, month, 1, import, debugTraceLevel))
                    ok = false;
            }
            else if (day < 22)
            {
                if (!DownloadAllPairs(year, month, 2, import, debugTraceLevel))
                    ok = false;
            }
            else if (day < 32)
            {
                if (!DownloadAllPairs(year, month, 3, import, debugTraceLevel))
                    ok = false;
            }
            return ok;
        }

        private static IEnumerable<string> FetchList(int year, int month)
        {
            string url = ComposeIndexUrl(year, month);
            string referer = ComposeIndexUrlReferer(year);
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
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
            const string pat = "<a href=\".\\"; //<a href=".\
            const int patLen = 11;
            var list = new List<string>(64);
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                if (null == responseStream)
                {
                    Trace.TraceError("Received null response stream.");
                    return list;
                }
                using (var streamReader = new StreamReader(responseStream))
                {
                    string line;
                    while (null != (line = streamReader.ReadLine()))
                    {
                        int i = line.IndexOf(pat, StringComparison.Ordinal);
                        if (0 > i)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        string s = line.Substring(i + patLen, 7).Replace("_", "");
                        Debug.WriteLine(string.Format("<[{0}]", s));
                        if (!list.Contains(s))
                            list.Add(s);
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] pairs fetched", list.Count);
            }
            catch (WebException ex)
            {
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
            if (0 == list.Count)
            {
                foreach (var v in Properties.Settings.Default.Symbols.Split(','))
                {
                    if (!list.Contains(v))
                        list.Add(v);
                }
            }
            return list;
        }

        private static bool DownloadAllPairs(int year, int month, int week, bool import, int debugTraceLevel)
        {
            bool ok = true;
            string referer = ComposeReferer(year, month);
            foreach (var v in FetchList(year, month))
            {
                string downloadable = ComposeDownloadable(v, year, month, week);
                bool alreadyExists;
                if (!Download(ComposeUrl(referer, v, week), downloadable, referer, out alreadyExists))
                    ok = true;//false;
                else if (import && !alreadyExists)
                {
                    if (!GaincapitalFxImport.DoImport(downloadable, debugTraceLevel))
                        ok = false;
                }
            }
            return ok;
        }

        private static string ComposeDownloadable(string symbol, int year, int month, int week)
        {
            string[] monthName = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            return string.Format("{0}\\{1}\\{2}\\{3}_{4}_Week{5}.zip", Properties.Settings.Default.DownloadDir, year, monthName[month - 1], symbol.Substring(0, 3), symbol.Substring(3), week);
        }

        private static string ComposeReferer(int year, int month)
        {
            string[] monthName = { "01 January", "02 February", "03 March", "04 April", "05 May", "06 June", "07 July", "08 August", "09 September", "10 October", "11 November", "12 December" };
            return string.Format("http://ratedata.gaincapital.com/{0}/{1}/", year, monthName[month - 1]);
        }

        private static string ComposeIndexUrl(int year, int month)
        {
            string[] monthName = { "01 January", "02 February", "03 March", "04 April", "05 May", "06 June", "07 July", "08 August", "09 September", "10 October", "11 November", "12 December" };
            return string.Format("http://ratedata.gaincapital.com/{0}/{1}/", year, monthName[month - 1]);
        }

        private static string ComposeIndexUrlReferer(int year)
        {
            return string.Format("http://ratedata.gaincapital.com/{0}/", year);
        }

        private static string ComposeUrl(string referer, string symbol, int week)
        {
            return string.Format("{0}{1}_{2}_Week{3}.zip", referer, symbol.Substring(0, 3), symbol.Substring(3), week);
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
