using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Globalization;
using System.IO.Compression;

namespace mbdt.NedkoersDownload
{
    internal static class NedkoersDownload
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
            List<string> list = new List<string>(), listNames = new List<string>();
            foreach (var v in Properties.Settings.Default.Symbols.Split(','))
            {
                downloadable = ComposeDownloadable(v, dateTime);
                string name = ComposeDownloadableName(v, dateTime);
                bool alreadyExists;
                if (!Download(ComposeUrl(v, dateTime), downloadable, Properties.Settings.Default.Referer, out alreadyExists))
                {
                    if (!alreadyExists)
                        ok = false;
                    else
                    {
                        list.Add(downloadable);
                        listNames.Add(name);
                    }
                }
                else
                {
                    list.Add(downloadable);
                    listNames.Add(name);
                }
            }
            if (Properties.Settings.Default.SingleZip)
            {
                downloadable = ComposeDownloadable("a1ex_nk_all.zip", dateTime);
                try
                {
                    using (ZipStorer zipStorer = ZipStorer.Create(downloadable, ""))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            zipStorer.AddFile(ZipStorer.Compression.Deflate, list[i], listNames[i], "");

                            // It is cumbersome to unzip plain csv files because they are in not standard format.
                            // Store zips instead.

                            ////using (MemoryStream stream = UnzippedFirstEntry(v))
                            //using (FileStream fs = File.OpenRead(v))
                            //{
                            //    using (GZipStream gstream = new GZipStream(fs, CompressionMode.Decompress, false))
                            //    {
                            //        using (MemoryStream stream = new MemoryStream(1024 * 1024))
                            //        {
                            //            gstream.CopyTo(stream);
                            //            stream.Position = 0;
                            //            zipStorer.AddStream(ZipStorer.Compression.Deflate, v.Replace(".zip", ".csv"), stream, dateTime, "");
                            //            stream.Close();
                            //        }
                            //        gstream.Close();
                            //    }
                            //    fs.Close();
                            //}
                            //using (Package package = Package.Open(v, FileMode.Open, FileAccess.Read))
                            //{
                            //    foreach (PackagePart part in package.GetParts())
                            //    {
                            //        using (Stream stream = part.GetStream(FileMode.Open, FileAccess.Read))
                            //        {
                            //            zipStorer.AddStream(ZipStorer.Compression.Deflate, v.Replace(".zip", ".csv"), stream, dateTime, "");
                            //            stream.Close();
                            //        }
                            //        break;
                            //    }
                            //}
                        }
                        //zipStorer.Close();
                    }
                    foreach (var v in list)
                    {
                        File.Delete(v);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Exception: [{0}]", ex.Message);
                }
            }
            return ok;
        }

        //static private MemoryStream UnzippedFirstEntry(string zipFile)
        //{
        //    if (!File.Exists(zipFile))
        //    {
        //        Trace.TraceError(String.Concat("Zip file does not exist: ", zipFile));
        //        return null;
        //    }
        //    FileInfo fileInfo = new FileInfo(zipFile);
        //    if (0 == fileInfo.Length)
        //    {
        //        Trace.TraceInformation(String.Concat("Zip file has zero length: ", zipFile));
        //        return null;
        //    }
        //    ZipStorer zip = ZipStorer.Open(zipFile, FileAccess.Read);
        //    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
        //    if (1 != dir.Count)
        //    {
        //        zip.Close();
        //        Trace.TraceError(String.Concat("Zip file does not contain the only entry: ", zipFile));
        //        return null;
        //    }
        //    MemoryStream stream = new MemoryStream(1024 * 256);
        //    zip.ExtractFile(dir[0], stream);
        //    zip.Close();
        //    return stream;
        //}

        private static string ComposeDownloadableName(string symbol, DateTime dateTime)
        {
            string ymdStamp = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return string.Format("{0}{1}", ymdStamp, symbol);
        }

        private static string ComposeDownloadable(string symbol, DateTime dateTime)
        {
            string dow = "workdays";
            if (dateTime.DayOfWeek == DayOfWeek.Sunday)
                dow = "sundays";
            else if (dateTime.DayOfWeek == DayOfWeek.Saturday)
                dow = "saturdays";
            string ymdStamp = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return string.Format("{0}\\{1}\\{2}\\{3}{4}", Properties.Settings.Default.DownloadDir, dateTime.Year, dow, ymdStamp, symbol);
        }

        private static string ComposeUrl(string symbol, DateTime dateTime)
        {
            string mdStamp = dateTime.ToString("MMdd", CultureInfo.InvariantCulture);
            return string.Format("http://www.fibbs.nl/user/MarketsNew.cgi/{0}{1}?UserId=9cab8a6366fd6da89b9376f9e7844965&Key=9560c0e87739bf7a8ebf2123888c25dd&Action=GetFile&DownLoadFormaat=Fibbs2&DownLoadFile={2}{3}", mdStamp, symbol, mdStamp, symbol);
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
