using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace SwpcAceUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> ht_1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ht_1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pd_1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pd_1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ps_1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ps_1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pt_1MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pt_1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> p10_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> p10_1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> p30_5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> p30_1HList = new SortedList<DateTime, double>(1024);
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
        private static void FetchMag(string url, SortedList<DateTime, double> htList, string referer, List<string> echoList)
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
                    const string missing = "-999.9";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20100924_ace_mag_1m.txt
                        //:Created: 2010 Sep 25 0011 UT
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Magnetometer values are in GSM coordinates.
                        //# 
                        //# Units: Bx, By, Bz, Bt in nT
                        //# Units: Latitude  degrees +/-  90.0
                        //# Units: Longitude degrees 0.0 - 360.0
                        //# Status(S): 0 = nominal data, 1 to 8 = bad data record, 9 = no data
                        //# Missing data values: -999.9
                        //# Source: ACE Satellite - Magnetometer
                        //#
                        //#              1-minute averaged Real-time Interplanetary Magnetic Field Values 
                        //# 
                        //#                 Modified Seconds
                        //# UT Date   Time  Julian   of the   ----------------  GSM Coordinates ---------------
                        //# YR MO DA  HHMM    Day      Day    S     Bx      By      Bz      Bt     Lat.   Long.
                        //#------------------------------------------------------------------------------------
                        //2010 09 24  0000   55463       0    0     3.3    -8.6     2.2     9.5    13.4   290.9
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        if (0 == line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vh = line.Substring(61, 8).Trim();
                        double dh;
                        if (vh.StartsWith(missing))
                            dh = double.NaN;
                        else
                        {
                            dh = double.Parse(vh, NumberStyles.Any, CultureInfo.InvariantCulture);
                            htList.Add(dt, dh);
                        }
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}]", dt.ToShortDateString(), dh));

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
        private static void FetchSwepam(string url, SortedList<DateTime, double> pdList, SortedList<DateTime, double> psList, SortedList<DateTime, double> ptList, string referer, List<string> echoList)
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
                    const string missing2 = "-9999.9";
                    const string missing = "-1.00e+05";
                    const string missing3 = "-1.0e+05";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20100924_ace_swepam_1m.txt
                        //:Created: 2010 Sep 25 0011 UT
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Units: Proton density p/cc
                        //# Units: Bulk speed km/s
                        //# Units: Ion tempeture degrees K
                        //# Status(S): 0 = nominal data, 1 to 8 = bad data record, 9 = no data
                        //# Missing data values: Density and Speed = -9999.9, Temp. = -1.00e+05
                        //# Source: ACE Satellite - Solar Wind Electron Proton Alpha Monitor
                        //#
                        //#   1-minute averaged Real-time Bulk Parameters of the Solar Wind Plasma
                        //# 
                        //#                Modified Seconds   -------------  Solar Wind  -----------
                        //# UT Date   Time  Julian  of the          Proton      Bulk         Ion
                        //# YR MO DA  HHMM    Day     Day     S    Density     Speed     Temperature
                        //#-------------------------------------------------------------------------
                        //2010 09 24  0000   55463       0    0        0.5      455.7     1.37e+05
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        if (0 == line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vpd = line.Substring(37, 11).Trim();
                        string vps = line.Substring(48, 11).Trim();
                        string vpt = line.Substring(59, 13).Trim();
                        double dpd;
                        if (vpd.StartsWith(missing) || vpd.StartsWith(missing2) || vpd.StartsWith(missing3))
                            dpd = double.NaN;
                        else
                            dpd = double.Parse(vpd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dps;
                        if (vps.StartsWith(missing) || vps.StartsWith(missing2) || vps.StartsWith(missing3))
                            dps = double.NaN;
                        else
                            dps = double.Parse(vps, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dpt;
                        if (vpt.StartsWith(missing) || vpt.StartsWith(missing2) || vpt.StartsWith(missing3))
                            dpt = double.NaN;
                        else
                            dpt = double.Parse(vpt, NumberStyles.Any, CultureInfo.InvariantCulture);
                        pdList.Add(dt, dpd);
                        psList.Add(dt, dps);
                        ptList.Add(dt, dpt);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}]", dt.ToShortDateString(), dpd, dps, dpt));

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
        private static void FetchSis(string url, SortedList<DateTime, double> p10List, SortedList<DateTime, double> p30List, string referer, List<string> echoList)
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
                    const string missing = "-1.00e+05";
                    const string missing2 = "-1.0e+05";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Data_list: 20100924_ace_sis_5m.txt
                        //:Created: 2010 Sep 25 0011 UT
                        //# Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //# Please send comments and suggestions to SWPC.Webmaster@noaa.gov 
                        //# 
                        //# Units: proton flux p/cs2-sec-ster
                        //# Status(S): 0 = nominal data, 1 to 8 = bad data record, 9 = no data
                        //# Missing data values: -1.00e+05
                        //# Source: ACE Satellite - Solar Isotope Spectrometer
                        //#
                        //# 5-minute averaged Real-time Integral Flux of High-energy Solar Protons
                        //# 
                        //#                 Modified Seconds
                        //# UT Date   Time   Julian  of the      ---- Integral Proton Flux ----
                        //# YR MO DA  HHMM     Day     Day       S    > 10 MeV    S    > 30 MeV
                        //#--------------------------------------------------------------------
                        //2010 09 24  0000    55463       0      0    1.84e+00    0    1.29e+00
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        int len = line.Length;
                        if (0 == len)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vp10 = line.Substring(40, 12).Trim();
                        string vp30 = line.Substring(57, len - 57/*12*/).Trim();
                        double dp10;
                        if (vp10.StartsWith(missing) || vp10.StartsWith(missing2))
                            dp10 = double.NaN;
                        else
                            dp10 = double.Parse(vp10, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp30;
                        if (vp30.StartsWith(missing) || vp30.StartsWith(missing2))
                            dp30 = double.NaN;
                        else
                            dp30 = double.Parse(vp30, NumberStyles.Any, CultureInfo.InvariantCulture);
                        p10List.Add(dt, dp10);
                        p30List.Add(dt, dp30);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] p10[{1}] p30[{2}]", dt.ToShortDateString(), dp10, dp30));

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
                int j = Properties.Settings.Default.DaysBackMag1m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace/{0}_ace_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchMag(url, ht_1MList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.MonthsBackMag1h + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/{0}_ace_mag_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchMag(url, ht_1HList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_mag_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                j = Properties.Settings.Default.DaysBackSwepam1m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace/{0}_ace_swepam_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchSwepam(url, pd_1MList, ps_1MList, pt_1MList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_swepam_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.MonthsBackSwepam1h + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/{0}_ace_swepam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchSwepam(url, pd_1HList, ps_1HList, pt_1HList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_swepam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                j = Properties.Settings.Default.DaysBackSis5m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace/{0}_ace_sis_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchSis(url, p10_5MList, p30_5MList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_sis_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.MonthsBackSis1h + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/{0}_ace_sis_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchSis(url, p10_1HList, p30_1HList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_sis_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                j = Properties.Settings.Default.DaysBackEpam5m + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace/{0}_ace_epam_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_epam_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                j = Properties.Settings.Default.MonthsBackEpam1h + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/{0}_ace_epam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_epam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                j = Properties.Settings.Default.MonthsBackLoc1h + 1;
                for (int i = 0; i < j; i++)
                {
                    url = string.Format("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/{0}_ace_loc_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_loc_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                Fetch("ftp://ftp.swpc.noaa.gov/pub/lists/ace/README", "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                if (echoList.Count > 0)
                    SaveFile(string.Concat(dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "_readme.txt"), echoList);
                Fetch("ftp://ftp.swpc.noaa.gov/pub/lists/ace2/README", "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                if (echoList.Count > 0)
                    SaveFile(string.Concat(dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "_readme2.txt"), echoList);

                if (!Properties.Settings.Default.DownloadOnly1mHt)
                {
                    Trace.TraceInformation("Updating Ht 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in ht_1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ht_1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hHt)
                {
                    Trace.TraceInformation("Updating Ht 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in ht_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ht_1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1mPd)
                {
                    Trace.TraceInformation("Updating Pd 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in pd_1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd_1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hPd)
                {
                    Trace.TraceInformation("Updating Pd 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in pd_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd_1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1mPs)
                {
                    Trace.TraceInformation("Updating Ps 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in ps_1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps_1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hPs)
                {
                    Trace.TraceInformation("Updating Ps 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in ps_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps_1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1mPt)
                {
                    Trace.TraceInformation("Updating Pt 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in pt_1MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pt_1MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hPt)
                {
                    Trace.TraceInformation("Updating Pt 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in pt_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pt_1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly5mP10)
                {
                    Trace.TraceInformation("Updating P10 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.P10InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in p10_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p10_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hP10)
                {
                    Trace.TraceInformation("Updating P10 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.P10InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in p10_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p10_1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly5mP30)
                {
                    Trace.TraceInformation("Updating P30 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.P30InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in p30_5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p30_5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnly1hP30)
                {
                    Trace.TraceInformation("Updating P30 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.P30InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in p30_1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p30_1HList[r];
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
