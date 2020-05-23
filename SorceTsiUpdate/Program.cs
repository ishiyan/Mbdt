using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace SorceTsiUpdate
{
    static class Program
    {
        static private readonly SortedList<DateTime, double> tsi6HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> tsi1DList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> tsite6HList = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> tsite1DList = new SortedList<DateTime, double>(1024);
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

        private static DateTime StringToDateTime(string input, bool parseHours)
        {
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(4, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(6, 2), CultureInfo.InvariantCulture);
            int hour = 0;
            if (parseHours)
            {
                string s = input.Substring(8);
                if (s.StartsWith(".125"))
                    hour = 0;
                else if (s.StartsWith(".375"))
                    hour = 6;
                else if (s.StartsWith(".625"))
                    hour = 12;
                else if (s.StartsWith(".875"))
                    hour = 18;
            }
            return new DateTime(year, month, day, hour, 0, 0);
        }

        private static void FetchList(string url, SortedList<DateTime, double> tList, SortedList<DateTime, double> eList, string referer, List<string> echoList, bool parseHours)
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
                    const string missing = "0.0000";
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //; SORCE TIM Total Solar Irradiance
                        //; 
                        //; ***SELECTION CRITERIA***
                        //; data_set_name: SORCE Level 3 Total Solar Irradiance
                        //; date_range: 20030225 to 20101025
                        //; cadence:  6 hours
                        //; version:       10
                        //; number_of_data:        11200
                        //; ***END SELECTION CRITERIA***
                        //;
                        //; ***DATA DEFINITIONS***, number = 15 [field name, type, format, (Col. #, description)]
                        //; nominal_date_yyyymmdd                 R8   f12.3 (Column  1: Nominal Data Time, YYYYMMDD)
                        //; nominal_date_jdn                      R8   f12.3 (Column  2: Nominal Data Time, Julian Day Number)
                        //; avg_measurement_date_jdn              R8   f15.6 (Column  3: Average Data Time, Julian Day Number)
                        //; std_dev_measurement_date              R4    f7.4 (Column  4: Stdev of Average Data Time, days, 1 sigma)
                        //; tsi_1au                               R8   f10.4 (Column  5: Total Solar Irradiance (TSI) at 1-AU, W/m^2)
                        //; instrument_accuracy_1au               R4   e10.3 (Column  6: Instrument Accuracy in 1-AU TSI, W/m^2, 1 sigma)
                        //; instrument_precision_1au              R4   e10.3 (Column  7: Instrument Precision in TSI at 1-AU, W/m^2, 1 sigma)
                        //; solar_standard_deviation_1au          R4   e10.3 (Column  8: Solar Standard Deviation in 1-AU TSI, W/m^2, 1 sigma)
                        //; measurement_uncertainty_1au           R4   e10.3 (Column  9: Total Uncertainty in TSI at 1-AU, W/m^2, 1 sigma)
                        //; tsi_true_earth                        R8   f10.4 (Column 10: Total Solar Irradiance at Earth distance, W/m^2)
                        //; instrument_accuracy_true_earth        R4   e10.3 (Column 11: Instrument Accuracy at Earth distance, W/m^2, 1 sigma)
                        //; instrument_precision_true_earth       R4   e10.3 (Column 12: Instrument Precision at Earth distance, W/m^2, 1 sigma)
                        //; solar_standard_deviation_true_earth   R4   e10.3 (Column 13: Solar Standard Deviation in TSI at Earth, W/m^2, 1 sigma)
                        //; measurement_uncertainty_true_earth    R4   e10.3 (Column 14: Total Uncertainty in TSI at Earth distance, W/m^2, 1 sigma)
                        //; provisional_flag                      I2      i2 (Column 15: Provisional Flag, 1=provisional data, 0=final data)
                        //; ***END DATA DEFINITIONS***
                        //;
                        //; ***FORTRAN FORMAT SPECIFIER***
                        //; (f12.3,f12.3,f15.6,f7.4,f10.4,e10.3,e10.3,e10.3,e10.3,f10.4,e10.3,e10.3,e10.3,e10.3,i2)
                        //; ***END FORTRAN FORMAT SPECIFIER***
                        //;
                        //; 
                        //; Background for the SORCE TIM instrument and TSI measurements
                        //; 
                        //; The SORCE Total Irradiance Monitor (TIM) measures the total solar
                        //; irradiance (TSI), a measure of the absolute intensity of solar radiation
                        //; integrated over the entire solar disk and the entire solar spectrum.
                        //; The SORCE Level 3 TSI data products are the daily and 6-hourly mean
                        //; irradiances, reported at both a mean solar distance of 1 astronomical
                        //; unit (AU) and at the true Earth-to-Sun distance of date, and zero
                        //; relative line-of-sight velocity with respect to the Sun. These products
                        //; respectively indicate emitted solar radiation variability (useful for
                        //; solar studies) and the solar energy input to the top of the Earth's
                        //; atmosphere (useful for Earth climate studies).
                        //; 
                        //; The TIM instrument is proving very stable with usage and solar exposure,
                        //; its long-term repeatability having uncertainties estimated to be less
                        //; than 0.014 W/m^2/yr (10 ppm/yr). Accuracy is estimated to be 0.48 W/m^2
                        //; (350 ppm), largely determined by uncertainties in the instrument's
                        //; linearity. This uncertainty is consistent with the agreement between
                        //; all four TIM radiometers. There remains an unresolved 4.7 W/m^2 difference
                        //; between the TIM instrument and other space-borne radiometers, and this
                        //; difference is being studied by the TSI and radiometry communities. Recent
                        //; optical power tests at NIST do not indicate that the TIM instrument is
                        //; incorrect by this amount.  The following paragraphs discuss the four
                        //; different uncertainties reported with the TSI measurements.
                        //; 
                        //; INSTRUMENT UNCERTAINTY reflects the instrument's relative standard 
                        //; uncertainty (absolute accuracy) and includes all known uncertainties from
                        //; ground- and space-based calibrations plus a time-dependent estimate of
                        //; uncertainty due to degradation. This value is roughly 350 ppm, and varies
                        //; slightly with measured instrument temperature or the time to the nearest
                        //; on-orbit calibration. This value is useful when comparing different TSI
                        //; instruments reporting data from the same time range on an absolute scale.
                        //; 
                        //; INSTRUMENT PRECISION reflects the TIM's sensitivity to a change in signal,
                        //; and is useful for determining relative changes in the TIM TSI due purely
                        //; to the Sun over time scales of two months or less (so that degradation
                        //; uncertainty does not have significant effect). This value of 5 ppm is
                        //; constant, and indicates the instrument's noise level.
                        //; 
                        //; High-cadence Level 2 data are averaged (un-weighted mean) to produce daily
                        //; and 6-hourly averaged Level 3 data. The standard deviation of the Level 2
                        //; values averaged to produce each Level 3 value is indicative of the solar
                        //; variability during the reported Level 3 measurement interval, and is
                        //; called the SOLAR STANDARD DEVIATION. This uncertainty redundantly includes
                        //; -- but is generally much larger than -- the Instrument Precision. The Solar
                        //; Standard Deviation is useful for estimating potential variations in TSI
                        //; within the time range of a Level 3 data value, such as when comparing TIM
                        //; TSI values with solar images or other TSI instruments reporting data at
                        //; slightly different times.
                        //; 
                        //; MEASUREMENT UNCERTAINTY is the net uncertainty of a reported Level 3 data
                        //; value, and is the root sum square of Instrument Uncertainty and Solar
                        //; Standard Deviation. Measurement Uncertainty is the value that should be
                        //; used when comparing absolute scale TSI data from non-identical time ranges.
                        //; 
                        //; The 1-AU and at-Earth TIM irradiances are tabulated below ("DATA RECORDS"),
                        //; with each row giving the nominal and measurement dates, the reported
                        //; irradiances, and estimated uncertainties. Each field (column) is defined
                        //; and described in the "DATA DEFINITIONS" section above. An IDL file reader
                        //; (read_lasp_ascii_file.pro) is available which will read this file and
                        //; return an array of structures whose field names and types are taken from
                        //; the "DATA DEFINITIONS" section.
                        //; 
                        //; Details of the TIM design and calibrations are given in:
                        //; 
                        //; Kopp, G. and Lawrence, G., "The Total Irradiance Monitor (TIM): Instrument
                        //; Design," Solar Physics, 230, 1, Aug. 2005, pp. 91-109.
                        //; 
                        //; Kopp, G., Heuerman, K., and Lawrence, G., "The Total Irradiance Monitor
                        //; (TIM): Instrument Calibration," Solar Physics, 230, 1, Aug. 2005,
                        //; pp. 111-127.
                        //; 
                        //; This data file and other SORCE data products may be obtained from:
                        //; http://lasp.colorado.edu/home/sorce/data/data_product_summary.htm
                        //; 
                        //; For more information on the SORCE TIM instrument and data, see:
                        //; http://lasp.colorado.edu/home/sorce/data/
                        //; 
                        //; For news and general information about the SORCE mission, see:
                        //; http://lasp.colorado.edu/home/sorce
                        //;
                        //; Columns:
                        //;     1           2            3          4        5         6         7         8         9        10        11        12        13        14    15
                        //; ***DATA RECORDS***, number =     11200
                        //20030225.125 2452695.625 2452695.625000 0.0000    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00 0
                        //20030225.375 2452695.875 2452695.875000 0.0000    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00 0
                        //20030225.625 2452696.125 2452696.125000 0.0000    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00    0.0000 0.000e+00 0.000e+00 0.000e+00 0.000e+00 0
                        //20030225.875 2452696.375 2452696.409156 0.0031 1361.8745 4.767e-01 6.800e-03 3.566e-02 4.780e-01 1389.5926 4.864e-01 6.800e-03 3.680e-02 4.878e-01 0
                        if (line.StartsWith(";", StringComparison.Ordinal))
                            continue;
                        Debug.WriteLine(string.Format(">[{0}]", line));
                        DateTime dt = StringToDateTime(line, parseHours);
                        string vs = line.Substring(46, 10).Trim();
                        string ve = line.Substring(96, 10).Trim();
                        double ds = vs.StartsWith(missing) ? double.NaN : double.Parse(vs, NumberStyles.Any, CultureInfo.InvariantCulture);
                        double de = ve.StartsWith(missing) ? double.NaN : double.Parse(ve, NumberStyles.Any, CultureInfo.InvariantCulture);
                        tList.Add(dt, ds);
                        eList.Add(dt, de);
                        w++;
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), ds, de));
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
        /*
        private static void FetchFilenames(string url, out string file1D, out string file6H, string referer)
        {
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = referer;
            file1D = null;
            file6H = null;
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
                        if (null != file1D && null != file6H)
                            break;
                        int i = line.IndexOf("href=\"/tsi_data/daily/sorce_tsi_L3_c24h_", StringComparison.Ordinal);
                        string s;
                        if (0 < i)
                        {
                            Debug.WriteLine(string.Format(">[{0}]", line));
                            s = line.Substring(i + 9);
                            i = s.IndexOf(".txt\"", StringComparison.Ordinal);
                            if (0 > i)
                                Trace.TraceError("Failed to find [.txt\"] match in substring [{0}], line [{1}]", s, line);
                            else
                            {
                                s = s.Substring(0, i + 4);
                                file1D = s;
                                Debug.WriteLine(string.Format("<1d[{0}]", file1D));
                            }
                            continue;
                        }
                        i = line.IndexOf("href=\"/tsi_data/six_hourly/sorce_tsi_L3_c06h_", StringComparison.Ordinal);
                        if (0 < i)
                        {
                            Debug.WriteLine(string.Format(">[{0}]", line));
                            s = line.Substring(i + 9);
                            i = s.IndexOf(".txt\"", StringComparison.Ordinal);
                            if (0 > i)
                                Trace.TraceError("Failed to find [.txt\"] match in substring [{0}], line [{1}]", s, line);
                            else
                            {
                                s = s.Substring(0, i + 4);
                                file6H = s;
                                Debug.WriteLine(string.Format("<6h[{0}]", file6H));
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Trace.TraceError("Download failed: [{0}], uri=[{1}]", ex.Message, url);
            }
        }
        */
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
                string /*file1D, file6H,*/ url, h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                //FetchFilenames("http://lasp.colorado.edu/home/sorce/data/", out file1D, out file6H, "http://lasp.colorado.edu/home/sorce/index.htm");
                //if (null != file1D)
                {
                    url = "http://lasp.colorado.edu/data/sorce/tsi_data/daily/sorce_tsi_L3_c24h_latest.txt"; // string.Concat("http://lasp.colorado.edu/home/sorce/", file1D);
                    FetchList(url, tsi1DList, tsite1DList, "http://lasp.colorado.edu/home/sorce/data/", echoList, false);
                    url = string.Format("{0}_tsiFullMission_24h.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                }
                //if (null != file6H)
                {
                    url = "http://lasp.colorado.edu/data/sorce/tsi_data/six_hourly/sorce_tsi_L3_c06h_latest.txt"; //string.Concat("http://lasp.colorado.edu/home/sorce/", file6H);
                    FetchList(url, tsi6HList, tsite6HList, "http://lasp.colorado.edu/home/sorce/data/", echoList, true);
                    url = string.Format("{0}_tsiFullMission_06h.txt", dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                }

                if (!Properties.Settings.Default.DownloadOnlyTsi6h)
                {
                    Trace.TraceInformation("Updating TsiAU 6h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.TsiInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour6, true);
                    scalarList.Clear();
                    foreach (var r in tsi6HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = tsi6HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyTsi1d)
                {
                    Trace.TraceInformation("Updating TsiAU 1d: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.TsiInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in tsi1DList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = tsi1DList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyTsiTe6h)
                {
                    Trace.TraceInformation("Updating TsiTE 6h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.TsiTEInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour6, true);
                    scalarList.Clear();
                    foreach (var r in tsite6HList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = tsite6HList[r];
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                    scalarData.Flush();
                    scalarData.Close();
                    instrument.Close();
                }
                if (!Properties.Settings.Default.DownloadOnlyTsiTe1d)
                {
                    Trace.TraceInformation("Updating TsiTE 1d: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.TsiTEInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    scalarList.Clear();
                    foreach (var r in tsite1DList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = tsite1DList[r];
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
