using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using System.Xml;

using mbdt.Utils;
using Mbh5;

namespace AscAceUpdate
{
    class Program
    {
        static private SortedList<DateTime, double> ht_1mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> ht_1hList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> pd_1mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> pd_1hList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> ps_1mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> ps_1hList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> pt_1mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> pt_1hList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> p10_5mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> p10_1hList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> p30_5mList = new SortedList<DateTime, double>(1024);
        static private SortedList<DateTime, double> p30_1hList = new SortedList<DateTime, double>(1024);
        static private string folder, downloadFolder;

        private static void SaveFile(string fileName, List<string> echoList)
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

        private static void FetchMag(string url, SortedList<DateTime, double> htList, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    DateTime dt;
                    string vh, line, missing = "-999.9";
                    double dh;
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
                        dt = StringToDateTime(line);
                        vh = line.Substring(61, 8).Trim();
                        if (vh.StartsWith(missing))
                            dh = double.NaN;
                        else
                        {
                            dh = double.Parse(vh, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                            htList.Add(dt, dh);
                        }
                        w++;
                        Debug.WriteLine(string.Format("<[{0}][{1}]", dt.ToShortDateString(), dh));
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
        }

        private static void FetchSwepam(string url, SortedList<DateTime, double> pdList, SortedList<DateTime, double> psList, SortedList<DateTime, double> ptList, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    DateTime dt;
                    string vpd, vps, vpt, line, missing2 = "-9999.9", missing = "-1.00e+05", missing3 = "-1.0e+05";
                    double dpd, dps, dpt;
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
                        dt = StringToDateTime(line);
                        vpd = line.Substring(37, 11).Trim();
                        vps = line.Substring(48, 11).Trim();
                        vpt = line.Substring(59, 13).Trim();
                        if (vpd.StartsWith(missing) || vpd.StartsWith(missing2) || vpd.StartsWith(missing3))
                            dpd = double.NaN;
                        else
                            dpd = double.Parse(vpd, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        if (vps.StartsWith(missing) || vps.StartsWith(missing2) || vps.StartsWith(missing3))
                            dps = double.NaN;
                        else
                            dps = double.Parse(vps, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        if (vpt.StartsWith(missing) || vpt.StartsWith(missing2) || vpt.StartsWith(missing3))
                            dpt = double.NaN;
                        else
                            dpt = double.Parse(vpt, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        pdList.Add(dt, dpd);
                        psList.Add(dt, dps);
                        ptList.Add(dt, dpt);
                        w++;
                        Debug.WriteLine(string.Format("<[{0}][{1}][{2}][{3}]", dt.ToShortDateString(), dpd, dps, dpt));
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
        }

        private static void FetchSis(string url, SortedList<DateTime, double> p10List, SortedList<DateTime, double> p30List, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    DateTime dt;
                    string vp10, vp30, line, missing = "-1.00e+05", missing2 = "-1.0e+05";
                    double dp10, dp30;
                    int len;
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
                        len = line.Length;
                        if (0 == len)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        dt = StringToDateTime(line);
                        vp10 = line.Substring(40, 12).Trim();
                        vp30 = line.Substring(57, len - 57/*12*/).Trim();
                        if (vp10.StartsWith(missing) || vp10.StartsWith(missing2))
                            dp10 = double.NaN;
                        else
                            dp10 = double.Parse(vp10, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        if (vp30.StartsWith(missing) || vp30.StartsWith(missing2))
                            dp30 = double.NaN;
                        else
                            dp30 = double.Parse(vp30, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        p10List.Add(dt, dp10);
                        p30List.Add(dt, dp30);
                        w++;
                        Debug.WriteLine(string.Format("<[{0}] p10[{1}] p30[{2}]", dt.ToShortDateString(), dp10, dp30));
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
        }

        private static void Fetch(string url, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    string line;
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        w++;
                    }
                }
                Trace.TraceInformation("Download complete: [{0}] rows fetched", w);
            }
            catch (WebException ex)
            {
                echoList.Clear();
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
        }

        static void Main(string[] args)
        {
            Repository repository = null;
            ScalarData scalarData = null;
            Scalar scalar = new Scalar();
            List<Scalar> scalarList = new List<Scalar>();
            List<string> echoList = new List<string>(4096);
            Instrument instrument = null;
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            DateTime d, dt = DateTime.Now;
            Trace.TraceInformation("Started: {0}", dt);
            try
            {
                folder = dt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                downloadFolder = string.Concat(Properties.Settings.Default.DownloadFolder, folder);
                if (!Directory.Exists(downloadFolder))
                    Directory.CreateDirectory(downloadFolder);
                string url, h5file = Properties.Settings.Default.RepositoryFile;
                FileInfo fileInfo = new FileInfo(h5file);
                if (!Directory.Exists(fileInfo.DirectoryName))
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                repository = Repository.OpenReadWrite(h5file, true, Properties.Settings.Default.Hdf5CorkTheCache);

                d = dt;
                for (int i = 0, j = Properties.Settings.Default.DaysBackMag1m + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace/{0}_ace_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchMag(url, ht_1mList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_mag_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                for (int i = 0, j = Properties.Settings.Default.MonthsBackMag1h + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace2/{0}_ace_mag_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchMag(url, ht_1hList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_mag_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                for (int i = 0, j = Properties.Settings.Default.DaysBackSwepam1m + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace/{0}_ace_swepam_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchSwepam(url, pd_1mList, ps_1mList, pt_1mList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_swepam_1m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                for (int i = 0, j = Properties.Settings.Default.MonthsBackSwepam1h + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace2/{0}_ace_swepam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchSwepam(url, pd_1hList, ps_1hList, pt_1hList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_swepam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                for (int i = 0, j = Properties.Settings.Default.DaysBackSis5m + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace/{0}_ace_sis_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    FetchSis(url, p10_5mList, p30_5mList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_sis_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                for (int i = 0, j = Properties.Settings.Default.MonthsBackSis1h + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace2/{0}_ace_sis_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    FetchSis(url, p10_1hList, p30_1hList, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_sis_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                for (int i = 0, j = Properties.Settings.Default.DaysBackEpam5m + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace/{0}_ace_epam_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                    url = string.Format("{0}_ace_epam_5m.txt", d.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddDays(-1);
                }
                d = dt;
                for (int i = 0, j = Properties.Settings.Default.MonthsBackEpam1h + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace2/{0}_ace_epam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_epam_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                d = dt;
                for (int i = 0, j = Properties.Settings.Default.MonthsBackLoc1h + 1; i < j; i++)
                {
                    url = string.Format("http://www.swpc.noaa.gov/ftpdir/lists/ace2/{0}_ace_loc_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    Fetch(url, "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                    url = string.Format("{0}_ace_loc_1h.txt", d.ToString("yyyyMM", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    d = d.AddMonths(-1);
                }

                Fetch("http://www.swpc.noaa.gov/ftpdir/lists/ace/README", "http://www.swpc.noaa.gov/ftpmenu/lists/ace.html", echoList);
                if (echoList.Count > 0)
                    SaveFile(string.Concat(dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "_readme.txt"), echoList);
                Fetch("http://www.swpc.noaa.gov/ftpdir/lists/ace2/README", "http://www.swpc.noaa.gov/ftpmenu/lists/ace2.html", echoList);
                if (echoList.Count > 0)
                    SaveFile(string.Concat(dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "_readme2.txt"), echoList);

                if (!Properties.Settings.Default.DownloadOnly1mHt)
                {
                    Trace.TraceInformation("Updating Ht 1m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                    scalarList.Clear();
                    foreach (var r in ht_1mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ht_1mList[r];
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
                    foreach (var r in ht_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ht_1hList[r];
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
                    foreach (var r in pd_1mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd_1mList[r];
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
                    foreach (var r in pd_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd_1hList[r];
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
                    foreach (var r in ps_1mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps_1mList[r];
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
                    foreach (var r in ps_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps_1hList[r];
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
                    foreach (var r in pt_1mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pt_1mList[r];
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
                    foreach (var r in pt_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pt_1hList[r];
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
                    foreach (var r in p10_5mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p10_5mList[r];
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
                    foreach (var r in p10_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p10_1hList[r];
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
                    foreach (var r in p30_5mList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p30_5mList[r];
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
                    foreach (var r in p30_1hList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = p30_1hList[r];
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
