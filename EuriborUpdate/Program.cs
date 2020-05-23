using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using Mbh5;

namespace EuriborUpdate
{
    static class Program
    {
        private class Rates
        {
            internal DateTime DateTime;
            internal readonly List<string> Euribor = new List<string>(5);
            internal Rates()
            {
                for (int i = 0; i < 5; i++)
                    Euribor.Add(null);
            }
            internal bool IsGood
            {
                get
                {
                    if (2 != (DateTime.Year / 1000))
                        return false;
                    return Euribor.All(s => !string.IsNullOrEmpty(s));
                }
            }
            internal string Dump
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("[{0}-{1}-{2}:", DateTime.Year, DateTime.Month, DateTime.Day);
                    sb.AppendFormat(" 1w({0})", Euribor[0]);
                    sb.AppendFormat(" 1m({0})", Euribor[1]);
                    sb.AppendFormat(" 3m({0})", Euribor[2]);
                    sb.AppendFormat(" 6m({0})", Euribor[3]);
                    sb.AppendFormat(" 12m({0})]", Euribor[4]);
                    return sb.ToString();
                }
            }

        }

        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();
        private static readonly List<string> nameList = NameList();
        private static List<string> NameList()
        {
            return new List<string>(5) {"EURIBOR1W", "EURIBOR1M", "EURIBOR3M", "EURIBOR6M", "EURIBOR12M"};
        }

        private static IEnumerable<Rates> Fetch()
        {
            const string url = "https://www.euribor-rates.eu/en/current-euribor-rates/"; // http://www.euribor-rates.eu/current-euribor-rates.asp
            var list = new List<Rates>(5);
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
                const string pattern1 = "<th class=\"text-right\">"; // <th class="text-right">4/9/2020</th>
                const string pattern2 = "<td class=\"text-right\">"; // <td class="text-right">-0.507 %</td>
                Rates rates1 = new Rates(), rates2 = new Rates(), rates3 = new Rates(), rates4 = new Rates(), rates5 = new Rates();
                string line = streamReader.ReadLine();
                while (null != line)
                {
                    int i = line.IndexOf(pattern1, StringComparison.Ordinal);
                    if (-1 < i)
                    {
                        string date;
                        string rate;
                        Debug.WriteLine(">" + line);
                        if (!ParseDateLine(line, out date))
                        {
                            Trace.TraceError(errorFormat, line, pattern1);
                            return list;
                        }
                        rates1.DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);

                        line = streamReader.ReadLine(); // [<th class="text-right">4/8/2020</th>]
                        Debug.WriteLine(">" + line);
                        if (!ParseDateLine(line, out date))
                        {
                            Trace.TraceError(errorFormat, line, pattern1);
                            return list;
                        }
                        rates2.DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);

                        line = streamReader.ReadLine(); // [<th class="text-right">4/8/2020</th>]
                        Debug.WriteLine(">" + line);
                        if (!ParseDateLine(line, out date))
                        {
                            Trace.TraceError(errorFormat, line, pattern1);
                            return list;
                        }
                        rates3.DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);

                        line = streamReader.ReadLine(); // [<th class="text-right">4/8/2020</th>]
                        Debug.WriteLine(">" + line);
                        if (!ParseDateLine(line, out date))
                        {
                            Trace.TraceError(errorFormat, line, pattern1);
                            return list;
                        }
                        rates4.DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);

                        line = streamReader.ReadLine(); // [<th class="text-right">4/8/2020</th>]
                        Debug.WriteLine(">" + line);
                        if (!ParseDateLine(line, out date))
                        {
                            Trace.TraceError(errorFormat, line, pattern1);
                            return list;
                        }
                        rates5.DateTime = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);

                        for (int j = 0; j < 5; j++)
                        {
                            while (true)
                            {
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern2, StringComparison.Ordinal); // [<td class="text-right">-0.507 %</td>]
                                if (i >= 0)
                                    break;
                            }
                            Debug.WriteLine(">" + line);
                            if (!ParseRateLine(line, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            rates1.Euribor[j] = rate;

                            line = streamReader.ReadLine();
                            Debug.WriteLine(">" + line);
                            if (!ParseRateLine(line, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            rates2.Euribor[j] = rate;

                            line = streamReader.ReadLine();
                            Debug.WriteLine(">" + line);
                            if (!ParseRateLine(line, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            rates3.Euribor[j] = rate;

                            line = streamReader.ReadLine();
                            Debug.WriteLine(">" + line);
                            if (!ParseRateLine(line, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            rates4.Euribor[j] = rate;

                            line = streamReader.ReadLine();
                            Debug.WriteLine(">" + line);
                            if (!ParseRateLine(line, out rate))
                            {
                                Trace.TraceError(errorFormat, line, pattern2);
                                return list;
                            }
                            rates5.Euribor[j] = rate;
                        }
                        list.Add(rates5);
                        list.Add(rates4);
                        list.Add(rates3);
                        list.Add(rates2);
                        list.Add(rates1);
                        return list;
                    }
                    line = streamReader.ReadLine();
                }
            }
            return list;
        }

        private static bool ParseDateLine(string line, out string date)
        {
            date = null;
            // [<th class="text-right">1/2/2020</th>]
            const string pattern1 = "<th class=\"text-right\">";
            const string pattern2 = "</th>";

            int i = line.IndexOf(pattern1, StringComparison.Ordinal);
            if (i < 0)
                return false;

            line = line.Substring(i + pattern1.Length);
            i = line.IndexOf(pattern2, StringComparison.Ordinal);
            if (i < 0)
                return false;
            date = line.Substring(0, i);

            return true;
        }

        private static bool ParseRateLine(string line, out string rate)
        {
            rate = null;
            // [<td class="text-right">-0.507 %</td>]
            const string pattern1 = "<td class=\"text-right\">";
            const string pattern2 = "%</td>";

            int i = line.IndexOf(pattern1, StringComparison.Ordinal);
            if (i < 0)
                return false;

            line = line.Substring(i + pattern1.Length);
            i = line.IndexOf(pattern2, StringComparison.Ordinal);
            if (i < 0)
                return false;
            rate = line.Substring(0, i).Trim(' ');

            return true;
        }

        static void Main()
        {
            Repository repository = null;
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            Repository.InterceptErrorStack();
            Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
            Trace.TraceInformation("=======================================================================================");
            Trace.TraceInformation("Started: {0}", DateTime.Now);
            try
            {
                string str = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(str);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache);
                IEnumerable<Rates> list = Fetch();
                //list.Reverse();// Already ordered chronologically in Fetch().
                foreach (var r in list)
                {
                    Trace.TraceInformation("Rate: " + r.Dump);
                    //if (r.IsGood)
                    //{
                        for (int i = 0; i < 5; i++)
                        {
                            if (null == r.Euribor[i])
                                continue;
                            ScalarData scalarData;
                            if (!dataDictionary.TryGetValue(nameList[i], out scalarData))
                            {
                                Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, nameList[i]), true);
                                scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                                instrumentDictionary.Add(nameList[i], instrument);
                                dataDictionary.Add(nameList[i], scalarData);
                            }
                            if (null != scalarData)
                            {
                                scalar.dateTimeTicks = r.DateTime.Ticks;
                                scalar.value = double.Parse(r.Euribor[i], CultureInfo.InvariantCulture);
                                scalarList.Clear();
                                scalarList.Add(scalar);
                                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                            }
                        }
                    //}
                    //else
                    //    Trace.TraceError("Bad rate: " + r.Dump);
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
