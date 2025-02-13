﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;
using Mbh5;

namespace EstrUpdate
{
    static class Program
    {
        private class Rate
        {
            internal DateTime DateTime;
            internal double Estr;
        }

        private static List<Rate> Fetch()
        {
            const string url = "https://www.ecb.europa.eu/stats/financial_markets_and_interest_rates/euro_short-term_rate/html/index.en.html";
            var list = new List<Rate>(1);
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
                const string pattern1 = "<th><strong>Rate</strong></th>";
                const string pattern2 = "<td><strong>";
                const string pattern3 = "</strong></td>";
                const string pattern4 = "<td>";
                const string pattern5 = "</td>";
                string line = streamReader.ReadLine();
                while (null != line)
                {
                    int i = line.IndexOf(pattern1, StringComparison.Ordinal); // <strong>Rate</strong>
                    if (-1 < i)
                    {
                        Trace.TraceInformation(">" + line);
                        line = streamReader.ReadLine();
                        if (line == null)
                        {
                            line = string.Empty;
                        }
                        Trace.TraceInformation(">" + line);
                        i = line.IndexOf(pattern2, StringComparison.Ordinal);
                        if (i < 0)
                        {
                            Trace.TraceError(errorFormat, line, pattern2);
                            return list;
                        }
                        string rate = line.Substring(i + pattern2.Length);
                        i = rate.IndexOf(pattern3, StringComparison.Ordinal);
                        if (i < 0)
                        {
                            Trace.TraceError(errorFormat, rate, pattern3);
                            return list;
                        }
                        rate = rate.Substring(0, i);
                        rate = rate.Replace(",", ".");
                        Trace.TraceInformation(">" + rate);

                        line = streamReader.ReadLine(); // </tr>
                        if (null == line)
                        {
                            Trace.TraceError("line is null");
                            return list;
                        }
                        line = streamReader.ReadLine(); // <tr>
                        if (null == line)
                        {
                            Trace.TraceError("line is null");
                            return list;
                        }
                        line = streamReader.ReadLine(); // <th>Reference date</th>
                        if (null == line)
                        {
                            Trace.TraceError("line is null");
                            return list;
                        }
                        line = streamReader.ReadLine(); // <td>12-04-2021</td>
                        Trace.TraceInformation(">" + line);
                        i = line.IndexOf(pattern4, StringComparison.Ordinal);
                        if (i < 0)
                        {
                            Trace.TraceError(errorFormat, line, pattern4);
                            return list;
                        }
                        string date = line.Substring(i + pattern4.Length);
                        Trace.TraceInformation(">" + date);
                        i = date.IndexOf(pattern5, StringComparison.Ordinal);
                        if (i < 0)
                        {
                            Trace.TraceError(errorFormat, date, pattern5);
                            return list;
                        }
                        date = date.Substring(0, i);
                        Trace.TraceInformation(">" + date);

                        var r = new Rate
                        {
                            DateTime = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            Estr = double.Parse(rate, CultureInfo.InvariantCulture)
                        };

                        var sb = new StringBuilder();
                        sb.AppendFormat("[{0}-{1}-{2}: {3}]", r.DateTime.Year, r.DateTime.Month, r.DateTime.Day, r.Estr);
                        Trace.TraceInformation(sb.ToString());

                        list.Add(r);
                        return list;
                    }
                    line = streamReader.ReadLine();
                }
            }
            Trace.TraceError("No expected patterns found. Has page format been changed?");
            return list;
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
            List<Rate> list = Fetch();
            if (list.Count > 0)
            {
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
                    scalarList.Clear();
                    foreach (var r in list)
                    {
                        scalar.dateTimeTicks = r.DateTime.Ticks;
                        scalar.value = r.Estr;
                        scalarList.Add(scalar);
                    }
                    scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception: [{0}]", e.Message);
                }
                finally
                {
                    scalarData?.Flush();
                    scalarData?.Close();
                    instrument?.Close();
                    repository?.Close();
                }
                Trace.TraceInformation("Finished: {0}", DateTime.Now);
            }
        }
    }
}
