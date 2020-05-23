using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace SwpcGoesUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> xs1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> xs5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> xl1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> xl5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> hp1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> he1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> hn1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ht1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ip1_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ip5_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ip10_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ip50_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ip100_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ie08_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ie20_5MList = new SortedList<DateTime, double>(1024);
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
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(input.Substring(12, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(14, 2), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, hour, minute, 0);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void FetchGoesXray(string url, SortedList<DateTime, double> sList, SortedList<DateTime, double> lList, string referer, List<string> echoList)
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
                    string line, missing = "-1.00e+05";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20101024_Gp_xr_5m.txt
                        //:Created: 2010 Oct 24 1540 UTC
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Label: Short = 0.05- 0.4 nanometer
                        //# Label: Long  = 0.1 - 0.8 nanometer
                        //# Units: Short = Watts per meter squared
                        //# Units: Long  = Watts per meter squared
                        //# Source: GOES-14
                        //# Location: W105
                        //# Missing data: -1.00e+05
                        //#
                        //#                         GOES-14 Solar X-ray Flux
                        //# 
                        //#                 Modified Seconds
                        //# UTC Date  Time   Julian  of the
                        //# YR MO DA  HHMM    Day     Day       Short       Long        Ratio
                        //#-------------------------------------------------------------------
                        //2010 10 24  0000   55493      0     2.72e-09    1.03e-07    2.65e-02
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                        {
                            int index = line.IndexOf("Missing data:", StringComparison.Ordinal);
                            if (-1 < index)
                            {
                                Debug.WriteLine(string.Format(">[{0}]", line));
                                missing = line.Substring(index + "Missing data:".Length).Trim();
                                Debug.WriteLine(string.Format("<[{0}]", missing));
                            }
                            continue;
                        }
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vs = line.Substring(32, 12).Trim();
                        string vl = line.Substring(44, 12).Trim();
                        double ds = vs.StartsWith(missing) ? double.NaN : double.Parse(vs, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dl = vl.StartsWith(missing) ? double.NaN : double.Parse(vl, NumberStyles.Any, CultureInfo.InvariantCulture);
                        sList.Add(dt, ds);
                        lList.Add(dt, dl);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), ds, dl));

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

        // ReSharper disable once UnusedParameter.Local
        private static void FetchGoesMag(string url, SortedList<DateTime, double> hpList, SortedList<DateTime, double> heList, SortedList<DateTime, double> hnList, SortedList<DateTime, double> htList, string referer, List<string> echoList)
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
                    string line, missing = "-1.00e+05";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20101024_Gp_mag_1m.txt
                        //:Created: 2010 Oct 24 1901 UTC
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Label: Hp component = perpendicular to the satellite orbital plane or
                        //#           parallel to the Earth's spin axis
                        //# Label: He component = perpendicular to Hp and directed earthwards
                        //# Label: Hn component = perpendicular to both Hp and He, directed eastwards
                        //# Label: Total Field  = 
                        //# Units: nanotesla (nT)
                        //# Source: GOES-13
                        //# Location: W075
                        //# Missing data: -1.00e+05
                        //#
                        //#         1-minute GOES-13 Geomagnetic Components and Total Field
                        //#
                        //#                 Modified Seconds
                        //# UTC Date  Time   Julian  of the
                        //# YR MO DA  HHMM    Day     Day        Hp          He          Hn    Total Field
                        //#-------------------------------------------------------------------------------
                        //2010 10 24  0000   55493      0     8.08e+01    5.04e+01    1.73e+00    9.52e+01
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                        {
                            int index = line.IndexOf("Missing data:", StringComparison.Ordinal);
                            if (-1 < index)
                            {
                                Debug.WriteLine(string.Format(">[{0}]", line));
                                missing = line.Substring(index + "Missing data:".Length).Trim();
                                Debug.WriteLine(string.Format("<[{0}]", missing));
                            }
                            continue;
                        }
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vp = line.Substring(32, 12).Trim();
                        string ve = line.Substring(44, 12).Trim();
                        string vn = line.Substring(56, 12).Trim();
                        string vs = line.Substring(68, 12).Trim();
                        double dp = vp.StartsWith(missing) ? double.NaN : double.Parse(vp, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de = ve.StartsWith(missing) ? double.NaN : double.Parse(ve, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dn = vn.StartsWith(missing) ? double.NaN : double.Parse(vn, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double ds = vs.StartsWith(missing) ? double.NaN : double.Parse(vs, NumberStyles.Any, CultureInfo.InvariantCulture);
                        hpList.Add(dt, dp);
                        heList.Add(dt, de);
                        hnList.Add(dt, dn);
                        htList.Add(dt, ds);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}]", dt.ToShortDateString(), dp, de, dn, ds));

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

        // ReSharper disable once UnusedParameter.Local
        private static void FetchGoesPart(string url, SortedList<DateTime, double> ip1List, SortedList<DateTime, double> ip5List, SortedList<DateTime, double> ip10List, SortedList<DateTime, double> ip50List, SortedList<DateTime, double> ip100List, SortedList<DateTime, double> ie08List, SortedList<DateTime, double> ie20List, string referer, List<string> echoList)
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
                    string line, missing = "-1.00e+05";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20101024_Gp_part_5m.txt
                        //:Created: 2010 Oct 24 2031 UTC
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Label: P > 1 = Particles at >1 Mev
                        //# Label: P > 5 = Particles at >5 Mev
                        //# Label: P >10 = Particles at >10 Mev
                        //# Label: P >30 = Particles at >30 Mev
                        //# Label: P >50 = Particles at >50 Mev
                        //# Label: P>100 = Particles at >100 Mev
                        //# Label: E>0.8 = Electrons at >0.8 Mev
                        //# Label: E>2.0 = Electrons at >2.0 Mev
                        //# Label: E>4.0 = Electrons at >4.0 Mev
                        //# Units: Particles = Protons/cm2-s-sr
                        //# Units: Electrons = Electrons/cm2-s-sr
                        //# Source: GOES-13
                        //# Location: W075
                        //# Missing data: -1.00e+05
                        //#
                        //#                      5-minute  GOES-13 Solar Particle and Electron Flux
                        //#
                        //#                 Modified Seconds
                        //# UTC Date  Time   Julian  of the
                        //# YR MO DA  HHMM    Day     Day     P > 1     P > 5     P >10     P >30     P >50     P>100     E>0.8     E>2.0     E>4.0
                        //#-------------------------------------------------------------------------------------------------------------------------
                        //2010 10 24  0000   55493      0   1.09e+01  2.97e-01  1.58e-01  6.55e-02  5.36e-02  2.82e-02  1.89e+04  1.10e+02 -1.00e+05
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                        {
                            int index = line.IndexOf("Missing data:", StringComparison.Ordinal);
                            if (-1 < index)
                            {
                                Debug.WriteLine(string.Format(">[{0}]", line));
                                missing = line.Substring(index + "Missing data:".Length).Trim();
                                Debug.WriteLine(string.Format("<[{0}]", missing));
                            }
                            continue;
                        }
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vp1 = line.Substring(32, 10).Trim();
                        string vp5 = line.Substring(42, 10).Trim();
                        string vp10 = line.Substring(52, 10).Trim();
                        //vp30 = line.Substring(62, 10).Trim();
                        string vp50 = line.Substring(72, 10).Trim();
                        string vp100 = line.Substring(82, 10).Trim();
                        string ve08 = line.Substring(92, 10).Trim();
                        string ve20 = line.Substring(102, 10).Trim();
                        double dp1 = vp1.StartsWith(missing) ? double.NaN : double.Parse(vp1, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp5 = vp5.StartsWith(missing) ? double.NaN : double.Parse(vp5, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp10 = vp10.StartsWith(missing) ? double.NaN : double.Parse(vp10, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp50 = vp50.StartsWith(missing) ? double.NaN : double.Parse(vp50, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp100 = vp100.StartsWith(missing) ? double.NaN : double.Parse(vp100, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de08 = ve08.StartsWith(missing) ? double.NaN : double.Parse(ve08, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de20 = ve20.StartsWith(missing) ? double.NaN : double.Parse(ve20, NumberStyles.Any, CultureInfo.InvariantCulture);
                        ip1List.Add(dt, dp1);
                        ip5List.Add(dt, dp5);
                        ip10List.Add(dt, dp10);
                        ip50List.Add(dt, dp50);
                        ip100List.Add(dt, dp100);
                        ie08List.Add(dt, de08);
                        ie20List.Add(dt, de20);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}][{5}][{6}][{7}]", dt.ToShortDateString(), dp1, dp5, dp10, dp50, dp100, de08, de20));

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

        // ReSharper disable once UnusedParameter.Local
        private static void Fetch(string url, string referer, List<string> echoList)
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
                string url, h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);

                DateTime d = dt;
                int j = Properties.Settings.Default.DaysBackX1m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/xray/{0}_Gp_xr_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchGoesXray(url, xs1MList, xl1MList, "http://www.swpc.noaa.gov/ftpmenu/lists/xray.html", echoList);
                    url = string.Format("{0}_Gp_xr_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/xray/{0}_Gs_xr_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/xray.html", echoList);
                    url = string.Format("{0}_Gs_xr_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.DaysBackX5m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/xray/{0}_Gp_xr_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchGoesXray(url, xs5MList, xl5MList, "http://www.swpc.noaa.gov/ftpmenu/lists/xray.html", echoList);
                    url = string.Format("{0}_Gp_xr_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/xray/{0}_Gs_xr_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/xray.html", echoList);
                    url = string.Format("{0}_Gs_xr_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.DaysBackH1m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/geomag/{0}_Gp_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchGoesMag(url, hp1MList, he1MList, hn1MList, ht1MList, "http://www.swpc.noaa.gov/ftpmenu/lists/geomag.html", echoList);
                    url = string.Format("{0}_Gp_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/geomag/{0}_Gs_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/geomag.html", echoList);
                    url = string.Format("{0}_Gs_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.DaysBackIp + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/particle/{0}_Gp_part_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchGoesPart(url, ip1_5MList, ip5_5MList, ip10_5MList, ip50_5MList, ip100_5MList, ie08_5MList, ie20_5MList, "http://www.swpc.noaa.gov/ftpmenu/lists/particle.html", echoList);
                    url = string.Format("{0}_Gp_part_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/particle/{0}_Gs_part_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/particle.html", echoList);
                    url = string.Format("{0}_Gs_part_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                Fetch("ftp://ftp.swpc.noaa.gov/pub/lists/xray/README", "http://www.swpc.noaa.gov/ftpmenu/lists/xray.html", echoList);
                if (echoList.Count > 0)
                    SaveFile("readme_xray.txt", echoList);
                Fetch("ftp://ftp.swpc.noaa.gov/pub/lists/geomag/README", "http://www.swpc.noaa.gov/ftpmenu/lists/geomag.html", echoList);
                if (echoList.Count > 0)
                    SaveFile("readme_geomag.txt", echoList);
                Fetch("ftp://ftp.swpc.noaa.gov/pub/lists/particle/README", "http://www.swpc.noaa.gov/ftpmenu/lists/particle.html", echoList);
                if (echoList.Count > 0)
                    SaveFile("readme_particle.txt", echoList);

                if (!Properties.Settings.Default.DownloadOnlyXs1m)
                {
                    Trace.TraceInformation("Updating Xs 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.XsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in xs1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = xs1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyXs5m)
                {
                    Trace.TraceInformation("Updating Xs 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.XsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in xs5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = xs5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyXl1m)
                {
                    Trace.TraceInformation("Updating Xl 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.XlInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in xl1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = xl1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyXl5m)
                {
                    Trace.TraceInformation("Updating Xl 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.XlInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in xl5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = xl5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyHp1m)
                {
                    Trace.TraceInformation("Updating Hp 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HpInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in hp1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = hp1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyHe1m)
                {
                    Trace.TraceInformation("Updating He 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HeInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in he1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = he1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyHn1m)
                {
                    Trace.TraceInformation("Updating Hn 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HnInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in hn1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = hn1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyHt1m)
                {
                    Trace.TraceInformation("Updating Ht 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in ht1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ht1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIp1)
                {
                    Trace.TraceInformation("Updating Ip1 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ip1InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ip1_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ip1_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIp5)
                {
                    Trace.TraceInformation("Updating Ip5 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ip5InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ip5_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ip5_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIp10)
                {
                    Trace.TraceInformation("Updating Ip10 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ip10InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ip10_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ip10_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIp50)
                {
                    Trace.TraceInformation("Updating Ip50 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ip50InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ip50_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ip50_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIp100)
                {
                    Trace.TraceInformation("Updating Ip100 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ip100InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ip100_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ip100_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIe08)
                {
                    Trace.TraceInformation("Updating Ie08 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ie08InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ie08_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ie08_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyIe20)
                {
                    Trace.TraceInformation("Updating Ie20 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ie20InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ie20_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ie20_5MList[r];
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
