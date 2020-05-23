using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace SohoUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> ps1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> ps5MList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pd1HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> pd5MList = new SortedList<DateTime, double>(1024);
        static private readonly List<string> htmlList = new List<string>(128);
        static private readonly List<string> min5List = new List<string>(128);
        static private readonly List<string> downList = new List<string>(128);
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
            //YR DOY:HR:MN:S    Vsw    Dens    Vth   ANGLE V_He   N/S   Sig  Vsw_f  Vsw_m  Dens_f Dens_m Vth_f  Vth_m Msw    Chi2 PM_Min  PM_Max  Vsw3  Vsw4   Vth3  Vth4 #Poss #ro
            //03 296:13:00:00   471.5    1.983   31.48   -2.32  482.   5.85   2.45   490.2   469.2    2.34    1.80   27.41   31.69  28.9    3.70     85.   14591.   465.5  471.2   16.3   32.8   11.  11
            int year = int.Parse(input.Substring(0, 2), CultureInfo.InvariantCulture);
            if (year > 70)
                year += 1900;
            else
                year += 2000;
            var dt = new DateTime(year, 1, 1, 0, 0, 0);
            int doy = int.Parse(input.Substring(3, 3), CultureInfo.InvariantCulture);
            dt = dt.AddDays(doy);
            int hour = int.Parse(input.Substring(7, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(10, 2), CultureInfo.InvariantCulture);
            int second = int.Parse(input.Substring(13, 2), CultureInfo.InvariantCulture);
            dt = dt.AddHours(hour);
            dt = dt.AddMinutes(minute);
            dt = dt.AddSeconds(second);
            return dt;
        }

        private static DateTime StringToDateTime2(string input)
        {
            // YY MON DY DOY:HH:MM:SS   SPEED     Np     Vth    N/S      GSE_X  GSE_Y  GSE_Z  RANGE  HGLAT  HGLONG
            // 96 JAN 20  20:18:00:00     434   10.2      37    3.2      211.2 -100.1  -11.0  145.9   -5.1   310.9
            int year = int.Parse(input.Substring(1, 2), CultureInfo.InvariantCulture);
            if (year > 70)
                year += 1900;
            else
                year += 2000;
            string m = input.Substring(4, 3);
            int month;
            if (m.StartsWith("JAN"))
                month = 1;
            else if (m.StartsWith("FEB"))
                month = 2;
            else if (m.StartsWith("MAR"))
                month = 3;
            else if (m.StartsWith("APR"))
                month = 4;
            else if (m.StartsWith("MAY"))
                month = 5;
            else if (m.StartsWith("JUN"))
                month = 6;
            else if (m.StartsWith("JUL"))
                month = 7;
            else if (m.StartsWith("AUG"))
                month = 8;
            else if (m.StartsWith("SEP"))
                month = 9;
            else if (m.StartsWith("OCT"))
                month = 10;
            else if (m.StartsWith("NOV"))
                month = 11;
            else //if (m.StartsWith("DEC"))
                month = 12;
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(input.Substring(15, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(18, 2), CultureInfo.InvariantCulture);
            int second = int.Parse(input.Substring(21, 2), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, hour, minute, second);
        }

        private static void FetchFileList(string url, List<string> mList, List<string> hList, List<string> dList, string referer)
        {
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
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
                    const string pat = "<TD><A HREF=\"archive/CRN";
                    const int patLen = 21; //<TD><A HREF="archive/
                    while (null != (line = streamReader.ReadLine()))
                    {
                        //<HEAD><TITLE>MTOF/PM Data by Carrington Rotation</TITLE></HEAD>
                        //<BODY>
                        //<H1><CENTER>MTOF/PM Data by Carrington Rotation</H1>
                        //<BR>
                        //Note: the Start and Stop times below were derived using <A HREF="CARRTIME.HTML">this technique.</A> 
                        //<BR>
                        //<BR>
                        //<TABLE BORDER=4 WIDTH=450>
                        //<TR>
                        //  <TH>Car Rot</TH>
                        //  <TH COLSPAN=2>Start Time</TH>
                        //
                        //  <TH COLSPAN=2>Stop Time</TH>
                        //  <TH COLSPAN=2>Links</TH>
                        //</TR>
                        //<TR  ALIGN=CENTER>
                        // <TD>2103</TD>
                        // <TD>2010 Oct 30</TD>
                        // <TD>1006</TD>
                        //
                        // <TD>2010 Nov 26</TD>
                        // <TD>1728</TD>
                        // <TD><A HREF="CRN_2103_A1.GIF">Plot</A></TD>
                        // <TD><A HREF="archive/CRN_2103_A1.HTML">List</A></TD>
                        //</TR>
                        //<TR  ALIGN=CENTER>
                        int index = line.IndexOf(pat, StringComparison.Ordinal);
                        if (0 > index)
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        string s = line.Substring(index + patLen);
                        index = s.IndexOf("\">", StringComparison.Ordinal);
                        if (0 > index)
                        {
                            Trace.TraceError("Invalid line [{0}]: cannot find pattern [{1}] in [{2}], skipping", line, "\">", s);
                            continue;
                        }
                        s = s.Substring(0, index);
                        Debug.WriteLine(string.Format("<[{0}]", s));
                        hList.Add(s);
                        s = s.Replace(".HTML", ".5MIN");
                        Debug.WriteLine(string.Format("<[{0}]", s));
                        mList.Add(s);
                        s = s.Replace(".5MIN", ".1HR");
                        Debug.WriteLine(string.Format("<[{0}]", s));
                        dList.Add(s);
                        s = s.Replace(".1HR", ".2HR");
                        Debug.WriteLine(string.Format("<[{0}]", s));
                        dList.Add(s);
                        w++;
                        if (w >= Properties.Settings.Default.EntriesBack)
                            break;

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
                Trace.TraceInformation("Download complete: [{0}] entries fetched", w);
            }
            catch (WebException ex)
            {
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception: [{0}], uri=[{1}]", ex.Message, url);
            }
        }

        private static void Fetch5Min(string url, SortedList<DateTime, double> sList, SortedList<DateTime, double> dList, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
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
                    const string missing = "-";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        // PROGRAM NAME:PMSW V7.27   RUN TIME:05:00:48 18-DEC-2003    INPUT FILE:LZ:SO_LZ_G029_20031023_V01.DAT1
                        // AVERAGED OVER 10 READOUTS                                 OUTPUT FILE:DISKM1:[SOHO]PMSW_CRN.5MIN
                        //
                        // FILE HEADER:
                        //   SC_ID           =             21
                        //   FIRST_EPOCH     = 1445558403.652
                        //   FIRST_TIME      =  2003  296   0   0   4.651854      2003  10  23   0   0   4.651854
                        //   LAST_TIME       =  2003  297   0   0   0.592772      2003  10  24   0   0   0.592772
                        //   NUM_PACKETS     =          46017
                        //   RECEIPT_EPOCH   = 1445709728.000
                        //   RECEIPT_TIME    =  2003  297  18   2   8.  0  0      2003  10  24  18   2   8.  0  0
                        // NPARAMS=22
                        // FORMAT=(I2.2,1X,I3,1H:,I2.2,1H:,I2.2,1H:,I2.2,F7.1,F8.3,F7.2,F7.2,F5.0,2F6.2,2F7.1,2F7.2,2F7.2,F5.1,  F7.2,F7.0,F8.0,F7.1,F6.1,2F6.1,F5.0)
                        //Vsw3 = Chi sq averaged Vsw, but without speed correction
                        //Vsw4 = Uncorrected Vsw from moment technique
                        //Vth3 = Chi sq averaged Vth, but without speed correction or power law Vth correction
                        //Vth4 = Uncorrected Vth from moment technique
                        //Chi2 = Sum of Chi**2 for all 6 rates; df=5; PCHI5 used for Chi2<20;  P(.5)=.99  P(4.4)=.5  P(15)=.01  P(20)=.001
                        //ANGLE = arrival direction in degrees (generally N/S, with + meaning FROM the south)
                        //V_He = predicted He speed based on Vsw, Dens, and Vth
                        //N/S = average theta bin after excluding bins 0 and 19
                        //PM_Min and PM_Max are corrected for quantization error but not for deadtime
                        //PM_Max is the average of maxima, not the maximum of averaged rates. PM_Min is the average of minima.
                        //Background is PMMIN_DTQ - SIGMA, where SIGMA =  quad. sum of quant. step and PMMIN/#readouts
                        //Minimum background-subtracted rate is SIGMA
                        //For dippers, uses average of 90% and original, average of 60% and original, ...
                        //Assumes that He/H ratio is 0.04
                        //Times indicated are start times in UTC
                        //
                        //YR DOY:HR:MN:S    Vsw    Dens    Vth   ANGLE V_He   N/S   Sig  Vsw_f  Vsw_m  Dens_f Dens_m Vth_f  Vth_m Msw    Chi2 PM_Min  PM_Max  Vsw3  Vsw4   Vth3  Vth4 #Poss
                        //03 296:13:03:04  470.4   2.126  30.93  -2.78  481  5.59  2.37  490.6  467.9   2.50   1.94  27.44  31.15 28.9   3.24     85   15660  465.7 470.2  16.4  32.1   13
                        if (!line.StartsWith("0", StringComparison.Ordinal) && !line.StartsWith("1", StringComparison.Ordinal) && !line.StartsWith("9", StringComparison.Ordinal) &&
                            !line.StartsWith("2", StringComparison.Ordinal) && !line.StartsWith("3", StringComparison.Ordinal) && !line.StartsWith("4", StringComparison.Ordinal))
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line);
                        string vv = line.Substring(15, 7).Trim();
                        string vd = line.Substring(22, 8).Trim();
                        double dv = vv.StartsWith(missing) ? double.NaN : double.Parse(vv, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dd = vd.StartsWith(missing) ? double.NaN : double.Parse(vd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        if (sList.ContainsKey(dt))
                        {
                            if (double.IsNaN(sList[dt]))
                            {
                                if (!double.IsNaN(dv))
                                    sList[dt] = dv;
                            }
                            else if (double.IsNaN(dv))
                            {
                            }
                            else if (Math.Abs(dv - sList[dt]) > double.Epsilon)
                            {
                                if (Properties.Settings.Default.UpdateExisting)
                                {
                                    Trace.TraceInformation("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, sList[dt], dv);
                                    sList[dt] = dv;
                                }
                                else
                                    Trace.TraceError("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, sList[dt], dv);
                            }
                        }
                        else
                            sList.Add(dt, dv);
                        if (dList.ContainsKey(dt))
                        {
                            if (double.IsNaN(dList[dt]))
                            {
                                if (!double.IsNaN(dd))
                                    dList[dt] = dd;
                            }
                            else if (double.IsNaN(dd))
                            {
                            }
                            else if (Math.Abs(dd - dList[dt]) > double.Epsilon)
                            {
                                if (Properties.Settings.Default.UpdateExisting)
                                {
                                    Trace.TraceInformation("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, dList[dt], dd);
                                    dList[dt] = dd;
                                }
                                else
                                    Trace.TraceError("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, dList[dt], dd);
                            }
                        }
                        else
                            dList.Add(dt, dd);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), dv, dd));

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

        private static void FetchHtml(string url, SortedList<DateTime, double> sList, SortedList<DateTime, double> dList, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
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
                        //  <HTML>
                        // <TITLE>Carrington Rotation 1905 </TITLE>
                        // <BODY>
                        // <Center>
                        // <h2>
                        // SOHO CELIAS Proton Monitor <br> Solar Wind Parameters for Carrington Rotation 1905 (hour averages)
                        // </h2></Center>
                        // <HR>
                        // <H2> Listed Parameters: </h2>
                        // Measurement time (Year, Day of Year, Hour, Minute, Second)
                        // <BR>
                        // Proton speed [km/sec]
                        // <BR> Proton density [protons per cubic centimeter]
                        // <BR> Most probable proton thermal speed [km/sec]
                        // <BR> Arrival direction [degrees from north-south, with + meaning FROM the south).
                        // <BR> (SPEED=-1 means NOT AVAILABLE)
                        // <p>
                        // SOHO spacecraft coordinates: based on SOHO PRE orbit files
                        // <BR>                             GSE X, Y, Z [Earth Radii]   1 R<SUB>E</SUB> == 6378 km
                        // <BR>                             Heliocentric Range [10<SUP>6</SUP> km],
                        // <BR>                             Heliographic Latitude and Longitude [deg].
                        // <P>
                        // FORMAT=(1X,I2,1X,I3,1X,I2,1X,I2,1X,I2,F7.1,3F7.2,3X,3F7.1,F7.1,F7.2,F8.2)
                        // <HR>
                        // <pre><font size=1>
                        //                              CELIAS/MTOF/PM DATA                      SOHO ORBIT DATA              
                        // InSitu_Start__Time       [km/s]  [cm-3] [km/s]  [deg]      [Re]   [Re]   [Re]  [Mkm]  [deg]  [deg]
                        // YY MON DY DOY:HH:MM:SS   SPEED     Np     Vth    N/S      GSE_X  GSE_Y  GSE_Z  RANGE  HGLAT  HGLONG
                        // 96 JAN 20  20:18:00:00     434   10.2      37    3.2      211.2 -100.1  -11.0  145.9   -5.1   310.9
                        if (!line.StartsWith(" 0", StringComparison.Ordinal) && !line.StartsWith(" 1", StringComparison.Ordinal) && !line.StartsWith(" 9", StringComparison.Ordinal) &&
                            !line.StartsWith(" 2", StringComparison.Ordinal) && !line.StartsWith(" 3", StringComparison.Ordinal) && !line.StartsWith(" 4", StringComparison.Ordinal))
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime2(line);
                        string vv = line.Substring(23, 8).Trim();
                        string vd = line.Substring(31, 7).Trim();
                        double dv = vv.StartsWith(missing) ? double.NaN : double.Parse(vv, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dd = vd.StartsWith(missing) ? double.NaN : double.Parse(vd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        if (sList.ContainsKey(dt))
                        {
                            if (double.IsNaN(sList[dt]))
                            {
                                if (!double.IsNaN(dv))
                                    sList[dt] = dv;
                            }
                            else if (double.IsNaN(dv))
                            {
                            }
                            else if (Math.Abs(dv - sList[dt]) > double.Epsilon)
                            {
                                if (Properties.Settings.Default.UpdateExisting)
                                {
                                    Trace.TraceInformation("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, sList[dt], dv);
                                    sList[dt] = dv;
                                }
                                else
                                    Trace.TraceError("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, sList[dt], dv);
                            }
                        }
                        else
                            sList.Add(dt, dv);
                        if (dList.ContainsKey(dt))
                        {
                            if (double.IsNaN(dList[dt]))
                            {
                                if (!double.IsNaN(dd))
                                    dList[dt] = dd;
                            }
                            else if (double.IsNaN(dd))
                            {
                            }
                            else if (Math.Abs(dd - dList[dt]) > double.Epsilon)
                            {
                                if (Properties.Settings.Default.UpdateExisting)
                                {
                                    Trace.TraceInformation("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, dList[dt], dd);
                                    dList[dt] = dd;
                                }
                                else
                                    Trace.TraceError("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, dList[dt], dd);
                            }
                        }
                        else
                            dList.Add(dt, dd);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), dv, dd));

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

        private static void Fetch(string url, string referer, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
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
            var echoList = new List<string>(8192);
            Instrument instrument = null;
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            const string url0 = "http://umtof.umd.edu/pm/";
            const string url1 = "http://umtof.umd.edu/pm/crn/";
            const string url2 = "http://umtof.umd.edu/pm/crn/archive/";
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
                FetchFileList(url1, min5List, htmlList, downList, url0);
                foreach (var s in min5List)
                {
                    Fetch5Min(string.Concat(url2, s), ps5MList, pd5MList, url2, echoList);
                    if (echoList.Count > 0)
                        SaveFile(string.Concat(s, ".txt"), echoList);
                }
                foreach (var s in htmlList)
                {
                    FetchHtml(string.Concat(url2, s), ps1HList, pd1HList, url2, echoList);
                    if (echoList.Count > 0)
                        SaveFile(string.Concat(s, ".txt"), echoList);
                }
                foreach (var s in downList)
                {
                    Fetch(string.Concat(url2, s), url2, echoList);
                    if (echoList.Count > 0)
                        SaveFile(string.Concat(s, ".txt"), echoList);
                }

                if (!Properties.Settings.Default.DownloadOnlyPs5m)
                {
                    Trace.TraceInformation("Updating Ps 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in ps5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPd5m)
                {
                    Trace.TraceInformation("Updating Pd 5m: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                    scalarList.Clear();
                    foreach (var r in pd5MList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd5MList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPs1h)
                {
                    Trace.TraceInformation("Updating Ps 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in ps1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = ps1HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyPd1h)
                {
                    Trace.TraceInformation("Updating Pd 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    scalarList.Clear();
                    foreach (var r in pd1HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = pd1HList[r];
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
