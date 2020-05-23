using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace NgdcApstarUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> aps3HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> aps1DList = new SortedList<DateTime, double>(1024);
        static private string folder, downloadFolder;

        private static void SaveFile(string fileName, IEnumerable<string> echoList)
        {
            string f = string.Concat(downloadFolder, "\\", fileName);
            Trace.TraceInformation("Writing " + f);
            using (var destFile = new StreamWriter(f))
            {
                foreach (var v in echoList)
                    destFile.WriteLine(v);
            }
            Trace.TraceInformation("Saved " + f);
        }

        private static DateTime StringToDateTime(string input)
        {
            int year = int.Parse(input.Substring(0, 2), CultureInfo.InvariantCulture);
            if (20 > year)
                year += 2000;
            else
                year += 1900;
            int month = int.Parse(input.Substring(2, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(4, 2), CultureInfo.InvariantCulture);
            DateTime dt;
            try
            {
                dt = new DateTime(year, month, day, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Bad date format on line [{0}], skipping: [{1}]", input, ex.Message);
                dt = new DateTime(0);
            }
            return dt;
        }

        private static void FetchList(string url, SortedList<DateTime, double> dList, SortedList<DateTime, double> hList/*, string referer*/, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (FtpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.KeepAlive = true;
            //webRequest.Referer = referer;
            int w = 0;
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                if (null == responseStream)
                {
                    Trace.TraceError("Received null response stream.");
                    return;
                }
                using (var streamReader = new StreamReader(responseStream))
                {
                    string line;
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //32 1 11352103327232733273333237 18 12  9 12 18 12 18 18 150.84 13  0.03
                        //32 1 21352113737333733473050303 22 22 18 22 18 39 15 48 261.26 26  0.03
                        if (line.StartsWith(""))//0x1a
                            break;
                        if (3 > line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        if (0 == dt.Ticks)
                            continue;
                        string vh1 = line.Substring(31, 3).Trim();
                        string vh2 = line.Substring(34, 3).Trim();
                        string vh3 = line.Substring(37, 3).Trim();
                        string vh4 = line.Substring(40, 3).Trim();
                        string vh5 = line.Substring(43, 3).Trim();
                        string vh6 = line.Substring(46, 3).Trim();
                        string vh7 = line.Substring(49, 3).Trim();
                        string vh8 = line.Substring(52, 3).Trim();
                        string vd = line.Substring(55, 3).Trim();
                        double dh1 = double.Parse(vh1, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh2 = double.Parse(vh2, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh3 = double.Parse(vh3, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh4 = double.Parse(vh4, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh5 = double.Parse(vh5, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh6 = double.Parse(vh6, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh7 = double.Parse(vh7, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dh8 = double.Parse(vh8, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dd = double.Parse(vd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        dList.Add(dt, dd);
                        hList.Add(dt, dh1);
                        hList.Add(dt.AddHours(3), dh2);
                        hList.Add(dt.AddHours(6), dh3);
                        hList.Add(dt.AddHours(9), dh4);
                        hList.Add(dt.AddHours(12), dh5);
                        hList.Add(dt.AddHours(15), dh6);
                        hList.Add(dt.AddHours(18), dh7);
                        hList.Add(dt.AddHours(21), dh8);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}]1d[{1}]3h[{2}][{3}][{4}][{5}][{6}][{7}][{8}][{9}]", dt.ToShortDateString(), dd, dh1, dh2, dh3, dh4, dh5, dh6, dh7, dh8));

                        // Get rid of exception: [Cannot access a disposed object. Object name: 'System.Net.Sockets.NetworkStream'.]
                        try
                        {
                            if (streamReader.EndOfStream)
                                break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: [{0}], uri=[{1}], {2} rows parsed", ex.Message, url, echoList.Count);
            }
        }

        private static void Fetch(string url/*, string referer*/, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (FtpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            //webRequest.Referer = referer;
            int w = 0;
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                if (null == responseStream)
                {
                    Trace.TraceError("Received null response stream.");
                    return;
                }
                using (var streamReader = new StreamReader(responseStream))
                {
                    string line;
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        w++;

                        // Get rid of exception: [Cannot access a disposed object. Object name: 'System.Net.Sockets.NetworkStream'.]
                        try
                        {
                            if (streamReader.EndOfStream)
                                break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: [{0}], uri=[{1}], {2} rows parsed", ex.Message, url, echoList.Count);
            }
        }

        static void Main()
        {
            Repository repository = null;
            ScalarData scalarData = null;
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            var echoList = new List<string>(4096);
            Instrument instrument = null;
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            DateTime dt = DateTime.Now;
            Trace.TraceInformation("Started: {0}", dt);
            try
            {
                folder = dt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                downloadFolder = string.Concat(Properties.Settings.Default.DownloadFolder, folder);
                if (!Directory.Exists(downloadFolder))
                    Directory.CreateDirectory(downloadFolder);
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                FetchList("ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/APSTAR/apindex", aps1DList, aps3HList/*, "http://www.ngdc.noaa.gov/stp/geomag/apstar.html"*/, echoList);
                string url = string.Format("{0}_apindex.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                if (echoList.Count > 0)
                    SaveFile(url, echoList);
                Fetch("ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/APSTAR/apindex.fmt"/*, "http://www.ngdc.noaa.gov/stp/geomag/apstar.html"*/, echoList);
                url = string.Format("{0}_apindex_fmt.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                if (echoList.Count > 0)
                    SaveFile(url, echoList);

                if (!Properties.Settings.Default.DownloadOnlyApStar1d)
                {
                    Trace.TraceInformation("Updating Ap* 1d: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.ApStarInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in aps1DList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = aps1DList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyApStar3h)
                {
                    Trace.TraceInformation("Updating Ap* 3h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.ApStarInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour3, true);
                    scalarList.Clear();
                    foreach (var r in aps3HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = aps3HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception: [{0}]", e.Message);
            }
            finally
            {
                if (null != scalarData)
                {
                    scalarData.Flush();
                    scalarData.Close();
                }
                if (null != instrument)
                    instrument.Close();
                if (null != repository)
                    repository.Close();
            }
            Trace.TraceInformation("Zipping: {0}", DateTime.Now);
            Packager.ZipTxtDirectory(string.Concat(downloadFolder, ".zip"), downloadFolder, true);
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
