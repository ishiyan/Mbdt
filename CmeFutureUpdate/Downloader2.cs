using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace CmeFutureUpdate
{
    static class Downloader2
    {
        private static bool firstTime = true;

        static Downloader2()
        {
            // Skip validation of SSL/TLS certificate
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Ssl3;
        }

        public static bool Download(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout, string referer=null, string userAgent=null, string accept=null)
        {
            if (firstTime)
            {
                firstTime = false;
                if (Download(uri, filePath, minimalLength, overwrite, retries, timeout, referer, userAgent, accept))
                    return true;
            }
            Debug.WriteLine(string.Concat("downloading ", filePath, " from ", uri));
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (null != directoryInfo && !directoryInfo.Exists)
                directoryInfo.Create();
            if (!overwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > minimalLength)
                    {
                        Trace.TraceInformation("file {0} already exists, skipping", filePath);
                        return true;
                    }
                    Trace.TraceInformation("file {0} already exists but length {1} is smaller than the minimal length {2}, overwriting", filePath, fileInfo.Length, minimalLength);
                }
            }
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            long bytesReceived = 0;
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
                    if (!string.IsNullOrEmpty(referer))
                        webRequest.Referer = referer;
                    if (string.IsNullOrEmpty(userAgent))
                        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    if (!string.IsNullOrEmpty(accept))
                        webRequest.Accept = accept;
                    // webRequest.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
                    webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                    webRequest.Headers.Add("sec-ch-ua", "\"Not A;Brand\"; v = \"99\", \"Chromium\"; v = \"90\", \"Microsoft Edge\"; v = \"90\"");
                    webRequest.Headers.Add("sec-ch-ua-mobile", "?0");
                    webRequest.Headers.Add("Sec-Fetch-Dest", "document");
                    webRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
                    webRequest.Headers.Add("Sec-Fetch-Site", "none");
                    webRequest.Headers.Add("Sec-Fetch-User", "?1");
                    webRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
                    webRequest.KeepAlive = true;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
                    Uri target = new Uri("https://www.cmegroup.com/");
                    var cookieContained = new CookieContainer();
                    cookieContained.Add(new Cookie("_CEFT", "Q%3D%3D%3D") { Domain = target.Host });
                    cookieContained.Add(new Cookie("_gcl_au", "1.1.309745119.1616008679") { Domain = target.Host });
                    cookieContained.Add(new Cookie("_fbp", "fb.1.1616008680429.1394031946") { Domain = target.Host });
                    cookieContained.Add(new Cookie("kppid", "ct7UM00vnHb") { Domain = target.Host });
                    cookieContained.Add(new Cookie("cmeConsentCookie", "true") { Domain = target.Host });
                    cookieContained.Add(new Cookie("__atuvc", "0%7C16%2C0%7C17%2C0%7C18%2C0%7C19%2C2%7C20") { Domain = target.Host });
                    cookieContained.Add(new Cookie("_ga", "GA1.2.886178196.1591878191") { Domain = target.Host });
                    cookieContained.Add(new Cookie("_ga_L69G7D7MMN", "GS1.1.1621363199.4.1.1621363463.0") { Domain = target.Host });
                    webRequest.CookieContainer = cookieContained;
                    WebResponse webResponse = webRequest.GetResponse();
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
                    if (bytesReceived > minimalLength)
                        retries = 0;
                    else
                    {
                        if (1 < retries)
                            Trace.TraceError("file {0}: downloaded length {1} is smaller than the minimal length {2}, retrying", filePath, bytesReceived, minimalLength);
                        else
                        {
                            Trace.TraceError("file {0}: downloaded length {1} is smaller than the minimal length {2}, giving up", filePath, bytesReceived, minimalLength);
                            File.Delete(filePath);
                        }
                        retries--;
                        bytesReceived = 0;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(1 < retries ? "file {0}: download failed [{1}], retrying ({2})" : "file {0}: download failed [{1}], giving up ({2})", filePath, e.Message, retries);
                    retries--;
                    bytesReceived = 0;
                }
            }
            return (0 < bytesReceived);
        }

        public static bool DownloadPost(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout, Dictionary<string,string> keyValueDictionary, string referer = null, string userAgent = null, string accept = null)
        {
            Debug.WriteLine(string.Concat("downloading (post)", filePath, " from ", uri));
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (null != directoryInfo && !directoryInfo.Exists)
                directoryInfo.Create();
            if (!overwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > minimalLength)
                    {
                        Trace.TraceInformation("file {0} already exists, skipping", filePath);
                        return true;
                    }
                    Trace.TraceInformation("file {0} already exists but length {1} is smaller than the minimal length {2}, overwriting", filePath, fileInfo.Length, minimalLength);
                }
            }
            var builder = new StringBuilder(1024);
            foreach (var kvp in keyValueDictionary)
            {
                if (builder.Length != 0)
                    builder.Append("&");
                builder.Append(kvp.Key);
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(kvp.Value));

            }
            string postData = builder.ToString();
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            long bytesReceived = 0;
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
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = postData.Length;
                    if (!string.IsNullOrEmpty(referer))
                        webRequest.Referer = referer;
                    if (string.IsNullOrEmpty(userAgent))
                        userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:15.0) Gecko/20100101 Firefox/15.0";
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    if (!string.IsNullOrEmpty(accept))
                        webRequest.Accept = accept;
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.KeepAlive = true;
                    using (Stream writeStream = webRequest.GetRequestStream())
                    {
                        var encoding = new UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postData);
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                    WebResponse webResponse = webRequest.GetResponse();
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
                    if (bytesReceived > minimalLength)
                        retries = 0;
                    else
                    {
                        if (1 < retries)
                            Trace.TraceError("file {0}: downloaded length {1} is smaller than the minimal length {2}, retrying", filePath, bytesReceived, minimalLength);
                        else
                        {
                            Trace.TraceError("file {0}: downloaded length {1} is smaller than the minimal length {2}, giving up", filePath, bytesReceived, minimalLength);
                            File.Delete(filePath);
                        }
                        retries--;
                        bytesReceived = 0;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(1 < retries ? "file {0}: download failed [{1}], retrying ({2})" : "file {0}: download failed [{1}], giving up ({2})", filePath, e.Message, retries);
                    retries--;
                    bytesReceived = 0;
                }
            }
            return (0 < bytesReceived);
        }
    }
}
