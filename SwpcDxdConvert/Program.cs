using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace SwpcDxdConvert
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
            return new DateTime(year, month, day, 0, 0, 0);
        }

        private static void CollectDgd(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-1";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                        l1List.Add(dt, da);
                    }
                    string vk1 = line.Substring(63, 2).Trim();
                    double dk1;
                    if (vk1.StartsWith(missing))
                        dk1 = double.NaN;
                    else
                    {
                        dk1 = double.Parse(vk1, NumberStyles.Any, CultureInfo.InvariantCulture);
                        //t = dt.AddHours(0);
                        l2List.Add(dt, dk1);
                    }
                    string vk2 = line.Substring(65, 2).Trim();
                    double dk2;
                    DateTime t;
                    if (vk2.StartsWith(missing))
                        dk2 = double.NaN;
                    else
                    {
                        dk2 = double.Parse(vk2, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(3);
                        l2List.Add(t, dk2);
                    }
                    string vk3 = line.Substring(67, 2).Trim();
                    double dk3;
                    if (vk3.StartsWith(missing))
                        dk3 = double.NaN;
                    else
                    {
                        dk3 = double.Parse(vk3, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(6);
                        l2List.Add(t, dk3);
                    }
                    string vk4 = line.Substring(69, 2).Trim();
                    double dk4;
                    if (vk4.StartsWith(missing))
                        dk4 = double.NaN;
                    else
                    {
                        dk4 = double.Parse(vk4, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(9);
                        l2List.Add(t, dk4);
                    }
                    string vk5 = line.Substring(71, 2).Trim();
                    double dk5;
                    if (vk5.StartsWith(missing))
                        dk5 = double.NaN;
                    else
                    {
                        dk5 = double.Parse(vk5, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(12);
                        l2List.Add(t, dk5);
                    }
                    string vk6 = line.Substring(73, 2).Trim();
                    double dk6;
                    if (vk6.StartsWith(missing))
                        dk6 = double.NaN;
                    else
                    {
                        dk6 = double.Parse(vk6, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(15);
                        l2List.Add(t, dk6);
                    }
                    string vk7 = line.Substring(75, 2).Trim();
                    double dk7;
                    if (vk7.StartsWith(missing))
                        dk7 = double.NaN;
                    else
                    {
                        dk7 = double.Parse(vk7, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(18);
                        l2List.Add(t, dk7);
                    }
                    string vk8 = line.Substring(77, 2).Trim();
                    double dk8;
                    if (vk8.StartsWith(missing))
                        dk8 = double.NaN;
                    else
                    {
                        dk8 = double.Parse(vk8, NumberStyles.Any, CultureInfo.InvariantCulture);
                        t = dt.AddHours(21);
                        l2List.Add(t, dk8);
                    }
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] a[{1}] k1[{2}] k2[{3}] k3[{4}] k4[{5}] k5[{6}] k6[{7}] k7[{8}] k8[{9}]", dt.ToShortDateString(), da, dk1, dk2, dk3, dk4, dk5, dk6, dk7, dk8));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectDsd(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missingX = "*";
            const string missing = "-1";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    l1List.Add(dt, dr);
                    l2List.Add(dt, ds);
                    l3List.Add(dt, da);
                    l4List.Add(dt, dx);
                    l5List.Add(dt, dnc);
                    l6List.Add(dt, dnm);
                    l7List.Add(dt, dnx);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}][{3}][{4}][{5}][{6}][{7}]", dt.ToShortDateString(), dr, ds, da, dx, dnc, dnm, dnx));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private static void CollectDpd(string sourceFileName)
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
                    string vp1 = line.Substring(10, 11).Trim();
                    string vp10 = line.Substring(21, 9).Trim();
                    string vp100 = line.Substring(30, 9).Trim();
                    string ve8 = line.Substring(39, 10).Trim();
                    string ve20 = line.Substring(49, 11).Trim();
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
                    l1List.Add(dt, dp1);
                    l2List.Add(dt, dp10);
                    l3List.Add(dt, dp100);
                    l4List.Add(dt, de8);
                    l5List.Add(dt, de20);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}] p1[{1}] p10[{2}] p100[{3}] e8[{4}] e20[{5}]", dt.ToShortDateString(), dp1, dp10, dp100, de8, de20));
                }
            }
            Trace.TraceInformation("Collection complete: [{0}] rows parsed", w);
        }

        private enum Modus
        {
            Dgd, Dsd, Dpd
        }

        static void Main(string[] args)
        {
            if (args.Length < 2)
                Console.WriteLine("Arguments: [dgd|dsd|dpd] dir_or_file_name");
            else
            {
                Modus modus;
                if (args[0].StartsWith("dgd"))
                    modus = Modus.Dgd;
                else if (args[0].StartsWith("dsd"))
                    modus = Modus.Dsd;
                else if (args[0].StartsWith("dpd"))
                    modus = Modus.Dpd;
                else
                {
                    Console.WriteLine("Invalid modus argument: {0}, must be one of [dgd|dsd|dpd]");
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
                        case Modus.Dgd:
                            TraverseTree(args[1], CollectDgd);
                            Trace.TraceInformation("Updating Ap: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.ApInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Kp: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.KpInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour3, true);
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
                        case Modus.Dsd:
                            TraverseTree(args[1], CollectDsd);
                            Trace.TraceInformation("Updating Rf: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.RfInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating SescSsn: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.SescSsnInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Ssa: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.SsaInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Xf: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.XfInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Nc: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.NcInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Nm: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.NmInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Nx: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.NxInstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                        case Modus.Dpd:
                            TraverseTree(args[1], CollectDpd);
                            Trace.TraceInformation("Updating Pf1: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Pf1InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Pf10: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Pf10InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Pf100: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Pf100InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Ef8: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ef8InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
                            Trace.TraceInformation("Updating Ef20: {0}", DateTime.Now);
                            instrument = repository.Open(Properties.Settings.Default.Ef20InstrumentPath, true);
                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
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
