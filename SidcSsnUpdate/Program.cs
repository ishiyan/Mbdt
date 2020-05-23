using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text;
using Mbh5;

namespace SidcSsnUpdate
{
    static class Program
    {
        private static readonly SortedList<DateTime, double> RtList = new SortedList<DateTime, double>();
        private static readonly SortedList<DateTime, double> RsList = new SortedList<DateTime, double>();
        private static readonly SortedList<DateTime, double> RnList = new SortedList<DateTime, double>();

        private static void Fetch(string url)
        {
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
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
                        Debug.WriteLine($">[{line}]");
                        if (line.Length < 37)
                            Trace.TraceError("illegal line [{0}], length < 37", line);

                        string s = line.Substring(0, 10);
                        if (s[5] == ' ')
                        {
                            var sb = new StringBuilder(s) { [5] = '0' };
                            s = sb.ToString();
                        }
                        DateTime dt;
                        try
                        {
                            dt = DateTime.ParseExact(s, "yyyy MM dd", CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("cannot parse date-time [{0}] in line [{1}], skipping the line", s, line);
                            continue;
                        }

                        s = line.Substring(20, 5).Trim();
                        int rt;
                        try
                        {
                            rt = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            Trace.TraceError("cannot parse total sunspot number [{0}] in line [{1}], skipping the line", s, line);
                            continue;
                        }

                        if (line.Length > 37)
                        {
                            s = line.Substring(24, 5).Trim();
                            int rn;
                            try
                            {
                                rn = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("cannot parse north hemisphere sunspot number [{0}] in line [{1}], skipping the line", s, line);
                                continue;
                            }

                            s = line.Substring(24, 5).Trim();
                            int rs;
                            try
                            {
                                rs = int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                Trace.TraceError("cannot parse south hemisphere sunspot number [{0}] in line [{1}], skipping the line", s, line);
                                continue;
                            }

                            RnList[dt] = rn < 0 ? double.NaN : rn;
                            RsList[dt] = rs < 0 ? double.NaN : rs;
                        }

                        RtList[dt] = rt < 0 ? double.NaN : rt;

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
                Trace.TraceInformation("Download complete");
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

        static void Main()
        {
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            DateTime dt = DateTime.Now;
            Trace.TraceInformation("Started: {0}", dt);
            try
            {
                Fetch("http://www.sidc.be/silso/DATA/EISN/EISN_current.txt");
                Fetch("http://www.sidc.be/silso/DATA/SN_d_hem_V2.0.txt");

                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
                {

                    Trace.TraceInformation("Updating Rt: {0}", DateTime.Now);
                    foreach (var r in RtList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RtList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RtInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }

                    Trace.TraceInformation("Updating Rn: {0}", DateTime.Now);
                    scalarList.Clear();
                    foreach (var r in RnList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RnList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RnInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
                            {
                                scalarData.Add(scalarList, DuplicateTimeTicks.Update, true);
                            }
                        }
                    }

                    Trace.TraceInformation("Updating Rs: {0}", DateTime.Now);
                    scalarList.Clear();
                    foreach (var r in RsList.Keys)
                    {
                        scalar.dateTimeTicks = r.Ticks;
                        scalar.value = RsList[r];
                        scalarList.Add(scalar);
                    }

                    if (scalarList.Count > 0)
                    {
                        using (Instrument instrument = repository.Open(Properties.Settings.Default.RsInstrumentPath, true))
                        {
                            using (ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true))
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
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
