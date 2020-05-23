using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace SwpcAceConvert
{
    static class Program
    {
        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                    action(entry);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
                action(root);
        }

        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();
        static private readonly SortedList<DateTime, double> l1List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> l2List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> l3List = new SortedList<DateTime, double>(1024);

        private static DateTime StringToDateTime(string input)
        {
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(input.Substring(12, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(14, 2), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, hour, minute, 0);
        }

        private static void CollectMag(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-999.9";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                        l1List.Add(dt, dh);
                    }
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}]", dt.ToShortDateString(), dh));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectSwepam(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing2 = "-9999.9";
            const string missing = "-1.00e+05";
            const string missing3 = "-1.0e+05";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, dpd);
                    l2List.Add(dt, dps);
                    l3List.Add(dt, dpt);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}]", dt.ToShortDateString(), dpd, dps, dpt));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectSis(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-1.00e+05";
            const string missing2 = "-1.0e+05";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, dp10);
                    l2List.Add(dt, dp30);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] p10[{1}] p30[{2}]", dt.ToShortDateString(), dp10, dp30));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private enum Modus
        {
            Mag1M, Mag1H, Swepam1M, Swepam1H, Sis5M, Sis1H
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("Arguments: [mag1m|mag1h|swepam1m|swepam1h|sis5m|sis1h] dir_or_file_name");
            else
            {
                Modus modus;
                if (args[0].StartsWith("mag1m"))
                    modus = Modus.Mag1M;
                else if (args[0].StartsWith("mag1h"))
                    modus = Modus.Mag1H;
                else if (args[0].StartsWith("swepam1m"))
                    modus = Modus.Swepam1M;
                else if (args[0].StartsWith("swepam1h"))
                    modus = Modus.Swepam1H;
                else if (args[0].StartsWith("sis5m"))
                    modus = Modus.Sis5M;
                else if (args[0].StartsWith("sis1h"))
                    modus = Modus.Sis1H;
                else
                {
                    Console.WriteLine("Invalid modus argument: {0}, must be one of [mag1m|mag1h|swepam1m|swepam1h|sis5m|sis1h]");
                    return;
                }
                Repository repository = null;
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Repository.InterceptErrorStack();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                Trace.TraceInformation("=======================================================================================");
                Trace.TraceInformation("Started [{0}]: {1}", args[0], DateTime.Now);
                try
                {
                    string str = Properties.Settings.Default.RepositoryFile;
                    var fileInfo = new FileInfo(str);
                    string directoryName = fileInfo.DirectoryName;
                    if (null != directoryName && !Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                    repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache);
                    Trace.TraceInformation("Traversing...");
                    Instrument instrument;
                    ScalarData scalarData;
                    switch (modus)
                    {
                        case Modus.Mag1M:
                            TraverseTree(args[1], CollectMag);
                            Trace.TraceInformation("Updating Ht 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Mag1H:
                            TraverseTree(args[1], CollectMag);
                            Trace.TraceInformation("Updating Ht 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Swepam1M:
                            TraverseTree(args[1], CollectSwepam);
                            Trace.TraceInformation("Updating Pd 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Ps 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                            scalarList.Clear();
                            foreach (var r in l2List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l2List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Pt 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PtInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                            scalarList.Clear();
                            foreach (var r in l3List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l3List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Swepam1H:
                            TraverseTree(args[1], CollectSwepam);
                            Trace.TraceInformation("Updating Pd 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Ps 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l2List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l2List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Pt 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.PtInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l3List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l3List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Sis5M:
                            TraverseTree(args[1], CollectSis);
                            Trace.TraceInformation("Updating P10 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.P10InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating P30 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.P30InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l2List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l2List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Sis1H:
                            TraverseTree(args[1], CollectSis);
                            Trace.TraceInformation("Updating P10 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.P10InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l1List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l1List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating P30 1h: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.P30InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                            scalarList.Clear();
                            foreach (var r in l2List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l2List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception: [{0}]", e.Message);
                }
                finally
                {
                    foreach (var kvp in dataDictionary)
                    {
                        ScalarData sd = kvp.Value;
                        sd.Flush();
                        sd.Close();
                    }
                    foreach (var kvp in instrumentDictionary)
                    {
                        kvp.Value.Close();
                    }
                    if (null != repository)
                        repository.Close();
                }
                Trace.TraceInformation("Finished: {0}", DateTime.Now);
            }
        }
    }
}
