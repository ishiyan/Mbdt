using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using Mbh5;

namespace EoniaUpdate
{
    static class Program
    {
        private class Rate
        {
            internal DateTime DateTime;
            internal string Eonia;

            internal bool IsGood
            {
                get
                {
                    return (2 == (DateTime.Year / 1000) && !string.IsNullOrEmpty(Eonia));
                }
            }
            internal string Dump
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("[{0}-{1}-{2}: {3}", DateTime.Year, DateTime.Month, DateTime.Day, string.IsNullOrEmpty(Eonia) ? "<null>" : Eonia);
                    return sb.ToString();
                }
            }
        }

        private static IEnumerable<Rate> Fetch()
        {
            const string url = "https://www.euribor-rates.eu/en/eonia/";
            var list = new List<Rate>(10);
            for (int i = 0; i < 10; i++)
                list.Add(new Rate());
            const string errorFormat = "unexpected line [{0}] failed to find [{1}], aborting";
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            // DefaultCredentials represents the system credentials for the current
            // security context in which the application is running. For a client-side
            // application, these are usually the Windows credentials
            // (user name, password, and domain) of the user running the application.
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            WebResponse webResponse = webRequest.GetResponse();
            Stream responseStream = webResponse.GetResponseStream();
            if (null == responseStream)
            {
                Trace.TraceError("Received null response stream.");
                return list;
            }
            using (var streamReader = new StreamReader(responseStream))
            {
                const string pattern1 = "<small class=\"text-muted\">Current rate</small>";
                const string pattern2 = "<tr><td>";
                string line = streamReader.ReadLine();
                while (null != line)
                {
                    int i = line.IndexOf(pattern1, StringComparison.Ordinal);
                    if (-1 < i)
                    {

                        string date;
                        string rate;
                        // streamReader.ReadLine(); // [<h2>By day</h2>]
                        // streamReader.ReadLine(); // [<small class=\"text-muted\">Current rate</small>]
                        streamReader.ReadLine(); // [</div>]
                        streamReader.ReadLine(); // [<div class="card-body">]
                        streamReader.ReadLine(); // [<table class="table table-striped">]
                        streamReader.ReadLine(); // [<tbody>]
                        line = streamReader.ReadLine(); // [<tr><td>4/8/2020</td><td class="text-right">-0.450 %</td></tr>]
                        if (null == line || !line.Contains(pattern2))
                        {
                            Trace.TraceError(errorFormat, line, pattern2);
                            return list;
                        }
                        Debug.WriteLine(">" + line);
                        if (!ParseLine(line, out date, out rate))
                        {
                            Trace.TraceError(errorFormat, line, pattern2);
                            return list;
                        }
                        Debug.WriteLine(">" + date + ", " + rate);
                        list[9].DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);
                        list[9].Eonia = rate;

                        int j;
                        for (j = 8; j >= 0; j--)
                        {
                            line = streamReader.ReadLine(); // [<tr><td>4/7/2020</td><td class="text-right">-0.448 %</td></tr>]
                            if (null == line || !line.Contains(pattern2))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            Debug.WriteLine(">" + line);
                            if (!ParseLine(line, out date, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            Debug.WriteLine(">" + date + ", " + rate);
                            list[j].DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);
                            list[j].Eonia = rate;
                        }
                        return list;
                    }
                    line = streamReader.ReadLine();
                }
            }
            return list;
        }

        private static bool ParseLine(string line, out string date, out string rate)
        {
            date = null;
            rate = null;
            // [<tr><td>4/8/2020</td><td class="text-right">-0.450 %</td></tr>]
            const string pattern1 = "<tr><td>";
            const string pattern2 = "</td><td class=\"text-right\">";
            const string pattern3 = "%</td></tr>";

            int i = line.IndexOf(pattern1, StringComparison.Ordinal);
            if (i < 0)
                return false;

            line = line.Substring(i + pattern1.Length);
            i = line.IndexOf(pattern2, StringComparison.Ordinal);
            if (i < 0)
                return false;
            date = line.Substring(0, i);

            line = line.Substring(i + pattern2.Length);
            i = line.IndexOf(pattern3, StringComparison.Ordinal);
            if (i < 0)
                return false;
            rate = line.Substring(0, i).Trim(' ');

            return true;
        }

        static void Main()
        {
            Repository repository = null;
            ScalarData scalarData = null;
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            Instrument instrument = null;
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            Trace.TraceInformation("Started: {0}", DateTime.Now);
            try
            {
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                instrument = repository.Open(Properties.Settings.Default.RepositoryRoot, true);
                scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                IEnumerable<Rate> list = Fetch();
                //list.Reverse();// Already ordered chronologically in Fetch().
                scalarList.Clear();
                foreach (var r in list)
                {
                    if (r.IsGood)
                    {
                        scalar.dateTimeTicks = r.DateTime.Ticks;
                        scalar.value = double.Parse(r.Eonia, CultureInfo.InvariantCulture);
                        scalarList.Add(scalar);
                    }
                    else
                        Trace.TraceError("Bad rate: " + r.Dump);
                }
                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
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
            Trace.TraceInformation("Finished: {0}", DateTime.Now);
        }
    }
}
