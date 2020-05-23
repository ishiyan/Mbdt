using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace NgdcKpApUpdate
{
    internal static class Program
    {
        private static readonly SortedList<DateTime, double> Kp3HList = new SortedList<DateTime, double>(4096);
        private static readonly SortedList<DateTime, double> Ap3HList = new SortedList<DateTime, double>(4096);
        private static readonly SortedList<DateTime, double> Ap1DList = new SortedList<DateTime, double>(1024);
        private static readonly SortedList<DateTime, double> Cp1DList = new SortedList<DateTime, double>(1024);
        private static string folder, downloadFolder;

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
            int year = int.Parse(input.Substring(0, 2), CultureInfo.InvariantCulture);
            if (20 > year)
                year += 2000;
            else
                year += 1900;
            int month = int.Parse(input.Substring(2, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(4, 2), CultureInfo.InvariantCulture);
            DateTime dt;
            try
            {
                dt = new DateTime(year, month, day, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Bad date format on line [{0}], skipping: [{1}]", input, ex.Message);
                dt = new DateTime(0);
            }
            return dt;
        }

        private static void FetchList(string url, SortedList<DateTime, double> khList, SortedList<DateTime, double> ahList, SortedList<DateTime, double> adList, SortedList<DateTime, double> cdList, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (FtpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
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
                    string[] vk = new string[8], va = new string[8];
                    double[] dk = new double[8], da = new double[8];
                    while (null != (line = streamReader.ReadLine()))
                    {
                        echoList.Add(line);
                        //          1111111111222222222233333333334444444444555555555566666666667777777777
                        //01234567890123456789012345678901234567890123456789012345678901234567890123456789
                        //            0011223344556677   000111222333444555666777xxx---
                        //1002022408203020101720171340167 15  7  4  6  7  6  5 27 100.52  9 73.00
                        //10020324082130102727272720 7173 15  4 12 12 12 12  7  3 100.52  9 72.30
                        //1801012515213337232327101013177 18 22  9  9 12  4  4  5 100.63-- - 066.80
                        //18010225152217 7 3 717102010 90  6  3  2  3  6  4  7  4  40.21-- - 067.20

                        if (line.StartsWith("")) // 0x1a
                            break;
                        if (3 > line.Length)
                            continue;
                        Debug.WriteLine($">[{line}]");

                        DateTime dt = StringToDateTime(line);
                        if (0 == dt.Ticks)
                            continue;

                        // 8 values of 3-hour Kp
                        for (int i = 0, j = 12; i < 8; i++, j += 2)
                            vk[i] = line.Substring(j, 2).Trim();

                        // 8 values of 3-hour Ap
                        for (int i = 0, j = 31; i < 8; i++, j += 3)
                            va[i] = line.Substring(j, 3).Trim();

                        // mean of 8 Ap values
                        string vad = line.Substring(55, 3).Trim();

                        // cp value
                        string vcd = line.Substring(58, 3).Trim();
                        for (int i = 0; i < 8; ++i)
                        {
                            try
                            {
                                dk[i] = double.Parse(vk[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("Failed to parse [{0}]-th 3-hour Kp value [{1}] in line [{2}], skipping the line.", i, vk[i], line);
                                goto nextLine;
                            }

                            try
                            {
                                da[i] = double.Parse(va[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("Failed to parse [{0}]-th 3-hour Ap value [{1}] in line [{2}], skipping the line.", i, va[i], line);
                                goto nextLine;
                            }
                        }

                        double dad;
                        try
                        {
                            dad = double.Parse(vad, NumberStyles.Any, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("Failed to parse mean of 8 Ap values [{0}] in line [{1}], skipping the line.", vad, line);
                            goto nextLine;
                        }

                        double dcd;
                        try
                        {
                            dcd = double.Parse(vcd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("Failed to parse Cp value [{0}] in line [{1}], skipping the line.", vcd, line);
                            goto nextLine;
                        }

                        adList.Add(dt, dad);
                        cdList.Add(dt, dcd);
                        for (int i = 0; i < 8; ++i)
                        {
                            khList.Add(dt.AddHours(i * 3), dk[i]);
                            ahList.Add(dt.AddHours(i * 3), da[i]);
                        }
                        ++w;

                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}]ap[{1}]cp[{2}]", dt.ToShortDateString(), dad, dcd));
                        nextLine:

                        // Get rid of exception: [Cannot access a disposed object. Object name: 'System.Net.Sockets.NetworkStream'.]
                        try
                        {
                            if (streamReader.EndOfStream)
                                break;
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("(1) Got exception: [{0}]", ex);
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

        private static void Collect(string sourceFileName)
        {
            ImportFile(sourceFileName, Kp3HList, Ap3HList, Ap1DList, Cp1DList);
        }

        private static void ImportFile(string file, SortedList<DateTime, double> khList, SortedList<DateTime, double> ahList, SortedList<DateTime, double> adList, SortedList<DateTime, double> cdList)
        {
            Trace.TraceInformation("Importing " + file);
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(file))
                {
                    string line;
                    string[] vk = new string[8], va = new string[8];
                    double[] dk = new double[8], da = new double[8];
                    while (null != (line = streamReader.ReadLine()))
                    {
                        //          1111111111222222222233333333334444444444555555555566666666667777777777
                        //01234567890123456789012345678901234567890123456789012345678901234567890123456789
                        //            0011223344556677   000111222333444555666777xxx---
                        //1002022408203020101720171340167 15  7  4  6  7  6  5 27 100.52  9 73.00
                        //10020324082130102727272720 7173 15  4 12 12 12 12  7  3 100.52  9 72.30
                        //1801012515213337232327101013177 18 22  9  9 12  4  4  5 100.63-- - 066.80
                        //18010225152217 7 3 717102010 90  6  3  2  3  6  4  7  4  40.21-- - 067.20

                        if (line.StartsWith("")) // 0x1a
                            break;
                        if (3 > line.Length)
                            continue;
                        Debug.WriteLine($">[{line}]");

                        DateTime dt = StringToDateTime(line);
                        if (0 == dt.Ticks)
                            continue;

                        // 8 values of 3-hour Kp
                        for (int i = 0, j = 12; i < 8; i++, j += 2)
                            vk[i] = line.Substring(j, 2).Trim();

                        // 8 values of 3-hour Ap
                        for (int i = 0, j = 31; i < 8; i++, j += 3)
                            va[i] = line.Substring(j, 3).Trim();

                        // mean of 8 Ap values
                        string vad = line.Substring(55, 3).Trim();

                        // cp value
                        string vcd = line.Substring(58, 3).Trim();
                        for (int i = 0; i < 8; ++i)
                        {
                            try
                            {
                                dk[i] = double.Parse(vk[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("Failed to parse [{0}]-th 3-hour Kp value [{1}] in line [{2}], skipping the line.", i, vk[i], line);
                                goto nextLine;
                            }

                            try
                            {
                                da[i] = double.Parse(va[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("Failed to parse [{0}]-th 3-hour Ap value [{1}] in line [{2}], skipping the line.", i, va[i], line);
                                goto nextLine;
                            }
                        }

                        double dad;
                        try
                        {
                            dad = double.Parse(vad, NumberStyles.Any, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("Failed to parse mean of 8 Ap values [{0}] in line [{1}], skipping the line.", vad, line);
                            goto nextLine;
                        }

                        double dcd;
                        try
                        {
                            dcd = double.Parse(vcd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("Failed to parse Cp value [{0}] in line [{1}], skipping the line.", vcd, line);
                            goto nextLine;
                        }

                        adList.Add(dt, dad);
                        cdList.Add(dt, dcd);
                        for (int i = 0; i < 8; ++i)
                        {
                            khList.Add(dt.AddHours(i * 3), dk[i]);
                            ahList.Add(dt.AddHours(i * 3), da[i]);
                        }
                        ++w;

                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "<[{0}]ap[{1}]cp[{2}]", dt.ToShortDateString(), dad, dcd));
                        nextLine: ;
                    }
                }
                Trace.TraceInformation("Import complete: [{0}] rows fetched", w);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Download failed: [{0}]", ex.Message);
            }
        }

        private static void Fetch(string url, List<string> echoList)
        {
            Trace.TraceInformation("Downloading URL " + url);
            echoList.Clear();
            var webRequest = (FtpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
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
                        catch (Exception ex)
                        {
                            Trace.TraceError("(2) Got exception: [{0}]", ex);
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

        static void Main(string[] args)
        {
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            var echoList = new List<string>(4096);
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            DateTime dt = DateTime.Now;
            Trace.TraceInformation("Started: {0}", dt);
            try
            {
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                if (args.Length > 0)
                {
                    TraverseTree(args[0], Collect);
                }
                else
                {
                    folder = dt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                    downloadFolder = string.Concat(Properties.Settings.Default.DownloadFolder, folder);
                    if (!Directory.Exists(downloadFolder))
                        Directory.CreateDirectory(downloadFolder);

                    string url;
                    for (int i = dt.Year - Properties.Settings.Default.LookbackYears; i <= dt.Year; i++)
                    {
                        url = $"ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/KP_AP/{i.ToString(CultureInfo.InvariantCulture)}";
                        FetchList(url, Kp3HList, Ap3HList, Ap1DList, Cp1DList, echoList);
                        if (0 < echoList.Count)
                        {
                            url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_{i}.txt";
                            if (echoList.Count > 0)
                                SaveFile(url, echoList);
                        }
                    }

                    Fetch("ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/KP_AP/kp_ap.fmt", echoList);
                    url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_kp_ap_fmt.txt";
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                }

                Repository.InterceptErrorStack();
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                {
                    if (Ap1DList.Count > 0)
                    {
                        Trace.TraceInformation("Updating Ap 1d: {0}", DateTime.Now);
                        scalarList.Clear();
                        foreach (var r in Ap1DList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Ap1DList[r];
                            scalarList.Add(scalar);
                        }

                        using (Instrument instrument = repository.Open(Properties.Settings.Default.ApInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }

                    if (Ap3HList.Count > 0)
                    {
                        Trace.TraceInformation("Updating Ap 3h: {0}", DateTime.Now);
                        scalarList.Clear();
                        foreach (var r in Ap3HList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Ap3HList[r];
                            scalarList.Add(scalar);
                        }

                        using (Instrument instrument = repository.Open(Properties.Settings.Default.ApInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour3, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }

                    if (Cp1DList.Count > 0)
                    {
                        Trace.TraceInformation("Updating Cp 1d: {0}", DateTime.Now);
                        scalarList.Clear();
                        foreach (var r in Cp1DList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Cp1DList[r];
                            scalarList.Add(scalar);
                        }

                        using (Instrument instrument = repository.Open(Properties.Settings.Default.CpInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }

                    if (Kp3HList.Count > 0)
                    {
                        Trace.TraceInformation("Updating Kp 3h: {0}", DateTime.Now);
                        scalarList.Clear();
                        foreach (var r in Kp3HList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Kp3HList[r];
                            scalarList.Add(scalar);
                        }

                        using (Instrument instrument = repository.Open(Properties.Settings.Default.KpInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour3, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception: [{0}]", e.Message);
            }
            if (args.Length == 0)
            {
                Trace.TraceInformation("Zipping: {0}", DateTime.Now);
                Packager.ZipTxtDirectory(string.Concat(downloadFolder, ".zip"), downloadFolder, true);
            }
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
