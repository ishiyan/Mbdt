using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using Mbh5;

namespace EurepoUpdate
{
    static class Program
    {
        private class Rates
        {
            internal DateTime DateTime;
            internal readonly List<string> Eurepo = new List<string>(10);
            internal Rates()
            {
                for (int i = 0; i < 10; i++)
                    Eurepo.Add(null);
            }
            internal bool IsGood
            {
                get
                {
                    if (2 != (DateTime.Year / 1000))
                        return false;
                    return Eurepo.All(s => !string.IsNullOrEmpty(s));
                }
            }
            internal string Dump
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("[{0}-{1}-{2}:", DateTime.Year, DateTime.Month, DateTime.Day);
                    sb.AppendFormat(" tn({0})", Eurepo[0]);
                    sb.AppendFormat(" 1w({0})", Eurepo[1]);
                    sb.AppendFormat(" 2w({0})", Eurepo[2]);
                    sb.AppendFormat(" 3w({0})", Eurepo[3]);
                    sb.AppendFormat(" 1m({0})", Eurepo[4]);
                    sb.AppendFormat(" 2m({0})", Eurepo[5]);
                    sb.AppendFormat(" 3m({0})", Eurepo[6]);
                    sb.AppendFormat(" 6m({0})", Eurepo[7]);
                    sb.AppendFormat(" 9m({0})", Eurepo[8]);
                    sb.AppendFormat(" 12m({0})", Eurepo[9]);
                    return sb.ToString();
                }
            }

        }

        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();
        private static readonly List<string> nameList = NameList();
        private static List<string> NameList()
        {
            var list = new List<string>(10) {"EUREPOTN", "EUREPO1W", "EUREPO2W", "EUREPO3W", "EUREPO1M", "EUREPO2M", "EUREPO3M", "EUREPO6M", "EUREPO9M", "EUREPO12M"};
            return list;
        }

        private static List<Rates> Parse(Stream stream)
        {
            var list = new List<Rates>();
            Rates ratesToday = new Rates(), ratesPrevious = new Rates();
            list.Add(ratesPrevious);
            list.Add(ratesToday);
            const string errorFormat = "unexpected line [{0}] failed to find [{1}], aborting";
            const string errorFormat2 = "end-of-stream reached: failed to find a line with [{0}] pattern, aborting";
            using (var streamReader = new StreamReader(stream))
            {
                const string pattern1 = "<strong>TN</strong>"; // <td style="background-color: rgb(226, 223, 220); padding-left: 5px;"><strong>TN</strong><br></td>
                const string pattern2 = "<strong></strong><br/>";
                const int pattern2Len = 22;
                const string pattern3 = "<br/>";
                const int pattern3Len = 5;
                const string pattern4 = "(";
                const int pattern4Len = 1;
                const string pattern5 = ")";
                const string dateFormat = "dd/MM/yyyy"; // 13/10/2010
                string line = streamReader.ReadLine();
                while (null != line)
                {
                    int i = line.IndexOf(pattern1, StringComparison.Ordinal);
                    if (-1 < i)
                    {
                        // ---------- TN ----------
                        streamReader.ReadLine(); // [<td>]
                        line = streamReader.ReadLine(); // [{tab}<strong></strong><br/>0.68<br/> (13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        Debug.WriteLine(">" + line);
                        if (null == line || 0 > (i = line.IndexOf(pattern2, StringComparison.Ordinal))) // [<strong></strong><b/r>]
                        {
                            Trace.TraceError(errorFormat, line, pattern2);
                            return list;
                        }
                        line = line.Substring(i + pattern2Len); // [0.68<br/> (13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        int j = line.IndexOf(pattern3, StringComparison.Ordinal);
                        if (0 > j)
                        {
                            Trace.TraceError(errorFormat, line, pattern3);
                            return list;
                        }
                        ratesToday.Eurepo[0] = line.Substring(0, j); // [0.68]
                        line = line.Substring(j + pattern3Len); // [ (13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        i = line.IndexOf(pattern4, StringComparison.Ordinal); // [(]
                        if (0 > i)
                        {
                            Trace.TraceError(errorFormat, line, pattern4);
                            return list;
                        }
                        line = line.Substring(i + pattern4Len); // [13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        j = line.IndexOf(pattern5, StringComparison.Ordinal); // [)]
                        if (0 > j)
                        {
                            Trace.TraceError(errorFormat, line, pattern5);
                            return list;
                        }
                        ratesToday.DateTime = DateTime.ParseExact(line.Substring(0, j), dateFormat, CultureInfo.InvariantCulture); // [13/10/2010]
                        Debug.WriteLine("<" + ratesToday.Eurepo[0] + ", " + line.Substring(0, j));
                        while (null != (line = streamReader.ReadLine()))
                        {
                            i = line.IndexOf(pattern2, StringComparison.Ordinal); // [<strong></strong><br/>]
                            if (0 <= i)
                                break;
                        }
                        if (null == line)
                        {
                            Trace.TraceError(errorFormat2, pattern2);
                            return list;
                        }
                        // [<strong></strong><br/>0.712<br/> (12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        line = line.Substring(i + pattern2Len); // [0.712<br/> (12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        j = line.IndexOf(pattern3, StringComparison.Ordinal); // [<br/>]
                        if (0 > j)
                        {
                            Trace.TraceError(errorFormat, line, pattern3);
                            return list;
                        }
                        ratesPrevious.Eurepo[0] = line.Substring(0, j); // [0.712]
                        line = line.Substring(j + pattern3Len); // [ (12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        i = line.IndexOf(pattern4, StringComparison.Ordinal); // [(]
                        if (0 > i)
                        {
                            Trace.TraceError(errorFormat, line, pattern4);
                            return list;
                        }
                        line = line.Substring(i + pattern4Len); // [12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                        j = line.IndexOf(pattern5, StringComparison.Ordinal); // [)]
                        if (0 > j)
                        {
                            Trace.TraceError(errorFormat, line, pattern5);
                            return list;
                        }
                        ratesPrevious.DateTime = DateTime.ParseExact(line.Substring(0, j), dateFormat, CultureInfo.InvariantCulture); // [12/10/2010]
                        Debug.WriteLine("<" + ratesPrevious.Eurepo[0] + ", " + line.Substring(0, j));

                        for (int index = 1; index < 10; index++)
                        {
                            // ---------- 1W ----------
                            while (null != (line = streamReader.ReadLine()))
                            {
                                i = line.IndexOf(pattern2, StringComparison.Ordinal); // [<strong></strong><br/>]
                                if (0 <= i)
                                    break;
                            }
                            if (null == line)
                            {
                                Trace.TraceError(errorFormat2, pattern2);
                                return list;
                            }
                            // [<strong></strong><br/>0.693<br/> (13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                            line = line.Substring(i + pattern2Len); // [0.693<br/> (13/10/2010)<script language="JavaScript" type="text/javascript">    ]
                            j = line.IndexOf(pattern3, StringComparison.Ordinal); // [<br/>]
                            if (0 > j)
                            {
                                Trace.TraceError(errorFormat, line, pattern3);
                                return list;
                            }
                            ratesToday.Eurepo[index] = line.Substring(0, j); // [0.693]
                            Debug.WriteLine("<" + ratesToday.Eurepo[index]);
                            while (null != (line = streamReader.ReadLine()))
                            {
                                i = line.IndexOf(pattern2, StringComparison.Ordinal); // [<strong></strong><br/>]
                                if (0 <= i)
                                    break;
                            }
                            if (null == line)
                            {
                                Trace.TraceError(errorFormat2, pattern2);
                                return list;
                            }
                            // [<strong></strong><br/>0.734<br/> (12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                            line = line.Substring(i + pattern2Len); // [0.734<br/> (12/10/2010)<script language="JavaScript" type="text/javascript">    ]
                            j = line.IndexOf(pattern3, StringComparison.Ordinal); // [<br/>]
                            if (0 > j)
                            {
                                Trace.TraceError(errorFormat, line, pattern3);
                                return list;
                            }
                            ratesPrevious.Eurepo[index] = line.Substring(0, j); // [0.734]
                            Debug.WriteLine("<" + ratesToday.Eurepo[index]);
                        }
                        return list;
                    }
                    line = streamReader.ReadLine();
                }
            }
            return list;
        }

        private static List<Rates> Fetch()
        {
            const string url = "http://www.euribor-ebf.eu/eurepo-org/eurepo-rates.html";
            var list = new List<Rates>();
            //CookieContainer cookieCointainer = new CookieContainer();
            Trace.TraceInformation("Downloading URL " + url);
            HttpWebRequest.DefaultMaximumErrorResponseLength = 1048576;
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.CookieContainer = cookieCointainer;
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            // DefaultCredentials represents the system credentials for the current
            // security context in which the application is running. For a client-side
            // application, these are usually the Windows credentials
            // (user name, password, and domain) of the user running the application.
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            webRequest.Referer = url;
            webRequest.Referer = "http://www.euribor-ebf.eu/eurepo-org/about-eurepo.html";
            //webRequest.AllowAutoRedirect = true;
            //webRequest.KeepAlive = true;
            //webRequest.Method = "GET";
            //webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            //webRequest.BeginGetResponse.ProtocolVersion = HttpVersion.Version11;
            try
            {
                var r = (HttpWebResponse)webRequest.GetResponse();
                list = Parse(r.GetResponseStream());
            }
            catch (WebException ex)
            {
                string responseFromServer = ex.Message + " ";
                var r = (HttpWebResponse)ex.Response;
                if (r != null)
                {
                    //if (ex.Status == WebExceptionStatus.ProtocolError)
                    //{
                    //    responseFromServer += string.Format("status: code [{0}] description [{1}] ", r.StatusCode, r.StatusDescription);
                    //}
                    list = Parse(r.GetResponseStream());
                    //using (StreamReader reader = new StreamReader(r.GetResponseStream()))
                    //{
                    //    responseFromServer += reader.ReadToEnd();
                    //}
                }
                Trace.TraceError("Web exception: [{0}]", responseFromServer);
            }
            return list;
        }

        static void Main()
        {
            Repository repository = null;
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            ScalarData scalarData = null;
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
                List<Rates> list = Fetch();
                //list.Reverse();// Already ordered chronologically in Fetch().
                for (int i = 0; i < 10; i++)
                {
                    scalarList.Clear();
                    foreach (var r in list)
                    {
                        if (r.IsGood)
                        {
                            if (!dataDictionary.TryGetValue(nameList[i], out scalarData))
                            {
                                Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, nameList[i]), true);
                                // set hdf5 comment here???
                                scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                                instrumentDictionary.Add(nameList[i], instrument);
                                dataDictionary.Add(nameList[i], scalarData);
                            }
                            if (null != scalarData)
                            {
                                scalar.dateTimeTicks = r.DateTime.Ticks;
                                scalar.value = double.Parse(r.Eurepo[i], CultureInfo.InvariantCulture);
                                scalarList.Add(scalar);
                            }
                        }
                        else
                            Trace.TraceError("Bad rate: " + r.Dump);
                    }
                    if (null != scalarData)
                        scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
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
