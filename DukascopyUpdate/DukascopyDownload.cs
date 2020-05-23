using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Globalization;

namespace mbdt.DukascopyUpdate
{
    internal static class DukascopyDownload
    {
        private static void FetchSymbols(List<string> nameList, List<string> valueList)
        {
            string[] symbols = Properties.Settings.Default.Symbols.Split(',');
            for (int i = 0; i < symbols.Length; )
            {
                valueList.Add(symbols[i++]);
                nameList.Add(symbols[i++]);
            }
        }

        internal static bool DownloadSymbols(DateTime dateTime, bool import, int debugTraceLevel)
        {
            bool ok = true;
            Trace.TraceInformation("-------------------------------------------------------");
            Trace.TraceInformation("{0} {1} days back", import ? "updating" : "downloading", Properties.Settings.Default.DownloadLookbackDays);
            List<string> nameList = new List<string>(), valueList = new List<string>();
            FetchSymbols(nameList, valueList);
            string downloadDir = string.Concat(Properties.Settings.Default.DownloadDir, "\\", dateTime.ToString("yyyyMMdd"));
            for (int i = 0; i < nameList.Count; i++)
            {
                if (!DownloadSymbol(downloadDir, nameList[i], valueList[i], dateTime, import, debugTraceLevel))
                    ok = false;
            }
            return ok;
        }

        private static readonly string[] intervals = { "60", "600", "3600", "1D" };
        private static readonly int[] pointsPerDay = { 24 * 60, 24 * 6, 24, 1 };

        private static bool DownloadSymbol(string downloadDir, string symbol, string symbolNumber, DateTime dateTime, bool import, int debugTraceLevel)
        {
            string file, url, urlFormat = "http://www.dukascopy.com/freeApplets/exp/exp.php?fromD={0}&np={1}&interval={2}&DF=d-m-Y&Stock={3}&endSym=win&split=tz";
            string stamp = dateTime.ToString("yyyyMMdd");
            bool ok = true, alreadyExists;
            int pointsTotal, np, j;
            double days = 0;
            DateTime dt;
            for (int i = 0; i < intervals.Length; i++)
            {
                pointsTotal = pointsPerDay[i] * Properties.Settings.Default.DownloadLookbackDays;
                dt = dateTime;
                j = 999;
                while (0 < pointsTotal)
                {
                    if (pointsTotal < 250)
                        np = 250;
                    else if (pointsTotal < 500)
                        np = 500;
                    else if (pointsTotal < 1000)
                        np = 1000;
                    else //if (pointsTotal < 2000)
                        np = 2000;
                    url = dt.ToString("MM.dd.yyyy");
                    url = string.Format(urlFormat, url, np.ToString(CultureInfo.InvariantCulture), intervals[i], symbolNumber);
                    file = string.Format("{0}\\{1}_{2}_{3}_{4}.csv", downloadDir, symbol, stamp, intervals[i], j--);
                    if (!Download(url, file, out alreadyExists))
                        ok = false;
                    else if (import && !alreadyExists)
                    {
                        if (!DukascopyImport.DoImport(file, debugTraceLevel))
                            ok = false;
                    }
                    pointsTotal -= np;
                    days = (double)np / (double)pointsPerDay[i];
                    dt = dt.AddDays(-days);
                }
            }
            return ok;
        }

        private static bool Download(string uri, string filePath, out bool alreadyExists)
        {
            Debug.WriteLine(string.Concat("downloading ", filePath, " from ", uri));
            alreadyExists = false;
            FileInfo fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (!directoryInfo.Exists)
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
            int t, retries = Properties.Settings.Default.DownloadRetries;
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            long bytesReceived = 0;
            Random r = new Random();
            while (0 < retries)
            {
                t = r.Next(Properties.Settings.Default.SleepBeforeDownloadMax);
                if (t < Properties.Settings.Default.SleepBeforeDownloadMin)
                    t = Properties.Settings.Default.SleepBeforeDownloadMin;
                Trace.TraceInformation("Sleeping {0} ticks", t);
                Thread.Sleep(t);
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
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
                    using (var sourceStream = webResponse.GetResponseStream())
                    {
                        using (var targetStream = new StreamWriter(filePath, false))
                        {
                            int bytesRead = 0;
                            while (0 < (bytesRead = sourceStream.Read(buffer, 0, bufferSize)))
                                targetStream.BaseStream.Write(buffer, 0, bytesRead);
                            bytesReceived = targetStream.BaseStream.Length;
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
                        bytesReceived = 0;
                    }
                }
                catch (Exception e)
                {
                    if (1 < retries)
                        Trace.TraceError("file {0}: download failed [{1}], retrying ({2})", filePath, e.Message, retries);
                    else
                        Trace.TraceError("file {0}: download failed [{1}], giving up ({2})", filePath, e.Message, retries);
                    retries--;
                    bytesReceived = 0;
                }
            }
            return File.Exists(filePath);
        }
    }
}
