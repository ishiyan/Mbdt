using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using mbdt.Utils;
using Mbh5;

namespace NgdcDstUpdate
{
    internal static class Program
    {
        private static readonly SortedList<DateTime, double> Dst1HList = new SortedList<DateTime, double>(4096);
        private static readonly SortedList<DateTime, double> Dst1DList = new SortedList<DateTime, double>(1024);
        private static readonly SortedList<DateTime, double> Qdst1HList = new SortedList<DateTime, double>(4096);
        private static readonly SortedList<DateTime, double> Qdst1DList = new SortedList<DateTime, double>(1024);
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
            int year = int.Parse(input.Substring(3, 2), CultureInfo.InvariantCulture);
            if (20 > year)
                year += 2000;
            else
                year += 1900;
            int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
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

        private static void FetchList(string url, SortedList<DateTime, double> dList, SortedList<DateTime, double> hList, List<string> echoList)
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
                    var vh = new string[24];
                    var dh = new double[24];
                    while (null != (line = streamReader.ReadLine()))
                    {
                        if (line.StartsWith("<HTML>"))
                            break;
                        echoList.Add(line);
                        if (line.StartsWith(""))//0x1a
                            break;
                        if (!line.StartsWith("DST"))
                            continue;
                        //DST0701P04       000-011-012-017-020-013-007-005-010-017-012-009-004-005-007-011-017-016-018-019-018-016-018-019-017-013
                        //DST0701P05       000-014-013-009-006-006-003-001-003-010-014-011-008-006-006-009-017-018-016-014-014-016-016-016-015-011
                        if (3 > line.Length)
                            continue;
                        Debug.WriteLine($">[{line}]");
                        DateTime dt = StringToDateTime(line);
                        if (0 == dt.Ticks)
                            continue;
                        for (int i = 0, j = 20; i <24; i++, j += 4)
                            vh[i] = line.Substring(j, 4).Trim();
                        string vd = line.Substring(116, 4).Trim();
                        for (int i = 0; i < 24; i++)
                            dh[i] = double.Parse(vh[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                        double dd = double.Parse(vd, NumberStyles.Any, CultureInfo.InvariantCulture);
                        dList.Add(dt, dd);
                        for (int i = 0; i < 24; i++)
                            hList.Add(dt.AddHours(i), dh[i]);
                        w++;
                        vd = string.Format(CultureInfo.InvariantCulture, "<[{0}]1d[{1}]1h[{2}", dt.ToShortDateString(), dd, dh[0]);
                        for (int i = 1; i < 24; i++)
                            vd = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", vd, dh[i]);
                        Debug.WriteLine(string.Concat(vd, "]"));

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

        private static void Collect(string sourceFileName)
        {
            ImportFile(sourceFileName, Dst1DList, Dst1HList);
        }

        private static void ImportFile(string file, SortedList<DateTime, double> dList, SortedList<DateTime, double> hList)
        {
            Trace.TraceInformation("Importing " + file);
            int w = 0;
            try
            {
                using (var streamReader = new StreamReader(file))
                {
                    string line;
                    var vh = new string[24];
                    var dh = new double[24];
                    while (null != (line = streamReader.ReadLine()))
                    {
                        if (!line.StartsWith("DST"))
                            continue;
                        //DST0701P04       000-011-012-017-020-013-007-005-010-017-012-009-004-005-007-011-017-016-018-019-018-016-018-019-017-013
                        //DST0701P05       000-014-013-009-006-006-003-001-003-010-014-011-008-006-006-009-017-018-016-014-014-016-016-016-015-011
                        if (3 > line.Length)
                            continue;
                        Debug.WriteLine($">[{line}]");
                        DateTime dt = StringToDateTime(line);
                        if (0 == dt.Ticks)
                            continue;
                        for (int i = 0, j = 20; i < 24; i++, j += 4)
                            vh[i] = line.Substring(j, 4).Trim();
                        string vd = line.Substring(116, 4).Trim();
                        for (int i = 0; i < 24; i++)
                            dh[i] = double.Parse(vh[i], NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        double dd = double.Parse(vd, NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite);
                        dList.Add(dt, dd);
                        for (int i = 0; i < 24; i++)
                            hList.Add(dt.AddHours(i), dh[i]);
                        w++;
                        vd = string.Format(CultureInfo.InvariantCulture, "<[{0}]1d[{1}]1h[{2}", dt.ToShortDateString(), dd, dh[0]);
                        for (int i = 1; i < 24; i++)
                            vd = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", vd, dh[i]);
                        Debug.WriteLine(string.Concat(vd, "]"));
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

        static void Main(string[] args)
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
                    int i = dt.Year - 2000;
                    string s;
                    string url;
                    {
                        s = (i - 1).ToString(CultureInfo.InvariantCulture);
                        if (i < 11)
                            s = string.Concat("0", s);
                        url = $"ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/DST/Q-LOOK_{dt.Year}.txt";
                        FetchList(url, Qdst1DList, Qdst1HList, echoList);
                        url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_q-look.{dt.Year}.txt";
                        if (echoList.Count > 0)
                            SaveFile(url, echoList);
                    }
                    s = i.ToString(CultureInfo.InvariantCulture);
                    url = $"ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/DST/Q-LOOK_{dt.Year}.txt";
                    FetchList(url, Qdst1DList, Qdst1HList, echoList);
                    url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_q-look.{dt.Year}.txt";
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                    for (i = Properties.Settings.Default.FirstQuickLookYear; i <= dt.Year; i++)
                    {
                        url = $"ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/DST/dst{i.ToString(CultureInfo.InvariantCulture)}.txt";
                        FetchList(url, Dst1DList, Dst1HList, echoList);
                        if (0 < echoList.Count)
                        {
                            url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_dst.{i}.txt";
                            if (echoList.Count > 0)
                                SaveFile(url, echoList);
                        }
                    }
                    Fetch("ftp://ftp.ngdc.noaa.gov/STP/GEOMAGNETIC_DATA/INDICES/DST/dst-hourly-format.pdf", echoList);
                    url = $"{dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}_dst-hourly-format.pdf";
                    if (echoList.Count > 0)
                        SaveFile(url, echoList);
                }
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                {
                    Trace.TraceInformation("Updating Dst 1d: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.DstInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                    if (Qdst1DList.Count > 0)
                    {
                        scalarList.Clear();
                        foreach (var r in Qdst1DList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Qdst1DList[r];
                            scalarList.Add(scalar);
                        }
                        scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                        scalarData.Flush();
                    }
                    if (Dst1DList.Count > 0)
                    {
                        scalarList.Clear();
                        foreach (var r in Dst1DList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Dst1DList[r];
                            scalarList.Add(scalar);
                        }
                        scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                        scalarData.Flush();
                    }
                    scalarData.Close();
                    instrument.Close();
                }
                {
                    Trace.TraceInformation("Updating Dst 1h: {0}", DateTime.Now);
                    instrument = repository.Open(Properties.Settings.Default.DstInstrumentPath, true);
                    scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Hour1, true);
                    if (Qdst1HList.Count > 0)
                    {
                        scalarList.Clear();
                        foreach (var r in Qdst1HList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Qdst1HList[r];
                            scalarList.Add(scalar);
                        }
                        scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                        scalarData.Flush();
                    }
                    if (Dst1HList.Count > 0)
                    {
                        scalarList.Clear();
                        foreach (var r in Dst1HList.Keys)
                        {
                            scalar.dateTimeTicks = r.Ticks;
                            scalar.value = Dst1HList[r];
                            scalarList.Add(scalar);
                        }
                        scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                        scalarData.Flush();
                    }
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
            if (args.Length == 0)
            {
                Trace.TraceInformation("Zipping: {0}", DateTime.Now);
                Packager.ZipTxtDirectory(string.Concat(downloadFolder, ".zip"), downloadFolder, true);
            }
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
