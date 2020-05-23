using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace SwpcDxdUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> rfList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ssnList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ssaList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> xfList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> apList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> kpList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pf1List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pf10List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pf100List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ef8List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ef20List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ncList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> nmList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> nxList = new SortedList<DateTime, double>(1024);
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
            return new DateTime(year, month, day, 0, 0, 0);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void FetchDgd(string url, SortedList<DateTime, double> aList, SortedList<DateTime, double> kList, string referer, List<string> echoList)
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
                    const string missing = "-1";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Product: Daily Geomagnetic Data          DGD.txt
                        //:Issued: 0630 UT 26 Oct 2010
                        //#
                        //#  Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //#  Please send comment and suggestions to SWPC.Webmaster@noaa.gov
                        //#
                        //#               Last 30 Days Daily Geomagnetic Data
                        //#
                        //#
                        //#                Middle Latitude        High Latitude            Estimated
                        //#              - Fredericksburg -     ---- College ----      --- Planetary ---
                        //#  Date        A     K-indices        A     K-indices        A     K-indices
                        //2010 09 27     5  2 0 1 1 2 2 2 2     5  1 1 1 0 2 3 2 1     6  1 0 0 0 2 2 3 2
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        if (0 == line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string va = line.Substring(60, 3).Trim();
                        double da;
                        if (va.StartsWith(missing))
                            da = double.NaN;
                        else
                        {
                            da = double.Parse(va, NumberStyles.Any, CultureInfo.InvariantCulture);
                            aList.Add(dt, da);
                        }
                        string vk1 = line.Substring(63, 2).Trim();
                        double dk1;
                        if (vk1.StartsWith(missing))
                            dk1 = double.NaN;
                        else
                        {
                            dk1 = double.Parse(vk1, NumberStyles.Any, CultureInfo.InvariantCulture);
                            //t = dt.AddHours(0);
                            kList.Add(dt, dk1);
                        }
                        string vk2 = line.Substring(65, 2).Trim();
                        DateTime t;
                        double dk2;
                        if (vk2.StartsWith(missing))
                            dk2 = double.NaN;
                        else
                        {
                            dk2 = double.Parse(vk2, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(3);
                            kList.Add(t, dk2);
                        }
                        string vk3 = line.Substring(67, 2).Trim();
                        double dk3;
                        if (vk3.StartsWith(missing))
                            dk3 = double.NaN;
                        else
                        {
                            dk3 = double.Parse(vk3, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(6);
                            kList.Add(t, dk3);
                        }
                        string vk4 = line.Substring(69, 2).Trim();
                        double dk4;
                        if (vk4.StartsWith(missing))
                            dk4 = double.NaN;
                        else
                        {
                            dk4 = double.Parse(vk4, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(9);
                            kList.Add(t, dk4);
                        }
                        string vk5 = line.Substring(71, 2).Trim();
                        double dk5;
                        if (vk5.StartsWith(missing))
                            dk5 = double.NaN;
                        else
                        {
                            dk5 = double.Parse(vk5, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(12);
                            kList.Add(t, dk5);
                        }
                        string vk6 = line.Substring(73, 2).Trim();
                        double dk6;
                        if (vk6.StartsWith(missing))
                            dk6 = double.NaN;
                        else
                        {
                            dk6 = double.Parse(vk6, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(15);
                            kList.Add(t, dk6);
                        }
                        string vk7 = line.Substring(75, 2).Trim();
                        double dk7;
                        if (vk7.StartsWith(missing))
                            dk7 = double.NaN;
                        else
                        {
                            dk7 = double.Parse(vk7, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(18);
                            kList.Add(t, dk7);
                        }
                        string vk8 = line.Substring(77, 2).Trim();
                        double dk8;
                        if (vk8.StartsWith(missing))
                            dk8 = double.NaN;
                        else
                        {
                            dk8 = double.Parse(vk8, NumberStyles.Any, CultureInfo.InvariantCulture);
                            t = dt.AddHours(21);
                            kList.Add(t, dk8);
                        }
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] a[{1}] k1[{2}] k2[{3}] k3[{4}] k4[{5}] k5[{6}] k6[{7}] k7[{8}] k8[{9}]", dt.ToShortDateString(), da, dk1, dk2, dk3, dk4, dk5, dk6, dk7, dk8));

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
        private static void FetchDsd(string url, SortedList<DateTime, double> rList, SortedList<DateTime, double> nList, SortedList<DateTime, double> aList, SortedList<DateTime, double> xList, SortedList<DateTime, double> n1List, SortedList<DateTime, double> n2List, SortedList<DateTime, double> n3List, string referer, List<string> echoList)
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
                    const string missingX = "*";
                    const string missing = "-1";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //:Product: Daily Solar Data            DSD.txt
                        //:Issued: 0825 UT 26 Oct 2010
                        //#
                        //#  Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //#  Please send comments and suggestions to SWPC.Webmaster@noaa.gov
                        //#
                        //#                Last 30 Days Daily Solar Data
                        //#
                        //#                         Sunspot       Stanford GOES14
                        //#           Radio  SESC     Area          Solar  X-Ray  ------ Flares ------
                        //#           Flux  Sunspot  10E-6   New     Mean  Bkgd    X-Ray      Optical
                        //#  Date     10.7cm Number  Hemis. Regions Field  Flux   C  M  X  S  1  2  3
                        //#--------------------------------------------------------------------------- 
                        //2010 09 26   84     57      590      1    -999   B1.2   0  0  0  3  0  0  0
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        if (0 == line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vr = line.Substring(10, 5).Trim();
                        string vs = line.Substring(15, 7).Trim();
                        string va = line.Substring(22, 9).Trim();
                        string vx = line.Substring(46, 7).Trim();
                        string vnc = line.Substring(53, 4).Trim();
                        string vnm = line.Substring(57, 3).Trim();
                        string vnx = line.Substring(60, 3).Trim();
                        double dr = vr.StartsWith(missing) ? double.NaN : double.Parse(vr, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double ds = vs.StartsWith(missing) ? double.NaN : double.Parse(vs, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double da = va.StartsWith(missing) ? double.NaN : double.Parse(va, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dx;
                        if (vx.StartsWith(missingX))
                            dx = double.NaN;
                        else
                        {
                            dx = double.Parse(vx.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture);
                            switch (vx[0])
                            {
                                //case 'A':
                                //    dx *= 1;
                                //    break;
                                case 'B':
                                    dx *= 10;
                                    break;
                                case 'C':
                                    dx *= 100;
                                    break;
                                case 'M':
                                    dx *= 1000;
                                    break;
                                case 'X':
                                    dx *= 10000;
                                    break;
                                case 'Y':
                                    dx *= 100000;
                                    break;
                            }
                        }
                        double dnc;
                        if (vnc.StartsWith(missing) || vnc.StartsWith(missingX))
                            dnc = double.NaN;
                        else
                            dnc = double.Parse(vnc, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dnm;
                        if (vnm.StartsWith(missing) || vnm.StartsWith(missingX))
                            dnm = double.NaN;
                        else
                            dnm = double.Parse(vnm, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dnx;
                        if (vnx.StartsWith(missing) || vnx.StartsWith(missingX))
                            dnx = double.NaN;
                        else
                            dnx = double.Parse(vnx, NumberStyles.Any, CultureInfo.InvariantCulture);
                        rList.Add(dt, dr);
                        nList.Add(dt, ds);
                        aList.Add(dt, da);
                        xList.Add(dt, dx);
                        n1List.Add(dt, dnc);
                        n2List.Add(dt, dnm);
                        n3List.Add(dt, dnx);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}][{5}][{6}][{7}]", dt.ToShortDateString(), dr, ds, da, dx, dnc, dnm, dnx));

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
        private static void FetchDpd(string url, SortedList<DateTime, double> p1List, SortedList<DateTime, double> p10List, SortedList<DateTime, double> p100List, SortedList<DateTime, double> e8List, SortedList<DateTime, double> e20List, string referer, List<string> echoList)
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
                        //:Product: Daily Particle Data          DPD.txt
                        //:Issued: 0223 UT 26 Oct 2010
                        //#
                        //#  Prepared by the U.S. Dept. of Commerce, NOAA, Space Weather Prediction Center
                        //#  Please send comments and suggestions to SWPC.Webmaster@noaa.gov
                        //# Neutron Monitor % of bkgd ended 1 Jun. http://www.swpc.noaa.gov/Thule.html
                        //#                Last 30 Days Daily Particle Data
                        //#
                        //#              GOES-13 Proton Fluence     GOES-13 Electron Fluence     Neutron
                        //#            --- Protons/cm2-day-sr ---  -- Electrons/cm2-day-sr --    Monitor
                        //#  Date       >1 MeV  >10 MeV  >100 MeV  >0.8 MeV   >2 MeV           % of bkgd
                        //#-------------------------------------------------------------------------------
                        //2010 09 26    2.8e+05  1.4e+04  3.6e+03   2.9e+09    2.3e+07          -999.99
                        if (line.StartsWith(":", StringComparison.Ordinal))
                            continue;
                        if (line.StartsWith("#", StringComparison.Ordinal))
                            continue;
                        if (0 == line.Length)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vp1 = line.Substring(12, 9).Trim();
                        string vp10 = line.Substring(21, 9).Trim();
                        string vp100 = line.Substring(30, 9).Trim();
                        string ve8 = line.Substring(40, 9).Trim();
                        string ve20 = line.Substring(51, 10).Trim();
                        double dp1;
                        if (vp1.StartsWith(missing) || vp1.StartsWith(missing2))
                            dp1 = double.NaN;
                        else
                            dp1 = double.Parse(vp1, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp10;
                        if (vp10.StartsWith(missing) || vp10.StartsWith(missing2))
                            dp10 = double.NaN;
                        else
                            dp10 = double.Parse(vp10, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dp100;
                        if (vp100.StartsWith(missing) || vp100.StartsWith(missing2))
                            dp100 = double.NaN;
                        else
                            dp100 = double.Parse(vp100, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de8;
                        if (ve8.StartsWith(missing) || ve8.StartsWith(missing2))
                            de8 = double.NaN;
                        else
                            de8 = double.Parse(ve8, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de20;
                        if (ve20.StartsWith(missing) || ve20.StartsWith(missing2))
                            de20 = double.NaN;
                        else
                            de20 = double.Parse(ve20, NumberStyles.Any, CultureInfo.InvariantCulture);
                        p1List.Add(dt, dp1);
                        p10List.Add(dt, dp10);
                        p100List.Add(dt, dp100);
                        e8List.Add(dt, de8);
                        e20List.Add(dt, de20);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] p1[{1}] p10[{2}] p100[{3}] e8[{4}] e20[{5}]", dt.ToShortDateString(), dp1, dp10, dp100, de8, de20));

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
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                //"ftp://ftp.swpc.noaa.gov/pub/indices/"
                FetchDgd("ftp://ftp.swpc.noaa.gov/pub/indices/DGD.txt", apList, kpList, "http://www.swpc.noaa.gov/Data/index.html", echoList);
                string url = string.Format("{0}_DGD.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                if (echoList.Count > 0)
                    SaveFile(url, echoList);

                FetchDsd("ftp://ftp.swpc.noaa.gov/pub/indices/DSD.txt", rfList, ssnList, ssaList, xfList, ncList, nmList, nxList, "http://www.swpc.noaa.gov/Data/index.html", echoList);
                url = string.Format("{0}_DSD.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                if (echoList.Count > 0)
                    SaveFile(url, echoList);

                FetchDpd("ftp://ftp.swpc.noaa.gov/pub/indices/DPD.txt", pf1List, pf10List, pf100List, ef8List, ef20List, "http://www.swpc.noaa.gov/Data/index.html", echoList);
                url = string.Format("{0}_DPD.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                if (echoList.Count > 0)
                    SaveFile(url, echoList);

                Fetch("ftp://ftp.swpc.noaa.gov/pub/indices/README", "http://www.swpc.noaa.gov/Data/index.html", echoList);
                if (echoList.Count > 0)
                    SaveFile(string.Concat(dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture), "_readme.txt"), echoList);

                if (!Properties.Settings.Default.DownloadOnlyRf)
                {
                    Trace.TraceInformation("Updating Rf: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.RfInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in rfList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = rfList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlySescSsn)
                {
                    Trace.TraceInformation("Updating SescSsn: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.SescSsnInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in ssnList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ssnList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlySsa)
                {
                    Trace.TraceInformation("Updating Ssa: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.SsaInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in ssaList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ssaList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyXf)
                {
                    Trace.TraceInformation("Updating Xf: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.XfInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in xfList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = xfList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyNc)
                {
                    Trace.TraceInformation("Updating Nc: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.NcInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in ncList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ncList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyNm)
                {
                    Trace.TraceInformation("Updating Nm: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.NmInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in nmList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = nmList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyNc)
                {
                    Trace.TraceInformation("Updating Nx: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.NxInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in nxList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = nxList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyAp)
                {
                    Trace.TraceInformation("Updating Ap: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.ApInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in apList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = apList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyKp)
                {
                    Trace.TraceInformation("Updating Kp: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.KpInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour3, true);
                    scalarList.Clear();
                    foreach (var r in kpList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = kpList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPf1)
                {
                    Trace.TraceInformation("Updating Pf1: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Pf1InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in pf1List.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pf1List[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPf10)
                {
                    Trace.TraceInformation("Updating Pf10: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Pf10InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in pf10List.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pf10List[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPf100)
                {
                    Trace.TraceInformation("Updating Pf100: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Pf100InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in pf100List.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pf100List[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyEf8)
                {
                    Trace.TraceInformation("Updating Ef8: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ef8InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in ef8List.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ef8List[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyEf20)
                {
                    Trace.TraceInformation("Updating Ef20: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.Ef20InstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in ef20List.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ef20List[r];
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
