using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace mbdt.Utils
{
    static class Downloader
    {
        private static bool firstTime = true;

        static Downloader()
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
                        userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:15.0) Gecko/20100101 Firefox/15.0";
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    if (!string.IsNullOrEmpty(accept))
                        webRequest.Accept = accept;
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.KeepAlive = true;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
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
