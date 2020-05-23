using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using Mbh5;

namespace LiborFred2Update
{
    static class Program
    {
        private static readonly Dictionary<string, ScalarData> dataDictionary = new Dictionary<string, ScalarData>();
        private static readonly Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();

        private static DateTime ParseDate(string input)
        {
            string[] a = input.Split(' ');
            int day = int.Parse(a[1]);
            int year = int.Parse(a[2]);
            int month;
            string m = a[0].ToLowerInvariant();
            if (m.StartsWith("jan"))
                month = 1;
            else if (m.StartsWith("feb"))
                month = 2;
            else if (m.StartsWith("mar"))
                month = 3;
            else if (m.StartsWith("apr"))
                month = 4;
            else if (m.StartsWith("may"))
                month = 5;
            else if (m.StartsWith("jun"))
                month = 6;
            else if (m.StartsWith("jul"))
                month = 7;
            else if (m.StartsWith("aug"))
                month = 8;
            else if (m.StartsWith("sep"))
                month = 9;
            else if (m.StartsWith("oct"))
                month = 10;
            else if (m.StartsWith("nov"))
                month = 11;
            else //if (m.StartsWith("dec"))
                month = 12;
            return new DateTime(year, month, day, 0, 0, 0);
        }

        private static Dictionary<string, SortedDictionary<DateTime, double>> Fetch1()
        {
            const string errorFormat = "unexpected line [{0}] failed to find [{1}], aborting";
            const int maturityCount = 15, currencyCount = 10;
            var maturityFormats = new[] {
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-overnight.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-1-week.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-2-weeks.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-1-month.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-2-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-3-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-4-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-5-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-6-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-7-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-8-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-9-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-10-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-11-months.aspx",
                "http://www.global-rates.com/interest-rates/libor/{0}/{1}-libor-interest-rate-12-months.aspx"
            };
            var maturitySuffices = new[] {
                "on", "1w", "2w", "1m", "2m", "3m", "4m", "5m",
                "6m", "7m", "8m", "9m", "10m", "11m", "12m"
            };
            var currencies = new[] { "AUD", "CAD", "CHF", "DKK", "EUR", "GBP", "JPY", "NZD", "SEK", "USD" };
            var currenciesSmall = new[] { "aud", "cad", "chf", "dkk", "eur", "gbp", "jpy", "nzd", "sek", "usd" };
            var currenciesVerb = new[] { "australian-dollar", "canadian-dollar", "swiss-franc", "danish-krone", "european-euro", "british-pound-sterling", "japanese-yen", "new-zealand-dollar", "swedish-krona", "american-dollar" };
#if FOO
            var tableMaturities = new[] {
                "overnight",
                "1 week",
                "2 weeks",
                "1 month",
                "2 months",
                "3 months",
                "4 months",
                "5 months",
                "6 months",
                "7 months",
                "8 months",
                "9 months",
                "10 months",
                "11 months",
                "12 months",
            };
#endif
            const string header = "<tr class=\"tabledata";
            const string prefix1 = "<td>&nbsp;";
            const int prefix1Length = 10;
            const string prefix2 = "<td align=\"center\">";
            const int prefix2Length = 19;
            const string suffix1 = "</td>";
            const string suffix2 = "&nbsp;%</td>";
            var maturityDictionary =
                new Dictionary<string,SortedDictionary<DateTime,double>>();
            for (int j = 0; j < currencyCount; j++)
            {
                for (int i = 0; i < maturityCount; i++)
                {
                    string name = string.Concat(currencies[j], maturitySuffices[i]);
                    string url = string.Format(maturityFormats[i], currenciesVerb[j], currenciesSmall[j]);
                    var dictionary = new SortedDictionary<DateTime, double>();
                    maturityDictionary.Add(name, dictionary);
                    Trace.TraceInformation("Downloading URL " + url);
                    var webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current
                    // security context in which the application is running. For a client-side
                    // application, these are usually the Windows credentials
                    // (user name, password, and domain) of the user running the application.
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Timeout = 240000;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:15.0) Gecko/20100101 Firefox/15.0";
                    WebResponse webResponse = webRequest.GetResponse();
                    Stream responseStream = webResponse.GetResponseStream();
                    if (null == responseStream)
                    {
                        Trace.TraceError("Received null response stream.");
                        continue;
                    }
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        int m=12;
                        while (true)
                        {
                            if (m == 0)
                                break;
                            string line = streamReader.ReadLine();
                            if (null == line)
                                break;
                            // [  <tr class="tabledata1">] or [  <tr class="tabledata2">]
                            int k = line.IndexOf(header, StringComparison.Ordinal);
                            if (-1 < k)
                            {
                                // [  <td>&nbsp;october 12 2010</td>]
                                line = streamReader.ReadLine();
                                if (null == line)
                                {
                                    Trace.TraceError("Unexpected end of stream.");
                                    break;
                                }
                                Debug.WriteLine(">" + line);
                                k = line.IndexOf(prefix1, StringComparison.Ordinal); // [<td>&nbsp;]
                                if (0 > k)
                                {
                                    Trace.TraceError(errorFormat, line, prefix1);
                                    break;
                                }
                                line = line.Substring(k + prefix1Length); // [october 12 2010</td>]
                                int l = line.IndexOf(suffix1, StringComparison.Ordinal);
                                if (0 > l)
                                {
                                    Trace.TraceError(errorFormat, line, suffix1);
                                    break;
                                }
                                DateTime d = ParseDate(line.Substring(0, l));
                                Debug.WriteLine("<" + d.ToShortDateString());
                                // [  <td align="center">0.76650&nbsp;%</td>]
                                line = streamReader.ReadLine();
                                if (null == line)
                                {
                                    Trace.TraceError("Unexpected end of stream.");
                                    break;
                                }
                                Debug.WriteLine(">" + line);
                                k = line.IndexOf(prefix2, StringComparison.Ordinal); // [<td align="center">]
                                if (0 > k)
                                {
                                    Trace.TraceError(errorFormat, line, prefix2);
                                    break;
                                }
                                line = line.Substring(k + prefix2Length); // [0.76650&nbsp;%</td>]
                                l = line.IndexOf(suffix2, StringComparison.Ordinal); // [&nbsp;%</td>]
                                if (0 > l)
                                {
                                    Trace.TraceError(errorFormat, line, suffix2);
                                    break;
                                }
                                double v = double.Parse(line.Substring(0, l), NumberStyles.Any, CultureInfo.InvariantCulture);
                                Debug.WriteLine("<" + v.ToString(CultureInfo.InvariantCulture));
                                Trace.TraceInformation("Parsed [{0}, {1}]", d.ToShortDateString(), v.ToString(CultureInfo.InvariantCulture));
                                dictionary.Add(d, v);
                                m--;
                            }
                        }
                    }
                    Trace.TraceInformation("Found {0} new rates", dictionary.Count);
                }
            }
            return maturityDictionary;
        }

#if FOO
        private static Dictionary<string, SortedDictionary<DateTime, double>> Fetch2()
        {
            const string errorFormat = "unexpected line [{0}] failed to find [{1}], aborting";
            const string urlPrefix = "http://www.homefinance.nl/english/international-interest-rates/libor";
            const int maturityCount = 15, currencyCount = 10;
            var maturityFormats = new[] {
                "{0}/{1}/libor-rates-overnight-{2}.asp",
                "{0}/{1}/libor-rates-1-week-{2}.asp",
                "{0}/{1}/libor-rates-2-weeks-{2}.asp",
                "{0}/{1}/libor-rates-1-month-{2}.asp",
                "{0}/{1}/libor-rates-2-months-{2}.asp",
                "{0}/{1}/libor-rates-3-months-{2}.asp",
                "{0}/{1}/libor-rates-4-months-{2}.asp",
                "{0}/{1}/libor-rates-5-months-{2}.asp",
                "{0}/{1}/libor-rates-6-months-{2}.asp",
                "{0}/{1}/libor-rates-7-months-{2}.asp",
                "{0}/{1}/libor-rates-8-months-{2}.asp",
                "{0}/{1}/libor-rates-9-months-{2}.asp",
                "{0}/{1}/libor-rates-10-months-{2}.asp",
                "{0}/{1}/libor-rates-11-months-{2}.asp",
                "{0}/{1}/libor-rates-12-months-{2}.asp",
            };
            var maturitySuffices = new[] {
                "on", "1w", "2w", "1m", "2m", "3m", "4m", "5m",
                "6m", "7m", "8m", "9m", "10m", "11m", "12m"
            };
            var currencies = new[] { "EUR", "USD", "GBP", "AUD", "CAD", "CHF", "DKK", "JPY", "NZD", "SEK" };
            var currenciesSmall = new[] { "eur", "usd", "gbp", "aud", "cad", "chf", "dkk", "jpy", "nzd", "sek" };
            var currenciesVerb = new[] { "euro", "usdollar", "poundsterling", "australian-dollar", "canadian-dollar", "swiss-franc", "danish-krone", "japanese-yen", "new-zealand-dollar", "swedish-krona" };
            var tableTitleFormat = new[] {
                ">Euro LIBOR {0} - current rates</TD>",
                ">{0} USD LIBOR - current rates</TD>",
                ">{0} GBP LIBOR - current rates</TD>",
                ">{0} AUD LIBOR - current rates</TD>",
                ">{0} CAD LIBOR - current rates</TD>",
                ">{0} CHF LIBOR - current rates</TD>",
                ">{0} DKK LIBOR - current rates</TD>",
                ">{0} JPY LIBOR - current rates</TD>",
                ">{0} NZD LIBOR - current rates</TD>",
                ">{0} SEK LIBOR - current rates</TD>"};
            var tableMaturitiesEuro = new[] {
                "overnight",
                "1 week",
                "2 weeks",
                "1 month",
                "2 months",
                "3 months",
                "4 months",
                "5 months",
                "6 months",
                "7 months",
                "8 months",
                "9 months",
                "10 months",
                "11 months",
                "12 months",
            };
            var tableMaturities = new[] {
                "Overnight",
                "1 week",
                "2 week",
                "1 month",
                "2 month",
                "3 month",
                "4 month",
                "5 month",
                "6 month",
                "7 month",
                "8 month",
                "9 month",
                "10 month",
                "11 month",
                "12 month",
            };
            var maturityDictionary =
                new Dictionary<string, SortedDictionary<DateTime, double>>();
            for (int j = 0; j < currencyCount; j++)
            {
                for (int i = 0; i < maturityCount; i++)
                {
                    string name = string.Concat(currencies[j], maturitySuffices[i]);
                    string url = string.Format(maturityFormats[i],
                        urlPrefix, currenciesVerb[j], currenciesSmall[j]);
                    var dictionary = new SortedDictionary<DateTime, double>();
                    maturityDictionary.Add(name, dictionary);
                    string tableHeader = string.Format(tableTitleFormat[j],
                        0 == j ? tableMaturitiesEuro[i] : tableMaturities[i]);
                    Trace.TraceInformation("Downloading URL " + url);
                    var webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current
                    // security context in which the application is running. For a client-side
                    // application, these are usually the Windows credentials
                    // (user name, password, and domain) of the user running the application.
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Timeout = 240000;
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:15.0) Gecko/20100101 Firefox/15.0";
                    WebResponse webResponse = webRequest.GetResponse();
                    Stream responseStream = webResponse.GetResponseStream();
                    if (null == responseStream)
                    {
                        Trace.TraceError("Received null response stream.");
                        continue;
                    }
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        string line = streamReader.ReadLine();
                        while (null != line)
                        {
                            // <TD width="73%">1 month GBP LIBOR - current rates</TD>
                            int k = line.IndexOf(tableHeader, StringComparison.Ordinal);
                            if (-1 < k)
                            {
                                Debug.WriteLine(">" + line);
                                // <TD width="27%"><IMG src="/images/misc/IttyBittyClear.gif"></TD>
                                line = streamReader.ReadLine();
                                Debug.WriteLine(">" + line);
                                // </TR>
                                line = streamReader.ReadLine();
                                Debug.WriteLine(">" + line);
                                // <TR class='tabledata'><TD style='border:1px solid #CCCCCC;border-width:0px 1px 1px 1px;'>10-13-2009</TD><TD style='border:1px solid #CCCCCC;border-width:0px 1px 1px 0px;' align='center'>0.51000&nbsp;%</TD></TR>...
                                line = streamReader.ReadLine();
                                Debug.WriteLine(">" + line);
                            again:
                                if (null == line)
                                    break;
                                k = line.IndexOf("<TD", StringComparison.Ordinal);
                                if (0 < k)
                                {
                                    // <TD style='border:1px solid #CCCCCC;border-width:0px 1px 1px 1px;'>10-13-2009</TD><TD style=...
                                    line = line.Substring(k);
                                    k = line.IndexOf(">", StringComparison.Ordinal);
                                    if (0 > k)
                                    {
                                        Trace.TraceError(errorFormat, line, ">");
                                        break;
                                    }
                                    // >10-13-2009</TD><TD style=...
                                    line = line.Substring(k + 1);
                                    // 10-13-2009</TD><TD style=...
                                    k = line.IndexOf("</TD>", StringComparison.Ordinal);
                                    if (0 > k)
                                    {
                                        Trace.TraceError(errorFormat, line, "</TD>");
                                        break;
                                    }
                                    string ds = line.Substring(0, k);
                                    Debug.WriteLine(">" + ds);
                                    line = line.Substring(k + 5);
                                    // <TD style=...
                                    k = line.IndexOf(">", StringComparison.Ordinal);
                                    if (0 > k)
                                    {
                                        Trace.TraceError(errorFormat, line, ">");
                                        break;
                                    }
                                    // >0.51000&nbsp;%</TD></TR>...
                                    line = line.Substring(k + 1);
                                    // 0.51000&nbsp;%</TD></TR>...
                                    k = line.IndexOf("&nbsp;%</TD>", StringComparison.Ordinal);
                                    if (0 > k)
                                    {
                                        Trace.TraceError(errorFormat, line, "&nbsp;%</TD>");
                                        break;
                                    }
                                    string vs = line.Substring(0, k);
                                    Debug.WriteLine(">" + vs);
                                    line = line.Substring(k + 12);
                                    DateTime d = DateTime.ParseExact(ds, "MM-dd-yyyy", CultureInfo.InvariantCulture);
                                    double v = double.Parse(vs, NumberStyles.Any, CultureInfo.InvariantCulture);
                                    Trace.TraceInformation("Parsed [{0}, {1}]", ds, vs);
                                    dictionary.Add(d, v);
                                    goto again;
                                }
                                break;
                            }
                            line = streamReader.ReadLine();
                        }
                    }
                    Trace.TraceInformation("Found {0} new rates", dictionary.Count);
                }
            }
            return maturityDictionary;
        }
#endif
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
                if (string.IsNullOrEmpty(directoryName))
                {
                    Trace.TraceError("Cannot locate the deirectory of the repository file [{0}]", str);
                    return;
                }
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                repository = Repository.OpenReadWrite(str, true, Properties.Settings.Default.Hdf5CorkTheCache);
                Dictionary<string, SortedDictionary<DateTime, double>> newRates = Fetch1();
                foreach (var kvp in newRates)
                {
                    string name = kvp.Key; SortedDictionary<DateTime, double> rate = kvp.Value;
                    scalarList.Clear();
                    ScalarData scalarData;
                    if (!dataDictionary.TryGetValue(name, out scalarData))
                    {
                        Instrument instrument = repository.Open(string.Concat(Properties.Settings.Default.RepositoryRoot, name), true);
                        // set hdf5 comment here???
                        scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                        instrumentDictionary.Add(name, instrument);
                        dataDictionary.Add(name, scalarData);
                    }
                    if (null != scalarData)
                    {
                        foreach (var drp in rate)
                        {
                            scalar.dateTimeTicks = drp.Key.Ticks;
                            scalar.value = drp.Value;
                            scalarList.Add(scalar);
                        }
                        scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                    }
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
