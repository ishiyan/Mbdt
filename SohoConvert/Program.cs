using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;

using Mbh5;

namespace SohoConvert
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

        private static void Collect(string sourceFileName)
        {
            if (sourceFileName.ToUpperInvariant().EndsWith(".5MIN"))
                CollectCrn5Min(sourceFileName);
            else if (sourceFileName.ToUpperInvariant().EndsWith(".HTML"))
                CollectHtml1Hour(sourceFileName);
            else if (sourceFileName.ToUpperInvariant().EndsWith(".1HR"))
                CollectCrn1Hour(sourceFileName);
        }

        private static void CollectCrn5Min(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    if (l1List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l1List[dt]))
                        {
                            if (!double.IsNaN(dv))
                                l1List[dt] = dv;
                        }
                        else if (double.IsNaN(dv))
                        {
                        }
                        else if (Math.Abs(dv - l1List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l1List[dt], dv);
                                l1List[dt] = dv;
                            }
                            else
                                Trace.TraceError("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l1List[dt], dv);
                        }
                    }
                    else
                        l1List.Add(dt, dv);
                    if (l2List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l2List[dt]))
                        {
                            if (!double.IsNaN(dd))
                                l2List[dt] = dd;
                        }
                        else if (double.IsNaN(dd))
                        {
                        }
                        else if (Math.Abs(dd - l2List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l2List[dt], dd);
                                l2List[dt] = dd;
                            }
                            else
                                Trace.TraceError("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l2List[dt], dd);
                        }
                    }
                    else
                        l2List.Add(dt, dd);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), dv, dd));
                }
                Trace.TraceInformation("Parsing complete: [{0}] rows parsed", w);
            }
        }

        private static void CollectCrn1Hour(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    // NPARAMS=23
                    // FORMAT=(I2.2,1X,I3,1H:,I2.2,1H:,I2.2,1H:,I2.2,F8.1,F9.3,F8.2,F8.2,2F7.2,2F8.1,2F8.2,2F8.2,F6.1,F6.2,F8.2,F8.0,F9.0,F8.1,F7.1,2F7.1,F6.0, I4)
                    //YR DOY:HR:MN:S    Vsw    Dens    Vth   ANGLE   N/S   Sig  Vsw_f  Vsw_m  Dens_f Dens_m Vth_f  Vth_m Msw Alph%   Chi2 PM_Min  PM_Max  Vsw3  Vsw4   Vth3  Vth4 #Poss #ro
                    //96 207:21:00:00   372.6   11.610   24.69   -2.50   5.74   2.45   378.7   373.7   11.36   11.45   25.76   24.75  21.6  0.04    4.15    195.  127510.   371.7  376.0   17.7   25.7   33.   4
                    if (!line.StartsWith("0", StringComparison.Ordinal) && !line.StartsWith("1", StringComparison.Ordinal) && !line.StartsWith("9", StringComparison.Ordinal) &&
                        !line.StartsWith("2", StringComparison.Ordinal) && !line.StartsWith("3", StringComparison.Ordinal) && !line.StartsWith("4", StringComparison.Ordinal))
                        continue;
                    Debug.WriteLine(string.Format(">[{0}]", line));
                    DateTime dt = StringToDateTime(line);
                    string vv = line.Substring(15, 8).Trim();
                    string vd = line.Substring(23, 9).Trim();
                    double dv = vv.StartsWith(missing) ? double.NaN : double.Parse(vv, NumberStyles.Any, CultureInfo.InvariantCulture);
                    double dd = vd.StartsWith(missing) ? double.NaN : double.Parse(vd, NumberStyles.Any, CultureInfo.InvariantCulture);
                    if (l5List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l5List[dt]))
                        {
                            if (!double.IsNaN(dv))
                                l5List[dt] = dv;
                        }
                        else if (double.IsNaN(dv))
                        {
                        }
                        else if (Math.Abs(dv - l5List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l5List[dt], dv);
                                l5List[dt] = dv;
                            }
                            else
                                Trace.TraceError("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l5List[dt], dv);
                        }
                    }
                    else
                        l5List.Add(dt, dv);
                    if (l6List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l6List[dt]))
                        {
                            if (!double.IsNaN(dd))
                                l6List[dt] = dd;
                        }
                        else if (double.IsNaN(dd))
                        {
                        }
                        else if (Math.Abs(dd - l6List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l6List[dt], dd);
                                l6List[dt] = dd;
                            }
                            else
                                Trace.TraceError("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l6List[dt], dd);
                        }
                    }
                    else
                        l6List.Add(dt, dd);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), dv, dd));
                }
                Trace.TraceInformation("Parsing complete: [{0}] rows parsed", w);
            }
        }

        private static void CollectHtml1Hour(string sourceFileName)
        {
            Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
            const string missing = "-1";
            int w = 0;
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
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
                    if (l3List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l3List[dt]))
                        {
                            if (!double.IsNaN(dv))
                                l3List[dt] = dv;
                        }
                        else if (double.IsNaN(dv))
                        {
                        }
                        else if (Math.Abs(dv - l3List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l3List[dt], dv);
                                l3List[dt] = dv;
                            }
                            else
                                Trace.TraceError("Ps: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l3List[dt], dv);
                        }
                    }
                    else
                        l3List.Add(dt, dv);
                    if (l4List.ContainsKey(dt))
                    {
                        if (double.IsNaN(l4List[dt]))
                        {
                            if (!double.IsNaN(dd))
                                l4List[dt] = dd;
                        }
                        else if (double.IsNaN(dd))
                        {
                        }
                        else if (Math.Abs(dd - l4List[dt]) > double.Epsilon)
                        {
                            if (Properties.Settings.Default.UpdateExisting)
                            {
                                Trace.TraceInformation("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", dt, l4List[dt], dd);
                                l4List[dt] = dd;
                            }
                            else
                                Trace.TraceError("Pd: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", dt, l4List[dt], dd);
                        }
                    }
                    else
                        l4List.Add(dt, dd);
                    w++;
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}][{1}][{2}]", dt.ToShortDateString(), dv, dd));
                }
                Trace.TraceInformation("Parsing complete: [{0}] rows parsed", w);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: dir_or_file_name");
            else
            {
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
                    TraverseTree(args[0], Collect);
                    Instrument instrument;
                    ScalarData scalarData;
                    if (l1List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Ps 5m: {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
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
                    }
                    if (l2List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Pd 5m: {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
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
                    }
                    if (l3List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Ps 1h (html): {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
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
                    }
                    if (l4List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Pd 1h (html): {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                        scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
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
                    }
                    if (l5List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Ps 1h: {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PsInstrumentPath, true);
                        scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
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
                    }
                    if (l6List.Count > 0)
                    {
                        Trace.TraceInformation("Updating Pd 1h: {0}", DateTime.Now);
                        instrument = repository.Open(Properties.Settings.Default.PdInstrumentPath, true);
                        scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
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
