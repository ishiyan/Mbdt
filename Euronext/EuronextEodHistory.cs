using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;
using System.Threading;

using mbdt.Utils;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext endofday history utilities.
    /// </summary>
    class EuronextEodHistory
    {
        #region Quote
        private class Quote
        {
            #region Members and accessors
            #region Line
            /// <summary>
            /// The quote string in xml form.
            /// </summary>
            public string Line;
            #endregion

            #region Jdn
            /// <summary>
            /// The Julian day number of this quote.
            /// </summary>
            public int Jdn;
            #endregion
            #endregion

            #region Constructor
            /// <summary>
            /// Constructs a new instance of the class.
            /// </summary>
            /// <param name="line">The quote string in xml form.</param>
            /// <param name="jdn">The Julian day number of this quote.</param>
            public Quote(string line, int jdn)
            {
                Line = line;
                Jdn = jdn;
            }
            #endregion
        }
        #endregion

        #region Constants
        private const string instrumentFormat = "<instrument vendor=\"Euronext\" isin=\"{0}\" mep=\"{1}\" name=\"{2}\" symbol=\"{3}\">";
        private const string quoteFormat = "<q c=\"{0}\" d=\"{1}\" h=\"{2}\" j=\"{3}\" l=\"{4}\" o=\"{5}\" v=\"{6}\"/>";
        private const string quoteEnd = "</q>";
        private const string instrumentEnd = "</instrument>";
        private const string instrumentsBegin = "<instruments>";
        private const string instrumentsEnd = "</instruments>";
        private const string endofdayBegin = "<endofday>";
        private const string endofdayEnd = "</endofday>";
        private const string endofday = "endofday";
        private const string history = "history";
        #endregion

        #region SplitLine
        private static string[] SplitLine(string line)
        {
            line = line.Replace("Â ", "");
            line = line.Replace("Â", "");
            line = line.Replace(" ", "");
            line = line.Replace("á", "");
            line = line.Replace(",", "");
            return line.Split(';');
        }
        #endregion

        #region Extract
        private static string Extract(string line, int fromIndex)
        {
            char[] chars = line.ToCharArray(fromIndex, line.Length - fromIndex);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in chars)
            {
                if ('<' == c)
                    break;
                else
                    stringBuilder.Append(c);
            }
            line = stringBuilder.ToString();
            line = line.Replace("á", "");
            line = line.Replace(" ", "");
            line = line.Replace(" ", "");
            return line;
        }
        #endregion

        #region Convert
        private static double Convert(string s, string name, int lineNumber, string line, EuronextInstrumentContext context)
        {
            if (string.IsNullOrEmpty(s) || "-" == s)
                return 0;
            else
            {
                double value;
                try
                {
                    value = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                }
                catch (Exception)
                {
                    Trace.TraceError("invalid endofday history csv {0}, line {1} [{2}] file {3}", name, lineNumber, line, Path.GetFileName(context.DownloadedPath));
                    value = 0;
                }
                return value;
            }
        }
        #endregion

        #region PickOne
        private static double PickOne(double value1, double value2, double value3)
        {
            return (0 != value1 ? value1 : (0 != value2 ? value2 : (0 != value3 ? value3 : 0)));
        }
        #endregion

        #region ImportCsv
        /// <summary>
        /// Imports a downloded Euronext endofday history csv file into a list containing quote strings in XML format.
        /// </summary>
        /// <param name="context">A EuronextInstrumentContext.</param>
        /// <returns>A list containing imported quote strings in XML format.</returns>
        private static List<Quote> ImportCsv(EuronextInstrumentContext context)
        {
            List<Quote> quoteList = new List<Quote>(1024);
            using (StreamReader csvStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, date;
                const string errorFormat = "invalid endofday history csv{0}, line {1} [{2}] file {3}, skipping";
                string[] splitted;
                int lineNumber = 4, jdn;
                double open, high, low, close, volume;
                csvStreamReader.ReadLine();        // ICompany name;ISIN;MEP;Symbol;Segment;Date
                csvStreamReader.ReadLine();        // AP ALTERNAT ASSETS;GB00B15Y0C52;AMS;AAA;-;03/07/08 16:09 CET
                csvStreamReader.ReadLine();        // Empty line
                line = csvStreamReader.ReadLine(); // Date;opening;High;Low;closing;Volume03/05/08;13.00;13.00;13.00;13.00;1000
                                                   // 03/06/08;12.89;13.50;12.87;13.50;346120
                const string pattern = "Date;opening;High;Low;closing;Volume";
                if (!string.IsNullOrEmpty(line) && line.StartsWith(pattern) && !string.IsNullOrEmpty((line = line.Substring(pattern.Length))))
                {
                    do
                    {
                        splitted = SplitLine(line);
                        if (6 == splitted.Length)
                        {
                            jdn = JulianDayNumber.FromMMsDDsYY(splitted[0]);
                            date = JulianDayNumber.ToYYYYMMDD(jdn);
                            open = Convert(splitted[1], "open", lineNumber, line, context);
                            high = Convert(splitted[2], "high", lineNumber, line, context);
                            low = Convert(splitted[3], "low", lineNumber, line, context);
                            close = Convert(splitted[4], "close", lineNumber, line, context);
                            volume = Convert(splitted[5], "volume", lineNumber, line, context);
                            if (0 == open)
                                open = PickOne(close, high, low);
                            if (0 == close)
                                close = PickOne(open, high, low);
                            if (0 == high)
                                high = PickOne(open, close, low);
                            if (0 == low)
                                low = PickOne(open, close, high);
                            quoteList.Add(new Quote(string.Format(quoteFormat,
                                close.ToString(CultureInfo.InvariantCulture.NumberFormat), date,
                                high.ToString(CultureInfo.InvariantCulture.NumberFormat), jdn,
                                low.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                open.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                volume.ToString(CultureInfo.InvariantCulture.NumberFormat)), jdn));
                        }
                        else
                            Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvStreamReader.ReadLine()));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (1 > quoteList.Count)
            {
                Trace.TraceError("no historical data found in csv file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return quoteList;
        }
        #endregion

        #region ImportCsvh
        /// <summary>
        /// Imports a downloded Euronext endofday history csv file into a list containing quote strings in XML format.
        /// </summary>
        /// <param name="context">A EuronextInstrumentContext.</param>
        /// <returns>A list containing imported quote strings in XML format.</returns>
        private static List<Quote> ImportCsvh(EuronextInstrumentContext context)
        {
            List<Quote> quoteList = new List<Quote>(1024);
            using (StreamReader csvhStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, date;
                const string errorFormat = "invalid endofday history csvh, line {0} [{1}] file {2}, skipping";
                string[] splitted;
                int lineNumber = 1, jdn;
                double open, high, low, close, volume;
                // dd/mm/yy;open;high;low;close;volume
                line = csvhStreamReader.ReadLine(); // ´╗┐16/10/06;27.21;27.91;27.16;27.61;12288608
                if (null != line)
                {
                    //line = line.Replace("´╗┐", "");
                    do
                    {
                        splitted = SplitLine(line);
                        if (6 == splitted.Length)
                        {
                            jdn = JulianDayNumber.FromMMsDDsYY(splitted[0]);
                            date = JulianDayNumber.ToYYYYMMDD(jdn);
                            open = Convert(splitted[1], "open", lineNumber, line, context);
                            high = Convert(splitted[2], "high", lineNumber, line, context);
                            low = Convert(splitted[3], "low", lineNumber, line, context);
                            close = Convert(splitted[4], "close", lineNumber, line, context);
                            volume = Convert(splitted[5], "volume", lineNumber, line, context);
                            if (0 == open)
                                open = PickOne(close, high, low);
                            if (0 == close)
                                close = PickOne(open, high, low);
                            if (0 == high)
                                high = PickOne(open, close, low);
                            if (0 == low)
                                low = PickOne(open, close, high);
                            quoteList.Add(new Quote(string.Format(quoteFormat,
                                close.ToString(CultureInfo.InvariantCulture.NumberFormat), date,
                                high.ToString(CultureInfo.InvariantCulture.NumberFormat), jdn,
                                low.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                open.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                volume.ToString(CultureInfo.InvariantCulture.NumberFormat)), jdn));
                        }
                        else
                            Trace.TraceError(errorFormat, lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvhStreamReader.ReadLine()));
                }
                else
                    Trace.TraceError("no endofday historical data found in csvh file {0}, skipping", Path.GetFileName(context.DownloadedPath));
            }
            return quoteList;
        }
        #endregion

        #region ParseJulianDayNumber
        private static int ParseJulianDayNumber(string line)
        {
            int jdn = 0, j = line.IndexOf(" j=\"") + 4;
            char c = line[j++];
            while ('0' <= c && c <= '9')
            {
                jdn *= 10;
                jdn += c - '0';
                c = line[j++];
            }
            return jdn;
        }
        #endregion

        #region DownloadHtml
        /// <summary>
        /// Downloads sequence of html pages extracting intraday quotes end writing them to a file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="filePath">The file name to write quotes to.</param>
        /// <param name="minimalLength">A minimal length of the file in bytes.</param>
        /// <param name="overwrite">If the file already exists, overwrite it.</param>
        /// <param name="retries">The number of download retries.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>True if download was successful.</returns>
        private static bool DownloadHtml(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout)
        {
            Debug.WriteLine(string.Concat("downloading endofday history html ", filePath, " from ", uri));
            FileInfo fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            if (!overwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > minimalLength)
                    {
                        Trace.TraceWarning("file {0} already exists, skipping", Path.GetFileName(filePath));
                        return true;
                    }
                    Trace.TraceWarning("file {0} already exists but length {1} is smaller than the minimal length {2}, overwriting", Path.GetFileName(filePath), fileInfo.Length, minimalLength);
                }
            }
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            const string pattern1 = "<td class=\"tableDateStamp\" style=\"white-space: nowrap\">";
            int pattern1Length = pattern1.Length;
            const string pattern2 = "&nbsp;";
            int pattern2Length = pattern2.Length;
            const string pattern3 = "<td>";
            int pattern3Length = pattern3.Length;
            const string pattern4 = "</td>";
            int pattern4Length = pattern4.Length;
            bool downloaded = false, found = false;
            int i, page = 0;
            string line, date, open, high, low, close, volume;
            StreamReader streamReader = null;
            StreamWriter streamWriter = null;
            while (0 < retries)
            {
                try
                {
                    WebRequest webRequest = HttpWebRequest.Create(uri);
                    //webRequest.Headers.Set(HttpRequestHeader.UserAgent, "foobar");
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current 
                    // instrument context in which the application is running. For a client-side 
                    // application, these are usually the Windows credentials 
                    // (user name, password, and domain) of the user running the application. 
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Timeout = timeout;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
                    // Skip validation of SSL/TLS certificate
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;
                    page = 1;
                    streamWriter = new StreamWriter(filePath, false, Encoding.UTF8, bufferSize);
                    streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                    found = false;
                    line = streamReader.ReadLine();
                    while (null != line)
                    {
                        i = line.IndexOf(pattern1);
                        if (-1 < i)
                        {
                            //  <tr>
                            //    <td class="tableHeader" >Date</td>
                            //    <td class="tableHeader" >opening</td>
                            //    <td class="tableHeader" >High</td>
                            //    <td class="tableHeader" >Low</td>
                            //    <td class="tableHeader" >closing</td>
                            //    <td class="tableHeader" >Volume</td>
                            //..</tr>
                            //  <tr class=bgColor7>
                            //    <td class="tableDateStamp" style="white-space: nowrap">16/10/06</td>
                            //    <td class="tableDateStamp" style="white-space: nowrap">27.21</td>
                            //    <td>27.91</td>
                            //    <td>27.16</td>
                            //    <td>27.61</td>
                            //    <td>12 288 608</td>
                            //  </tr>
                            //  <tr class=bgColor1>
                            //    <td class="tableDateStamp" style="white-space: nowrap">17/10/06</td>
                            found = true;
                            i += pattern1Length;
                            date = line.Substring(i, 8);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern1) + pattern1Length;
                            open = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            high = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            low = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            close = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            volume = Extract(line, i);
                            streamWriter.WriteLine("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume);
                            Debug.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume));
                        }
                        line = streamReader.ReadLine();
                    }
                    while (found)
                    {
                        page++;
                        streamReader.Close();
                        streamReader.Dispose();
                        line = string.Concat(uri, "&pageIndex=", page.ToString());
                        Debug.WriteLine(string.Concat("downloading endofday history html page ", page, " from ", line));
                        webRequest = HttpWebRequest.Create(line);
                        webRequest.Proxy = WebRequest.DefaultWebProxy;
                        webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                        webRequest.Timeout = timeout;
                        streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                        found = false;
                        line = streamReader.ReadLine();
                        while (null != line)
                        {
                            i = line.IndexOf(pattern1);
                            if (-1 < i)
                            {
                                found = true;
                                i += pattern1Length;
                                date = line.Substring(i, 8);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern1) + pattern1Length;
                                open = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                high = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                low = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                close = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                volume = Extract(line, i);
                                streamWriter.WriteLine("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume);
                                Debug.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume));
                            }
                            line = streamReader.ReadLine();
                        }
                    }
                    streamReader.Close();
                    streamWriter.Close();
                    streamReader.Dispose();
                    streamWriter.Dispose();
                    fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        if (fileInfo.Length > minimalLength)
                        {
                            downloaded = true;
                            retries = 0;
                        }
                        else
                        {
                            if (1 < retries)
                                Trace.TraceError("endofday history html file {0}: downloaded length {1} is smaller than the minimal length {2}, retrying ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
                            else
                            {
                                Trace.TraceError("endofday history html file {0}: downloaded length {1} is smaller than the minimal length {2}, giving up ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
                                File.Delete(filePath);
                            }
                            retries--;
                        }
                    }
                    else
                        retries--;
                }
                catch (Exception e)
                {
                    if (1 < retries)
                        Trace.TraceError("endofday history html file {0} page {1}: download failed [{2}], retrying ({3})", Path.GetFileName(filePath), page, e.Message, retries);
                    else
                        Trace.TraceError("endofday history html file {0} page {1}: download failed [{2}], giving up ({3})", Path.GetFileName(filePath), page, e.Message, retries);
                    retries--;
                    if (null != streamReader)
                    {
                        streamReader.Close();
                        streamReader.Dispose();
                        streamReader = null;
                    }
                    if (null != streamWriter)
                    {
                        streamWriter.Close();
                        streamWriter.Dispose();
                        streamWriter = null;
                    }
                }
            }
            return downloaded;
        }
        #endregion

        #region Merge
        /// <summary>
        /// Merges the downloaded csv file with the repository xml file.
        /// </summary>
        /// <param name="xmlPath">The repository xml file.</param>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and a path to the csv file.</param>
        /// <returns>True if merged, false otherwise.</returns>
        public static bool Merge(string xmlPath, EuronextInstrumentContext context)
        {
            int jdn;
            List<Quote> quoteList = context.DownloadedPath.EndsWith(".csvh") ?
                ImportCsvh(context) : ImportCsv(context);
            if (null == quoteList || 0 == quoteList.Count)
                return false;
            const int bufferSize = 0x1000;
            string line, xmlPathMerged = string.Concat(xmlPath, ".merged");
            using (StreamWriter xmlStreamWriter = new StreamWriter(xmlPathMerged, false, Encoding.UTF8, bufferSize))
            {
                if (File.Exists(xmlPath))
                {
                    using (StreamReader xmlStreamReader = new StreamReader(xmlPath, Encoding.UTF8))
                    {
                        bool notMerged = true;
                        while (null != (line = xmlStreamReader.ReadLine()))
                        {
                            if (line.StartsWith("<instrument "))
                            {
                                xmlStreamWriter.WriteLine(line);
                                if (line.Contains(string.Concat(" isin=\"", context.Isin, "\"")) &&
                                    line.Contains(string.Concat(" mep=\"", context.Mep, "\"")) &&
                                    line.Contains(string.Concat(" symbol=\"", context.Symbol, "\"")))
                                {
                                    notMerged = false;
                                    while (null != (line = xmlStreamReader.ReadLine()))
                                    {
                                        if (line.StartsWith("<q "))
                                        {
                                            if (0 < quoteList.Count)
                                            {
                                                Quote quote;
                                                jdn = ParseJulianDayNumber(line);
                                                do
                                                {
                                                    quote = quoteList[0];
                                                    if (jdn < quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(line);
                                                        break;
                                                    }
                                                    else if (jdn == quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(quote.Line);
                                                        quoteList.RemoveAt(0);
                                                        break;
                                                    }
                                                    else // if (jdn > quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(quote.Line);
                                                        quoteList.RemoveAt(0);
                                                        if (0 == quoteList.Count)
                                                            xmlStreamWriter.WriteLine(line);
                                                    }
                                                } while (0 < quoteList.Count);
                                            }
                                            else
                                            {
                                                xmlStreamWriter.WriteLine(line);
                                            }
                                        }
                                        else if (line.StartsWith(endofdayEnd))
                                        {
                                            quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                                            quoteList.Clear();
                                            xmlStreamWriter.WriteLine(line);
                                            break;
                                        }
                                        else
                                            xmlStreamWriter.WriteLine(line);
                                    }
                                }
                                else // copy non-matched instrument
                                {
                                    while (null != (line = xmlStreamReader.ReadLine()))
                                    {
                                        xmlStreamWriter.WriteLine(line);
                                        if (line.StartsWith(instrumentEnd))
                                            break;
                                    }
                                }
                            }
                            else if (line.StartsWith(instrumentsEnd))
                            {
                                if (notMerged)
                                {
                                    xmlStreamWriter.WriteLine(instrumentFormat, context.Isin, context.Mep, context.Name, context.Symbol);
                                    xmlStreamWriter.WriteLine(endofdayBegin);
                                    quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                                    quoteList.Clear();
                                    xmlStreamWriter.WriteLine(endofdayEnd);
                                    xmlStreamWriter.WriteLine(instrumentEnd);
                                    notMerged = false;
                                }
                                xmlStreamWriter.WriteLine(line);
                                break;
                            }
                            else
                                xmlStreamWriter.WriteLine(line);
                        }
                    }
                }
                else
                {
                    // Just write new output stream and copy the csv endofday history data.
                    xmlStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
                    xmlStreamWriter.WriteLine(instrumentsBegin);
                    xmlStreamWriter.WriteLine(instrumentFormat, context.Isin, context.Mep, context.Name, context.Symbol);
                    xmlStreamWriter.WriteLine(endofdayBegin);
                    quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                    xmlStreamWriter.WriteLine(endofdayEnd);
                    xmlStreamWriter.WriteLine(instrumentEnd);
                    xmlStreamWriter.WriteLine(instrumentsEnd);
                }
            }
            if (File.Exists(xmlPathMerged))
            {
                if (File.Exists(xmlPath))
                    File.Replace(xmlPathMerged, xmlPath, null);
                else
                    File.Move(xmlPathMerged, xmlPath);
            }
            return true;
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads endofday history data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a traling separator.</param>
        /// <param name="allowHtml">Try to download html if csv download fails.</param>
        /// <param name="days">The number of last history days to download or 0 to download all available history data.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        public static bool Download(EuronextInstrumentContext context, string downloadDir, bool allowHtml, int days)
        {
            List<string> channels = new List<string>() { "2634", "2593", "2783", "2549", "2512" };
            Random random = new Random(DateTime.Now.Millisecond);
            string cha = channels[random.Next(0, channels.Count)];
            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoh.csv", context.Mep, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;
            bool succeded = true;
            const string requestDateFormat = "dd/MM/yyyy";
            DateTime dateTime = DateTime.Now;
            string dateTo = dateTime.ToString(requestDateFormat, DateTimeFormatInfo.InvariantInfo);
            string dateFrom = "01/01/1950";
            if (0 < days)
                dateFrom = dateTime.AddDays(-days).ToString(requestDateFormat, DateTimeFormatInfo.InvariantInfo);
            string uri = string.Format("http://www.euronext.com/tools/datacentre/dataCentreDownload.jcsv?lan=EN&quote=&time=&dayvolume=&indexCompo=&opening=on&high=on&low=on&closing=on&volume=on&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha={0}&dateFrom={1}&dateTo={2}&isinCode={3}&selectedMep={4}&isin={3}&mep={4}",
            //string uri = string.Format("http://160.92.106.167/tools/datacentre/dataCentreDownload.jcsv?lan=EN&quote=&time=&dayvolume=&indexCompo=&opening=on&high=on&low=on&closing=on&volume=on&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha={0}&dateFrom={1}&dateTo={2}&isinCode={3}&selectedMep={4}&isin={3}&mep={4}",
                cha, dateFrom, dateTo, context.Isin, Euronext.MepToInteger(context.Mep).ToString());
            if (!Downloader.Download(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, null))
            {
                if (!allowHtml)
                    return false;
                uri = string.Format("http://www.euronext.com/tools/datacentre/dataCentreDownloadHTML-{0}-EN.html?&volume=on&opening=on&low=on&high=on&closing=on&indexCompo=&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha={0}&dateFrom={1}&dateTo={2}&isinCode={3}&selectedMep={4}&isin={3}&mep={4}",
                //uri = string.Format("http://160.92.106.167/tools/datacentre/dataCentreDownloadHTML-{0}-EN.html?&volume=on&opening=on&low=on&high=on&closing=on&indexCompo=&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha={0}&dateFrom={1}&dateTo={2}&isinCode={3}&selectedMep={4}&isin={3}&mep={4}",
                    cha, dateFrom, dateTo, context.Isin, Euronext.MepToInteger(context.Mep).ToString());
                s = string.Concat(context.DownloadedPath, "h");
                context.DownloadedPath = s;
                if (!DownloadHtml(uri, s, EuronextInstrumentContext.HistoryDownloadMinimalLength, EuronextInstrumentContext.HistoryDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout))
                    succeded = false;
            }
            return succeded;
        }
        #endregion

        #region Zip
        /// <summary>
        /// Makes a zip file from a directory of downloaded files and deletes this directory.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> to get the download repository suffix and the datestamp in YYYYMMDD format.</param>
        public static void Zip(EuronextInstrumentContext context)
        {
            string directory = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, history, context.DownloadRepositorySuffix);
            string separator = Path.DirectorySeparatorChar.ToString();
            string parent = directory;
            if (directory.EndsWith(separator))
                parent = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                parent = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            parent = string.Concat(Directory.GetParent(parent).FullName, separator);
            Packager.ZipCsvDirectory(string.Concat(parent, context.Yyyymmdd, "enx_eoh.zip"), directory, true);
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs a daily update task.
        /// </summary>
        /// <param name="days">The number of history days to download.</param>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int UpdateTask(int days)
        {
            object notDownloadedListLock = new object();
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, mergedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, mergedDiscoveredInstruments = 0, discoveredInstruments = 0;
            string downloadDir = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, history);

            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved instruments: {0}", DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                    string s = string.Concat(EuronextInstrumentContext.EndofdayRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered instruments: {0}", DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                    string s = string.Concat(EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(approvedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    approvedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                            string s = string.Concat(EuronextInstrumentContext.EndofdayRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(discoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    discoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                            string s = string.Concat(EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments, mergedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments, mergedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments, mergedApprovedInstruments + mergedDiscoveredInstruments);
            Zip(context);
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region DownloadTask
        /// <summary>
        /// Performs a download task.
        /// </summary>
        /// <param name="days">The number of history days to download.</param>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int DownloadTask(string downloadPath, int days)
        {
            if (string.IsNullOrEmpty(downloadPath))
                downloadPath = "";
            else
            {
                if (!downloadPath.EndsWith(Path.DirectorySeparatorChar.ToString()) && !downloadPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    downloadPath = string.Concat(downloadPath, Path.DirectorySeparatorChar.ToString());
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
            }

            object notDownloadedListLock = new object();
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, discoveredInstruments = 0;
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved to {0}: {1}", downloadPath, DateTime.Now);
            EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, (esc, cfi) =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath, false, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered to {0}: {1}", downloadPath, DateTime.Now);
            EuronextExecutor.Iterate(discoveredList, (esc, cfi) =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (cfi.LimitReached)
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
                else if (Download(esc, downloadPath, false, days))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> approvedContextListList = EuronextExecutor.Split(approvedNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    approvedNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(approvedContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath, pass == passCount, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    List<List<EuronextInstrumentContext>> discoveredContextListList = EuronextExecutor.Split(discoveredNotDownloadedList, EuronextInstrumentContext.WorkerThreads);
                    discoveredNotDownloadedList.Clear();
                    EuronextExecutor.Iterate(discoveredContextListList, (esc, cfi) =>
                    {
                        if (cfi.LimitReached)
                        {
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                        else if (Download(esc, downloadPath, pass == passCount, days))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.HistoryDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.HistoryWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments);
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region ImportTask
        /// <summary>
        /// Performs an import task.
        /// </summary>
        /// <param name="importPath">A path to an import directory or an import file.</param>
        /// <returns>The number of orphaned instruments.</returns>
        public static int ImportTask(string importPath)
        {
            Trace.TraceInformation("Scanning {0} and {1}: {2}", EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.DiscoveredIndexPath, DateTime.Now);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryApproved = EuronextExecutor.ScanIndex(EuronextInstrumentContext.ApprovedIndexPath);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryDiscovered = EuronextExecutor.ScanIndex(EuronextInstrumentContext.DiscoveredIndexPath);
            Trace.TraceInformation("Splitting {0}: {1}", importPath, DateTime.Now);
            List<string> orphaned;
            List<List<string>> list = EuronextExecutor.Split(importPath, dictionaryApproved, dictionaryDiscovered, EuronextInstrumentContext.WorkerThreads, out orphaned);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Merging: {0}", DateTime.Now);
            long totalInstruments = 0, mergedInstruments = 0;
            string yyyymmdd = "";
            EuronextExecutor.Iterate(list, dictionaryApproved, dictionaryDiscovered, yyyymmdd, (xml, esc) =>
            {
                Interlocked.Increment(ref totalInstruments);
                EuronextInstrumentContext.VerifyFile(xml);
                if (Merge(xml, esc))
                    Interlocked.Increment(ref mergedInstruments);
            }, false);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History imported instruments: total {0}, merged {1}", totalInstruments, mergedInstruments);
            orphaned.ForEach(file => Trace.TraceInformation("Orphaned import file [{0}], skipped", file));
            return orphaned.Count;
        }
        #endregion

    }
}
