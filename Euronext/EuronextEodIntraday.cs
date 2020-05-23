using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.Serialization;

using mbdt.Utils;
using System.Xml;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext intraday utilities.
    /// </summary>
    class EuronextEodIntraday
    {
        #region Constants
        private const string instrumentFormat = "<instrument vendor=\"Euronext\" isin=\"{0}\" mep=\"{1}\" name=\"{2}\" symbol=\"{3}\">";
        private const string tickFormat = "<t p=\"{0}\" s=\"{1}\" t=\"{2}\" v=\"{3}\"/>";
        private const string quoteEnd = "</q>";
        private const string instrumentEnd = "</instrument>";
        private const string instrumentsBegin = "<instruments>";
        private const string instrumentsEnd = "</instruments>";
        private const string intradayBegin = "<intraday>";
        private const string intradayEnd = "</intraday>";
        private const string intraday = "intraday";
        #endregion

        #region IsEndQuote
        /// <summary>
        /// Checks if the line is an end-of-quote line.
        /// </summary>
        /// <param name="line">The line to check.</param>
        /// <returns>True if he line is an end-of-quote line.</returns>
        private static bool IsEndQuote(string line)
        {
            return 3 < line.Length && '<' == line[0] && '/' == line[1] && 'q' == line[2] && '>' == line[3];
        }
        #endregion

        #region SecondsFromHHsMMsSS
        /// <summary>
        /// Converts a hh:mm:ss time stamp to a number of seconds from beginning of this day.
        /// </summary>
        /// <param name="hhSmmSss">A time stamp.</param>
        /// <returns>The number of seconds or -1 in case of invalid input string.</returns>
        private static int SecondsFromHHsMMsSS(string hhSmmSss)
        {
            if (hhSmmSss.Length > 7)
            {
                char c = hhSmmSss[0];
                if ('0' <= c && c <= '9')
                {
                    int value = 10 * (c - '0');
                    c = hhSmmSss[1];
                    if ('0' <= c && c <= '9')
                    {
                        value += c - '0';
                        if (0 <= value && value < 24) // Hour
                        {
                            int second = value * 3600;
                            c = hhSmmSss[2];
                            if (':' == c)
                            {
                                c = hhSmmSss[3];
                                if ('0' <= c && c <= '9')
                                {
                                    value = 10 * (c - '0');
                                    c = hhSmmSss[4];
                                    if ('0' <= c && c <= '9')
                                    {
                                        value += c - '0';
                                        if (0 <= value && value < 60) // Minute
                                        {
                                            second += value * 60;
                                            c = hhSmmSss[5];
                                            if (':' == c)
                                            {
                                                c = hhSmmSss[6];
                                                if ('0' <= c && c <= '9')
                                                {
                                                    value = 10 * (c - '0');
                                                    c = hhSmmSss[7];
                                                    if ('0' <= c && c <= '9')
                                                    {
                                                        value += c - '0';
                                                        if (0 <= value && value < 60) // Second
                                                            return second + value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }
        #endregion

        #region SplitLine
        private static string[] SplitLine(string line)
        {
            line = line.Replace("Â ", "");
            line = line.Replace("Â", "");
            line = line.Replace(" ", "");
            line = line.Replace(",", "");
            return line.Split('\t');
        }
        #endregion

        #region ParseVolume
        /// <summary>
        /// Parses a volume string.
        /// </summary>
        /// <param name="line">A string to parse.</param>
        /// <returns>A value of the volume.</returns>
        private static int ParseVolume(string line)
        {
            int volume = 0;
            foreach (char c in line)
            {
                if ('0' <= c && c <= '9')
                {
                    volume *= 10;
                    volume += c - '0';
                }
            }
            return volume;
        }
        #endregion

        #region BeginQuote
        /// <summary>
        /// Formats a begin-of-quote XML tag string with attributes. 
        /// </summary>
        /// <param name="jdn">A julian day number.</param>
        /// <returns>The begin-of-quote XML tag string.</returns>
        private static string BeginQuote(int jdn)
        {
            return string.Format("<q d=\"{0}\" j=\"{1}\">", JulianDayNumber.ToYYYYMMDD(jdn), jdn);
        }
        #endregion

        #region ImportCsv
        /// <summary>
        /// Imports a downloded Euronext intraday csv file into a stack containing tick strings in XML format. 
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="accumulate">Accamulate volume for sequential trades with the same time and price.</param>
        /// <returns>A stack containing imported tick strings in XML format.</returns>
        private static Stack<string> ImportCsv(EuronextInstrumentContext context, out int jdn, bool accumulate)
        {
            Stack<string> tickStack = new Stack<string>(1024);
            jdn = 0;
            using (StreamReader csvStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, time;
                const string errorFormat = "invalid intraday csv{0}, line {1} [{2}] file {3}, skipping";
                string[] splitted;
                int lineNumber = 4, seconds = 0;
                csvStreamReader.ReadLine();        // Intraday of current trading day (21/03/08)
                csvStreamReader.ReadLine();        // Empty line
                line = csvStreamReader.ReadLine(); // Company name<tab>ISIN<tab>Euronext code<tab>MEP<tab>Trading venue<tab>Price Multiplier<tab>Quantity notation<tab>Symbol<tab>CFI classification<tab>Date
                seconds = line.Contains("CFI classification") ? 1 : 0;
                line = csvStreamReader.ReadLine(); // ABN AMRO HOLDING<tab>NL0000301109<tab>NL0000301109<tab>AMS<tab>EURONEXT AMSTERDAM<tab>1.0<tab>Number of units<tab>AABA<tab>ESXXXX Equities<tab>20/03/08 17:35 CET
                if (null != line)
                {
                    csvStreamReader.ReadLine();    // Empty line
                    csvStreamReader.ReadLine();    // Date - time<tab>Trade id<tab>Quote<tab>Volume
                    splitted = line.Split('\t');
                    if (8 < splitted.Length)
                    {
                        try
                        {
                            time = (9 < splitted.Length && 1 == seconds) ? splitted[9] : splitted[8];
                            jdn = JulianDayNumber.FromDDsMMsYY(time);
                        }
                        catch (Exception /*e*/)
                        {
                            //Trace.TraceError(errorFormat, string.Concat(" header datestamp (jd: ", e.Message, ")"), lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            Trace.TraceError(errorFormat, " header datestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            jdn = 0;
                            return null;
                        }
                    }
                    else
                    {
                        Trace.TraceError(errorFormat, " header datestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        return null;
                    }
                    lineNumber = 7;
                    tickStack.Push(quoteEnd);
                    if (accumulate)
                    {
                        string pricePrev = "", timePrev = "";
                        int volume = 0, secondsPrev = 0;
                        bool accumulated = false;
                        while (null != (line = csvStreamReader.ReadLine()))
                        {
                            splitted = SplitLine(line);
                            if (4 == splitted.Length)
                            {
                                time = splitted[0];
                                if (-1 != (seconds = SecondsFromHHsMMsSS(time)))
                                {
                                    if (accumulated)
                                    {
                                        if (seconds == secondsPrev && splitted[2].Equals(pricePrev, StringComparison.InvariantCulture))
                                            volume += ParseVolume(splitted[3]);
                                        else // Time or price differs
                                        {
                                            string s = pricePrev;
                                            try
                                            {
                                                double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                                s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                                            }
                                            catch (Exception)
                                            {
                                                Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                                s = pricePrev;
                                            }
                                            // This fixes a "50.10" -> "50.1" glitch
                                            tickStack.Push(string.Format(tickFormat, /*pricePrev*/s, secondsPrev, timePrev, volume));
                                            timePrev = time;
                                            secondsPrev = seconds;
                                            pricePrev = splitted[2];
                                            volume = ParseVolume(splitted[3]);
                                        }
                                    }
                                    else
                                    {
                                        timePrev = time;
                                        secondsPrev = seconds;
                                        pricePrev = splitted[2];
                                        volume = ParseVolume(splitted[3]);
                                        accumulated = true;
                                    }
                                }
                                else
                                    Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            }
                            else
                                Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            lineNumber++;
                        }
                        if (accumulated)
                        {
                            string s = pricePrev;
                            try
                            {
                                double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                            }
                            catch (Exception)
                            {
                                Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                s = pricePrev;
                            }
                            // This fixes a "50.10" -> "50.1" glitch
                            tickStack.Push(string.Format(tickFormat, /*pricePrev*/s, secondsPrev, timePrev, volume));
                        }
                    }
                    else
                    {
                        while (null != (line = csvStreamReader.ReadLine()))
                        {
                            splitted = SplitLine(line);
                            if (4 == splitted.Length)
                            {
                                time = splitted[0];
                                if (-1 != (seconds = SecondsFromHHsMMsSS(time)))
                                {
                                    string s = splitted[2];
                                    try
                                    {
                                        double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                        s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                                    }
                                    catch (Exception)
                                    {
                                        Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                        s = splitted[2];
                                    }
                                    // This fixes a "50.10" -> "50.1" glitch
                                    tickStack.Push(string.Format(tickFormat, /*splitted[2]*/s, seconds, time, ParseVolume(splitted[3])));
                                }
                                else
                                    Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            }
                            else
                                Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            lineNumber++;
                        }
                    }
                    tickStack.Push(BeginQuote(jdn));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header datestamp", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (3 > tickStack.Count)
            {
                Trace.TraceError("no intraday data found in csv file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return tickStack;
        }
        #endregion

        #region ImportCsvh
        /// <summary>
        /// Imports a downloded Euronext intraday csvh file into a stack containing tick strings in XML format. 
        /// </summary>
        /// <param name="context">A instrument context.</param>
        /// <param name="jdn">A julian day number of the intraday data.</param>
        /// <param name="accumulate">Accamulate volume for sequential trades with the same time and price.</param>
        /// <returns>A stack containing imported tick strings in XML format.</returns>
        private static Stack<string> ImportCsvh(EuronextInstrumentContext context, int jdn, bool accumulate)
        {
            Stack<string> tickStack = new Stack<string>(1024);
            using (StreamReader csvhStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, time;
                const string errorFormat = "invalid intraday csvh{0}, line {1} [{2}] file {3}, skipping";
                string[] splitted;
                int lineNumber = 1, seconds = 0;
                line = csvhStreamReader.ReadLine(); // ´╗┐18:07:15\t0\t371.20\t0
                if (null != line)
                {
                    //line = line.Replace("´╗┐", "");
                    tickStack.Push(quoteEnd);
                    if (accumulate)
                    {
                        string pricePrev = "", timePrev = "";
                        int volume = 0, secondsPrev = 0;
                        bool accumulated = false;
                        do
                        {
                            splitted = SplitLine(line);
                            if (4 == splitted.Length)
                            {
                                time = splitted[0];
                                if (-1 != (seconds = SecondsFromHHsMMsSS(time)))
                                {
                                    if (accumulated)
                                    {
                                        if (seconds == secondsPrev && splitted[2].Equals(pricePrev, StringComparison.InvariantCulture))
                                            volume += ParseVolume(splitted[3]);
                                        else // Time or price differs
                                        {
                                            string s = pricePrev;
                                            try
                                            {
                                                double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                                s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                                            }
                                            catch (Exception)
                                            {
                                                Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                                s = pricePrev;
                                            }
                                            // This fixes a "50.10" -> "50.1" glitch
                                            tickStack.Push(string.Format(tickFormat, /*pricePrev*/s, secondsPrev, timePrev, volume));
                                            timePrev = time;
                                            secondsPrev = seconds;
                                            pricePrev = splitted[2];
                                            volume = ParseVolume(splitted[3]);
                                        }
                                    }
                                    else
                                    {
                                        timePrev = time;
                                        secondsPrev = seconds;
                                        pricePrev = splitted[2];
                                        volume = ParseVolume(splitted[3]);
                                        accumulated = true;
                                    }
                                }
                                else
                                    Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            }
                            else
                                Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            lineNumber++;
                        } while (null != (line = csvhStreamReader.ReadLine()));
                        if (accumulated)
                        {
                            string s = pricePrev;
                            try
                            {
                                double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                            }
                            catch (Exception)
                            {
                                Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                s = pricePrev;
                            }
                            // This fixes a "50.10" -> "50.1" glitch
                            tickStack.Push(string.Format(tickFormat, /*pricePrev*/s, secondsPrev, timePrev, volume));
                        }
                    }
                    else
                    {
                        do
                        {
                            splitted = SplitLine(line);
                            if (4 == splitted.Length)
                            {
                                time = splitted[0];
                                if (-1 != (seconds = SecondsFromHHsMMsSS(time)))
                                {
                                    string s = splitted[2];
                                    try
                                    {
                                        double p = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                                        s = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
                                    }
                                    catch (Exception)
                                    {
                                        Trace.TraceError(errorFormat, " price", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                                        s = splitted[2];
                                    }
                                    // This fixes a "50.10" -> "50.1" glitch
                                    tickStack.Push(string.Format(tickFormat, /*splitted[2]*/s, seconds, time, ParseVolume(splitted[3])));
                                }
                                else
                                    Trace.TraceError(errorFormat, " timestamp", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            }
                            else
                                Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                            lineNumber++;
                        } while (null != (line = csvhStreamReader.ReadLine()));
                    }
                    tickStack.Push(string.Format("<q d=\"{0}\" j=\"{1}\">", JulianDayNumber.ToYYYYMMDD(jdn), jdn));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header datestamp", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (3 > tickStack.Count)
            {
                Trace.TraceError("no intraday data found in csvh file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return tickStack;
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
            Debug.WriteLine(string.Concat("downloading intraday html ", filePath, " from ", uri));
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
            const string pattern5 = "<td align=\"left\">";
            const int pattern5NotInitilized = 9999999;
            int pattern5Index = pattern5NotInitilized;
            bool downloaded = false, found = false;
            int i, page = 0, tid = 0;
            string line, date, id, quote, volume;
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
                            found = true;
                            i += pattern1Length;
                            date = line.Substring(i, 8);
                            line = streamReader.ReadLine();
                            if (pattern5NotInitilized == pattern5Index)
                                pattern5Index = line.IndexOf(pattern5);
                            if (-1 < pattern5Index)
                            {
                                //  <tr>
                                //    <td class="tableHeader" >Date - time</td>
                                //    <td class="tableHeader" >Trade id</td>
                                //    <td class="tableHeader" >Quote</td>
                                //    <td class="tableHeader" >Volume</td>
                                //..</tr>
                                //  <tr class=bgColor7>
                                //    <td class="tableDateStamp" style="white-space: nowrap">17:37:17</td>
                                //    <td align="left">
                                //
                                //        &nbsp;39458</td>
                                //    <td>55.97</td>
                                //    <td>9 771</td>
                                //  </tr>
                                //  <tr class=bgColor1>
                                //    <td class="tableDateStamp" style="white-space: nowrap">17:37:10</td>
                                line = streamReader.ReadLine();
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern2) + pattern2Length;
                                id = line.Substring(i, line.IndexOf(pattern4) - i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                quote = line.Substring(i, line.IndexOf(pattern4) - i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                volume = line.Substring(i, line.IndexOf(pattern4) - i);
                                volume = volume.Replace(" ", "");
                                streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}", date, id, quote, volume);
                                Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", date, id, quote, volume));
                            }
                            else
                            {
                                //  <tr>
                                //    <td class="tableHeader" >Date - time</td>
                                //    <td class="tableHeader" >Quote</td>
                                //  </tr>
                                //  <tr class=bgColor7>
                                //    <td class="tableDateStamp" style="white-space: nowrap">18:07:15</td>
                                //    <td>371.20</td>
                                //  </tr>
                                //  <tr class=bgColor1>
                                //    <td class="tableDateStamp" style="white-space: nowrap">18:02:15</td>
                                //    <td>371.20</td>
                                //  </tr>
                                i = line.IndexOf(pattern3) + pattern3Length;
                                quote = line.Substring(i, line.IndexOf(pattern4) - i);
                                streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}", date, tid++, quote, 0);
                                Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", date, tid, quote, 0));
                            }
                        }
                        line = streamReader.ReadLine();
                    }
                    while (found)
                    {
                        page++;
                        streamReader.Close();
                        streamReader.Dispose();
                        line = string.Concat(uri, "&pageIndex=", page.ToString());
                        Debug.WriteLine(string.Concat("downloading intraday html page ", page, " from ", line));
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
                                if (pattern5NotInitilized == pattern5Index)
                                    pattern5Index = line.IndexOf(pattern5);
                                if (-1 < pattern5Index)
                                {
                                    line = streamReader.ReadLine();
                                    line = streamReader.ReadLine();
                                    i = line.IndexOf(pattern2) + pattern2Length;
                                    id = line.Substring(i, line.IndexOf(pattern4) - i);
                                    line = streamReader.ReadLine();
                                    i = line.IndexOf(pattern3) + pattern3Length;
                                    quote = line.Substring(i, line.IndexOf(pattern4) - i);
                                    line = streamReader.ReadLine();
                                    i = line.IndexOf(pattern3) + pattern3Length;
                                    volume = line.Substring(i, line.IndexOf(pattern4) - i);
                                    volume = volume.Replace(" ", "");
                                    streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}", date, id, quote, volume);
                                    Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", date, id, quote, volume));
                                }
                                else
                                {
                                    i = line.IndexOf(pattern3) + pattern3Length;
                                    quote = line.Substring(i, line.IndexOf(pattern4) - i);
                                    streamWriter.WriteLine("{0}\t{1}\t{2}\t{3}", date, tid++, quote, 0);
                                    Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", date, tid, quote, 0));
                                }
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
                                Trace.TraceError("intraday html file {0}: downloaded length {1} is smaller than the minimal length {2}, retrying ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
                            else
                            {
                                Trace.TraceError("intraday html file {0}: downloaded length {1} is smaller than the minimal length {2}, giving up ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
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
                        Trace.TraceError("intraday html file {0} page {1}: download failed [{2}], retrying ({3})", Path.GetFileName(filePath), page, e.Message, retries);
                    else
                        Trace.TraceError("intraday html file {0} page {1}: download failed [{2}], giving up ({3})", Path.GetFileName(filePath), page, e.Message, retries);
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
            int jdn = JulianDayNumber.FromYYYYMMDD(context.Yyyymmdd);
            Stack<string> tickStack = context.DownloadedPath.EndsWith(".csvh") ?
                ImportCsvh(context, jdn, true) : ImportCsv(context, out jdn, true);
            if (null == tickStack)
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
                                            int j = ParseJulianDayNumber(line);
                                            if (j < jdn)
                                            {
                                                xmlStreamWriter.WriteLine(line);
                                                while (null != (line = xmlStreamReader.ReadLine()))
                                                {
                                                    xmlStreamWriter.WriteLine(line);
                                                    if (line.StartsWith(quoteEnd))
                                                        break;
                                                }
                                            }
                                            else if (j == jdn)
                                            {
                                                foreach (string s in tickStack)
                                                    xmlStreamWriter.WriteLine(s);
                                                tickStack.Clear();
                                                while (null != (line = xmlStreamReader.ReadLine()))
                                                    if (line.StartsWith(quoteEnd))
                                                        break;
                                            }
                                            else // if (j > jdn)
                                            {
                                                foreach (string s in tickStack)
                                                    xmlStreamWriter.WriteLine(s);
                                                tickStack.Clear();
                                                xmlStreamWriter.WriteLine(line);
                                                while (null != (line = xmlStreamReader.ReadLine()))
                                                {
                                                    xmlStreamWriter.WriteLine(line);
                                                    if (line.StartsWith(quoteEnd))
                                                        break;
                                                }
                                            }
                                        }
                                        else if (line.StartsWith(intradayEnd))
                                        {
                                            foreach (string s in tickStack)
                                                xmlStreamWriter.WriteLine(s);
                                            tickStack.Clear();
                                            xmlStreamWriter.WriteLine(line);
                                            break;
                                        }
                                        else
                                            xmlStreamWriter.WriteLine(line);
                                    }
                                }
                                else
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
                                    xmlStreamWriter.WriteLine(intradayBegin);
                                    foreach (string s in tickStack)
                                        xmlStreamWriter.WriteLine(s);
                                    xmlStreamWriter.WriteLine(intradayEnd);
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
                    // Just write new output stream and copy the csv intraday data.
                    xmlStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
                    xmlStreamWriter.WriteLine(instrumentsBegin);
                    xmlStreamWriter.WriteLine(instrumentFormat, context.Isin, context.Mep, context.Name, context.Symbol);
                    xmlStreamWriter.WriteLine(intradayBegin);
                    foreach (string s in tickStack)
                        xmlStreamWriter.WriteLine(s);
                    xmlStreamWriter.WriteLine(intradayEnd);
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
        /// Downloads intraday data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a traling separator.</param>
        /// <param name="allowHtml">Try to download html if csv download fails.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        public static bool Download2(EuronextInstrumentContext context, string downloadDir, bool allowHtml)
        {
            List<string> channels = new List<string>(){ "2634", "2593", "2783", "2549", "2512", "1821" };
            Random random = new Random(DateTime.Now.Millisecond);
            string cha = channels[random.Next(0, channels.Count)];
            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoi.csv", context.Mep, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;
            bool succeded = true;
            string uri = string.Concat(
                "http://www.euronext.com/tools/datacentre/dataCentreDownloadExcell.jcsv?quote=on&volume=on&lan=EN&cha=",
                //"http://160.92.106.167/tools/datacentre/dataCentreDownloadExcell.jcsv?quote=on&volume=on&lan=EN&cha=",
                cha, "&time=on&formatDecimal=&formatDecimalValue=&typeDownload=1&choice=1&indexCompo=&format=txt&selectedMep=",
                Euronext.MepToInteger(context.Mep).ToString(), "&isinCode=", context.Isin);
            if (!Downloader.Download(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, cha))
            {
                if (!allowHtml)
                    return false;
                uri = string.Concat(
                    "http://www.euronext.com/tools/datacentre/dataCentreDownloadHTML-", cha, "-EN.html?&quote=on&time=on&dayvolume=on&volume=on&indexCompo=&format=txt&formatDate=dd/mm/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&typeDownload=1&choice=1&cha=", cha, "&isinCode=",
                    //"http://160.92.106.167/tools/datacentre/dataCentreDownloadHTML-", cha, "-EN.html?&quote=on&time=on&dayvolume=on&volume=on&indexCompo=&format=txt&formatDate=dd/mm/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&typeDownload=1&choice=1&cha=", cha, "&isinCode=",
                    context.Isin, "&selectedMep=", Euronext.MepToInteger(context.Mep).ToString(), "&isin=", context.Isin, "&Mep=", Euronext.MepToInteger(context.Mep).ToString());
                s = string.Concat(context.DownloadedPath, "h");
                context.DownloadedPath = s;
                if (!DownloadHtml(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout))
                    succeded = false;
            }
            return succeded;
        }

        /// <summary>
        /// Downloads intraday data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a traling separator.</param>
        /// <param name="allowHtml">Try to download html if csv download fails.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        public static bool Download(EuronextInstrumentContext context, string downloadDir, bool allowHtml)
        {
            List<string> channels = new List<string>(){ "2634", "2593", "2783", "2549", "2512", "1821" };
            Random random = new Random(DateTime.Now.Millisecond);
            string cha = channels[random.Next(0, channels.Count)];
            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoi.csv", context.Mep, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;
            bool succeded = true;
            string uri = string.Concat(
                "http://www.euronext.com/tools/datacentre/dataCentreDownloadExcell.jcsv?quote=on&volume=on&lan=EN&cha=",
                //"http://160.92.106.167/tools/datacentre/dataCentreDownloadExcell.jcsv?quote=on&volume=on&lan=EN&cha=",
                cha, "&time=on&formatDecimal=&formatDecimalValue=&typeDownload=1&choice=1&indexCompo=&format=txt&selectedMep=",
                Euronext.MepToInteger(context.Mep).ToString(), "&isinCode=", context.Isin);
            if (!Downloader.Download(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, cha))
            {
                if (!allowHtml)
                    return false;
                uri = string.Concat(
                    "http://www.euronext.com/tools/datacentre/dataCentreDownloadHTML-", cha, "-EN.html?&quote=on&time=on&dayvolume=on&volume=on&indexCompo=&format=txt&formatDate=dd/mm/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&typeDownload=1&choice=1&cha=", cha, "&isinCode=",
                    //"http://160.92.106.167/tools/datacentre/dataCentreDownloadHTML-", cha, "-EN.html?&quote=on&time=on&dayvolume=on&volume=on&indexCompo=&format=txt&formatDate=dd/mm/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&typeDownload=1&choice=1&cha=", cha, "&isinCode=",
                    context.Isin, "&selectedMep=", Euronext.MepToInteger(context.Mep).ToString(), "&isin=", context.Isin, "&Mep=", Euronext.MepToInteger(context.Mep).ToString());
                s = string.Concat(context.DownloadedPath, "h");
                context.DownloadedPath = s;
                if (!DownloadHtml(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout))
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
            string directory = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, intraday, context.DownloadRepositorySuffix);
            string separator = Path.DirectorySeparatorChar.ToString();
            string parent = directory;
            if (directory.EndsWith(separator))
                parent = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                parent = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            parent = string.Concat(Directory.GetParent(parent).FullName, separator);
            Packager.ZipCsvDirectory(string.Concat(parent, context.Yyyymmdd, "enx_eoi.zip"), directory, true);
        }
        #endregion

        #region Serialization
        private static void SerializeTo(List<EuronextExecutor.Instrument> instance, string fileName)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(List<EuronextExecutor.Instrument>), null, 65536, false, true, null);
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                dcs.WriteObject(fs, instance);
                fs.Close();
            }
        }

        private static List<EuronextExecutor.Instrument> DeserializeFrom(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(List<EuronextExecutor.Instrument>), null, 65536, false, true, null);
            List<EuronextExecutor.Instrument> instance = (List<EuronextExecutor.Instrument>)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
            File.Delete(fileName);
            return instance;
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs a daily update task.
        /// </summary>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int UpdateTask()
        {
            const string approvedStorageFile = "EuronextIntradayUpdate.approved.storage";
            const string discoveredStorageFile = "EuronextIntradayUpdate.discovered.storage";
            object notDownloadedListLock = new object();
            List<EuronextExecutor.Instrument> approvedStorage = null, discoveredStorage = null;
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<List<EuronextExecutor.Instrument>> approvedList, discoveredList;

            if (File.Exists(approvedStorageFile) || File.Exists(discoveredStorageFile))
            {
                if (File.Exists(approvedStorageFile))
                {
                    approvedStorage = DeserializeFrom(approvedStorageFile);
                    Trace.TraceInformation("Deserialized from approved storage file: {0}, {1}", approvedStorageFile, DateTime.Now);
                }
                else
                    approvedStorage = new List<EuronextExecutor.Instrument>();
                if (File.Exists(discoveredStorageFile))
                {
                    discoveredStorage = DeserializeFrom(discoveredStorageFile);
                    Trace.TraceInformation("Deserialized from discovered storage file: {0}, {1}", discoveredStorageFile, DateTime.Now);
                }
                else
                    discoveredStorage = new List<EuronextExecutor.Instrument>();
                approvedList = EuronextExecutor.Split(approvedStorage, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(discoveredStorage, EuronextInstrumentContext.WorkerThreads);
            }
            else
            {
                Trace.TraceInformation("Preparing: {0}", DateTime.Now);
                approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            }
            long downloadedApprovedInstruments = 0, mergedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, mergedDiscoveredInstruments = 0, discoveredInstruments = 0;
            string downloadDir = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, intraday);

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
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                    string s = string.Concat(EuronextInstrumentContext.IntradayRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
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
                else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                    string s = string.Concat(EuronextInstrumentContext.IntradayDiscoveredRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instrument (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
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
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false/*pass == passCount*/))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                            string s = string.Concat(EuronextInstrumentContext.IntradayRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instrument (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
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
                        else if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false/*pass == passCount*/))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                            string s = string.Concat(EuronextInstrumentContext.IntradayDiscoveredRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} approved   instrument: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments, mergedApprovedInstruments);
            Trace.TraceInformation("Intraday {0} discovered instrument: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments, mergedDiscoveredInstruments);
            Trace.TraceInformation("Intraday {0} both                 : total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments, mergedApprovedInstruments + mergedDiscoveredInstruments);
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    approvedStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} approved instrument: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        approvedStorage.Add(new EuronextExecutor.Instrument() { Isin = esc.Isin, Mep = esc.Mep, Name = esc.Name, Symbol = esc.Symbol, File = esc.RelativePath });
                    });
                    SerializeTo(approvedStorage, approvedStorageFile);
                    Trace.TraceInformation("Serialized to approved storage file: {0}", approvedStorageFile);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    discoveredStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} discovered instrument: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        discoveredStorage.Add(new EuronextExecutor.Instrument() { Isin = esc.Isin, Mep = esc.Mep, Name = esc.Name, Symbol = esc.Symbol, File = esc.RelativePath });
                    });
                    SerializeTo(discoveredStorage, discoveredStorageFile);
                    Trace.TraceInformation("Serialized to discovered storage file: {0}", discoveredStorageFile);
                }
            }
            else
            {
                Zip(context);
                Trace.TraceInformation("Zipped downloaded files.");
                if (File.Exists(approvedStorageFile))
                {
                    File.Delete(approvedStorageFile);
                    Trace.TraceInformation("Deleted approved storage file: {0}", approvedStorageFile);
                }
                if (File.Exists(discoveredStorageFile))
                {
                    File.Delete(discoveredStorageFile);
                    Trace.TraceInformation("Deleted discovered storage file: {0}", discoveredStorageFile);
                }
            }
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region DownloadTask
        /// <summary>
        /// Performs a download task.
        /// </summary>
        /// <returns>The number of not downloaded instruments.</returns>
        public static int DownloadTask(string downloadPath)
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

            const string approvedStorageFile = "EuronextIntradayUpdate.approved.storage";
            const string discoveredStorageFile = "EuronextIntradayUpdate.discovered.storage";
            object notDownloadedListLock = new object();
            List<EuronextExecutor.Instrument> approvedStorage = null, discoveredStorage = null;
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(4096);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<List<EuronextExecutor.Instrument>> approvedList, discoveredList;

            if (File.Exists(approvedStorageFile) || File.Exists(discoveredStorageFile))
            {
                if (File.Exists(approvedStorageFile))
                {
                    approvedStorage = DeserializeFrom(approvedStorageFile);
                    Trace.TraceInformation("Deserialized from approved storage file: {0}, {1}", approvedStorageFile, DateTime.Now);
                }
                else
                    approvedStorage = new List<EuronextExecutor.Instrument>();
                if (File.Exists(discoveredStorageFile))
                {
                    discoveredStorage = DeserializeFrom(discoveredStorageFile);
                    Trace.TraceInformation("Deserialized from discovered storage file: {0}, {1}", discoveredStorageFile, DateTime.Now);
                }
                else
                    discoveredStorage = new List<EuronextExecutor.Instrument>();
                approvedList = EuronextExecutor.Split(approvedStorage, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(discoveredStorage, EuronextInstrumentContext.WorkerThreads);
            }
            else
            {
                Trace.TraceInformation("Preparing: {0}", DateTime.Now);
                approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
                discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            }
            long downloadedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, discoveredInstruments = 0;
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved instruments to {0}: {1}", downloadPath, DateTime.Now);
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
                else if (Download(esc, downloadPath, false))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                        cfi.LimitReached = true;
                    }
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered instruments to {0}: {1}", downloadPath, DateTime.Now);
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
                else if (Download(esc, downloadPath, false))
                {
                    cfi.Count = 0;
                    cfi.LimitReached = false;
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                }
                else
                {
                    if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                    {
                        cfi.LimitReached = true;
                        Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                    }
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instrument (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
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
                        else if (Download(esc, downloadPath, pass == passCount))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                approvedNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instrument (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
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
                        else if (Download(esc, downloadPath, pass == passCount))
                        {
                            cfi.Count = 0;
                            cfi.LimitReached = false;
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                        }
                        else
                        {
                            if (EuronextInstrumentContext.IntradayDownloadMaximumLimitConsecutiveFails == ++cfi.Count)
                            {
                                Trace.TraceError("Consecutive download fail limit reached: thread {0} count {1}", Thread.CurrentThread.Name, cfi.Count);
                                cfi.LimitReached = true;
                            }
                            lock (notDownloadedListLock)
                            {
                                discoveredNotDownloadedList.Add(esc);
                            }
                        }
                    }, EuronextInstrumentContext.IntradayWorkerThreadDelayMilliseconds);
                }
                pass++;
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} approved   instrument: total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments);
            Trace.TraceInformation("Intraday {0} discovered instrument: total {1}, downloaded {2}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments);
            Trace.TraceInformation("Intraday {0} both                 : total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments);
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedNotDownloadedList.Count)
                {
                    approvedStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} approved instrument: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        approvedStorage.Add(new EuronextExecutor.Instrument() { Isin = esc.Isin, Mep = esc.Mep, Name = esc.Name, Symbol = esc.Symbol, File = esc.RelativePath });
                    });
                    SerializeTo(approvedStorage, approvedStorageFile);
                    Trace.TraceInformation("Serialized to approved storage file: {0}", approvedStorageFile);
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    discoveredStorage = new List<EuronextExecutor.Instrument>(1024);
                    Trace.TraceInformation("Failed to download {0} discovered instrument: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                        discoveredStorage.Add(new EuronextExecutor.Instrument() { Isin = esc.Isin, Mep = esc.Mep, Name = esc.Name, Symbol = esc.Symbol, File = esc.RelativePath });
                    });
                    SerializeTo(discoveredStorage, discoveredStorageFile);
                    Trace.TraceInformation("Serialized to discovered storage file: {0}", discoveredStorageFile);
                }
            }
            else
            {
                if (File.Exists(approvedStorageFile))
                {
                    File.Delete(approvedStorageFile);
                    Trace.TraceInformation("Deleted approved storage file: {0}", approvedStorageFile);
                }
                if (File.Exists(discoveredStorageFile))
                {
                    File.Delete(discoveredStorageFile);
                    Trace.TraceInformation("Deleted discovered storage file: {0}", discoveredStorageFile);
                }
            }
            return approvedNotDownloadedList.Count + discoveredNotDownloadedList.Count;
        }
        #endregion

        #region ImportTask
        /// <summary>
        /// Performs an import task.
        /// </summary>
        /// <param name="importPath">A path to an import directory or an import file.</param>
        /// <param name="yyyymmdd">A target date.</param>
        /// <returns>The number of orphaned instruments.</returns>
        public static int ImportTask(string importPath, string yyyymmdd)
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
            EuronextExecutor.Iterate(list, dictionaryApproved, dictionaryDiscovered, yyyymmdd, (xml, esc) =>
            {
                Interlocked.Increment(ref totalInstruments);
                EuronextInstrumentContext.VerifyFile(xml);
                if (Merge(xml, esc))
                    Interlocked.Increment(ref mergedInstruments);
            }, true);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Intraday {0} imported instrument: total {1}, merged {2}", yyyymmdd, totalInstruments, mergedInstruments);
            orphaned.ForEach(file => Trace.TraceInformation("Orphaned import file [{0}], skipped", file));
            return orphaned.Count;
        }
        #endregion
    }
}
