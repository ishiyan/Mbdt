using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace SwpcGoesConvert
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
        static private readonly SortedList<DateTime, double> l4List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> l5List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> l6List = new SortedList<DateTime, double>(1024);
        static private readonly SortedList<DateTime, double> l7List = new SortedList<DateTime, double>(1024);

        private static DateTime StringToDateTime(string input)
        {
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(input.Substring(12, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(input.Substring(14, 2), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, hour, minute, 0);
        }

        private static void CollectXray(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            string missing = "-1.00e+05";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, ds);
                    l2List.Add(dt, dl);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), ds, dl));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectMag(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            string missing = "-1.00e+05";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, dp);
                    l2List.Add(dt, de);
                    l3List.Add(dt, dn);
                    l4List.Add(dt, ds);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}]", dt.ToShortDateString(), dp, de, dn, ds));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectIpart(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            string missing = "-1.00e+05";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, dp1);
                    l2List.Add(dt, dp5);
                    l3List.Add(dt, dp10);
                    l4List.Add(dt, dp50);
                    l5List.Add(dt, dp100);
                    l6List.Add(dt, de08);
                    l7List.Add(dt, de20);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}][{5}][{6}][{7}]", dt.ToShortDateString(), dp1, dp5, dp10, dp50, dp100, de08, de20));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private enum Modus
        {
            Xray1M, Xray5M, Mag1M, Ipart5M
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("Arguments: [xray1m|xray5m|mag1m|ipart5m] dir_or_file_name");
            else
            {
                Modus modus;
                if (args[0].StartsWith("xray1m"))
                    modus = Modus.Xray1M;
                else if (args[0].StartsWith("xray5m"))
                    modus = Modus.Xray5M;
                else if (args[0].StartsWith("mag1m"))
                    modus = Modus.Mag1M;
                else if (args[0].StartsWith("ipart5m"))
                    modus = Modus.Ipart5M;
                else
                {
                    Console.WriteLine("Invalid modus argument: {0}, must be one of [xray1m|xray5m|mag1m|ipart5m]");
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
                        case Modus.Xray1M:
                            TraverseTree(args[1], CollectXray);
                            Trace.TraceInformation("Updating Xs 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.XsInstrumentPath, true);
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
                            Trace.TraceInformation("Updating Xl 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.XlInstrumentPath, true);
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
                            break;
                        case Modus.Xray5M:
                            TraverseTree(args[1], CollectXray);
                            Trace.TraceInformation("Updating Xs 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.XsInstrumentPath, true);
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
                            Trace.TraceInformation("Updating Xl 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.XlInstrumentPath, true);
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
                        case Modus.Mag1M:
                            TraverseTree(args[1], CollectMag);
                            Trace.TraceInformation("Updating Hp 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HpInstrumentPath, true);
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
                            Trace.TraceInformation("Updating He 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HeInstrumentPath, true);
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
                            Trace.TraceInformation("Updating Hn 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HnInstrumentPath, true);
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
                            Trace.TraceInformation("Updating Ht 1m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.HtInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute1, true);
                            scalarList.Clear();
                            foreach (var r in l4List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l4List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            break;
                        case Modus.Ipart5M:
                            TraverseTree(args[1], CollectIpart);
                            Trace.TraceInformation("Updating Ip1 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ip1InstrumentPath, true);
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
                            Trace.TraceInformation("Updating Ip5 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ip5InstrumentPath, true);
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
                            Trace.TraceInformation("Updating Ip10 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ip10InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
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
                            Trace.TraceInformation("Updating Ip50 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ip50InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l4List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l4List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Ip100 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ip100InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l5List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l5List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Ie08 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ie08InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l6List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l6List[r];
                                scalarList.Add(scalar);
                            }
                            scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            scalarData.Flush();
                            scalarData.Close();
                            instrument.Close();
                            Trace.TraceInformation("Updating Ie20 5m: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ie20InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Minute5, true);
                            scalarList.Clear();
                            foreach (var r in l7List.Keys)
                            {
                                scalar.dateTimeTicks = r.Ticks;
                                scalar.value = l7List[r];
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
