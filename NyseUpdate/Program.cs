using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Excel;
using Mbh5;

namespace mbdt.NyseUpdate
{
    public static class Program
    {
        private class Info
        {
            public string FilePathCsv { get; set; }
            public string FileNameH5 { get; set; }
            public string InstrumentPath { get; set; }
            public bool HasVolume { get; set; }
        }
        private static readonly List<Info> infoList = new List<Info>();

        public static void Main(string[] args)
        {
            //Cvt("a.csv");
            //return;
            Environment.ExitCode = 0;
            if (args.Length == 0)
            {
                int daysBackDay = Properties.Settings.Default.DownloadLookabckDaysDay;
                int daysBackMinute = Properties.Settings.Default.DownloadLookbackDaysMinute;
                DateTime lastDate = Properties.Settings.Default.DownloadLastDate;
                if (lastDate.Year < 1900)
                    lastDate = DateTime.Today;
                Trace.TraceInformation("=======================================================================================");
                Trace.TraceInformation("Update to [{0}] from {1} days back: 1m [{2}], 1d [{3}]", Properties.Settings.Default.DownloadDir, lastDate.ToShortDateString(), daysBackMinute, daysBackDay);
                if (!DownloadAllSymbols(lastDate, daysBackDay, daysBackMinute))
                    Environment.ExitCode = 1;
                foreach (var info in infoList)
                    Import(info);
            }
            else if (args.Length == 4)
            {
                Trace.TraceInformation("=======================================================================================");
                Trace.TraceInformation("Import to [{0}\\{1}:{2}] [{3}], .csv files in [{4}]",
                    Properties.Settings.Default.RepositoryPath, args[0], args[1], args[2], args[3]);
                Import(new Info
                {
                    FileNameH5 = args[0], InstrumentPath = args[1], HasVolume = args[2].StartsWith("hasvolume"), FilePathCsv = args[3]
                });
            }
            else
            {
                Console.WriteLine("Arguments: h5_file_name instrument_path novolume|hasvolume input_file_or_dir");
                Console.WriteLine("h5_file_name:         e.g., DJI.h5");
                Console.WriteLine("                      will be created in repository directory specified in .config file");
                Console.WriteLine("instrument_path:      e.g., /XNYS_DJI_US2605661048/");
                Console.WriteLine("                      must be consistent with settings in .config file");
                Console.WriteLine("novolume|hasvolume:   novolume -> ohlc, hasvolume -> ohlcv");
                Console.WriteLine("                      must be consistent with settings in .config file");
                Console.WriteLine("input_file_or_dir:    must end with _1m.csv or _1d.csv");
                Console.WriteLine("                      all other files will be ignored");
                Environment.ExitCode = 1;
                return;
            }
            Trace.TraceInformation("Finished: {0}, exit code {1}", DateTime.Now, Environment.ExitCode);
        }

        #region Download
        private static bool DownloadAllSymbols(DateTime lastDate, int daysBackDay, int daysBackMinute)
        {
            string qsWsid = GetSessionId();
            bool okDay = true, okMinute = true;
            foreach (var v in Properties.Settings.Default.Symbols.Split(';'))
            {
                //lastDate = new DateTime(2011, 8, 1);
                //daysBackDay = 0;
                //daysBackMinute = 50000;

                string[] w = v.Split(',');
                if (w.Length != 5)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        "Instrument info [{0}] must have 5 comma-delimited parts but has {1}", v, w.Length));
                string prefix = w[0];
                string symbol = w[1];
                string fileNameH5 = w[2];
                string instrumentPath = w[3];
                bool hasVolume = w[4].StartsWith("hasvolume");
                //symbol = "DMT";
                //prefix = "41.10.";

                string referer = "http://quotespeed.morningstar.com/apis/api.jsp?instid=NYSE&module=chart&symbol=" + prefix + symbol;
                string name = ComposeDownloadableName(symbol, lastDate, daysBackMinute, "1m");
                string downloadable = ComposeDownloadable(name);
                string ymdStampLast = lastDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                string ymdStampFirst = lastDate.AddDays(-daysBackMinute).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                Dictionary<string, string> postDictionary = new Dictionary<string, string>
                {
                    {"f", "1"},
                    { "preAfter", "false"},
                    { "sd", ymdStampFirst},
                    { "ed", ymdStampLast},
                    { "days", daysBackMinute.ToString()},
                    { "cdt", "6"},
                    { "tickers", prefix + symbol},
                    { "headers", "time:Time,open:Open,high:High,low:Low,close:Close,volume:Volume"},
                    { "ept", "csv"},
                    { "qs_wsid", Properties.Settings.Default.SessionId.Length > 0 ? Properties.Settings.Default.SessionId : qsWsid}
                    //{ "qs_wsid", qsWsid.Length > 0 ? qsWsid : Properties.Settings.Default.SessionId}
                };

                if (daysBackMinute > 0)
                {
                    if (!DownloadPost("http://quotespeed.morningstar.com/ra/export",
                        downloadable, 1, true, Properties.Settings.Default.DownloadRetries,
                        Properties.Settings.Default.DownloadTimeoutMilliseconds, postDictionary,
                        referer, Properties.Settings.Default.UserAgent))
                        okMinute = false;
                    else
                        infoList.Add(new Info
                        {
                            FilePathCsv = downloadable, FileNameH5 = fileNameH5, HasVolume = hasVolume, InstrumentPath = instrumentPath
                        });
                }

                name = ComposeDownloadableName(symbol, lastDate, daysBackDay, "1d");
                downloadable = ComposeDownloadable(name);
                ymdStampFirst = lastDate.AddDays(-daysBackDay).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                postDictionary["f"] = "d";
                postDictionary["sd"] = ymdStampFirst;
                postDictionary["days"] = daysBackDay.ToString();
                postDictionary["cdt"] = "7";
                if (daysBackDay > 0)
                {
                    if (!DownloadPost("http://quotespeed.morningstar.com/ra/export",
                        downloadable, 1, true, Properties.Settings.Default.DownloadRetries,
                        Properties.Settings.Default.DownloadTimeoutMilliseconds, postDictionary,
                        referer, Properties.Settings.Default.UserAgent))
                        okDay = false;
                    else
                        infoList.Add(new Info
                        {
                            FilePathCsv = downloadable, FileNameH5 = fileNameH5, HasVolume = hasVolume, InstrumentPath = instrumentPath
                        });
                }
            }
            return okMinute & okDay;
        }

        private static string ComposeDownloadableName(string symbol, DateTime dateTime, int daysBack, string timeFrame)
        {
            string ymdStampLast = dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string ymdStampFirst = dateTime.AddDays(-daysBack).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}", ymdStampLast, ymdStampFirst, symbol, timeFrame);
        }

        private static string GetSessionId()
        {
            string sessionId = "";
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create("https://www.nyse.com/Morningstar.shtml");
                webRequest.Proxy = WebRequest.DefaultWebProxy;
                // DefaultCredentials represents the system credentials for the current 
                // security context in which the application is running. For a client-side 
                // application, these are usually the Windows credentials 
                // (user name, password, and domain) of the user running the application. 
                webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                webRequest.Method = "GET";
                webRequest.Referer = "https://www.nyse.com/quote/index/!DJI";
                webRequest.UserAgent = Properties.Settings.Default.UserAgent;
                webRequest.Timeout = Properties.Settings.Default.DownloadTimeoutMilliseconds;
                webRequest.Accept = "Accept=application/json, text/javascript, */*; q=0.01";
                //webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                webRequest.KeepAlive = true;
                WebResponse webResponse = webRequest.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                if (null == responseStream)
                {
                    Trace.TraceError("GetSessionId: received null response stream.");
                    return sessionId;
                }
                using (var streamReader = new StreamReader(responseStream))
                {
                    // {"status":{"errorCode":"0","subErrorCode":"","errorMsg":"Successful"},"data":"9CFCF83C7B7149B751C5F1C609A7E732","attachment":{"SessionID":"9CFCF83C7B7149B751C5F1C609A7E732","CToken":"1462025477054","sessionTimeout":1800}}
                    string line = streamReader.ReadLine();
                    if (null == line)
                    {
                        Trace.TraceError("GetSessionId: got null line from response stream.");
                        return sessionId;
                    }
                    const string pattern = "\"data\":\"";
                    const int patternLen = 8;
                    int i = line.IndexOf(pattern, StringComparison.Ordinal);
                    if (0 > i)
                    {
                        Trace.TraceError("GetSessionId: pattern ["+pattern+"] not found in " + line);
                        return sessionId;
                    }
                    // 9CFCF83C7B7149B751C5F1C609A7E732","attachment":{"SessionID":"9CFCF83C7B7149B751C5F1C609A7E732","CToken":"1462025477054","sessionTimeout":1800}}
                    string s = line.Substring(i + patternLen);
                    i = s.IndexOf("\"", StringComparison.Ordinal);
                    if (0 > i)
                    {
                        Trace.TraceError("GetSessionId: pattern [\"] not found in " + s);
                        return sessionId;
                    }
                    sessionId = s.Substring(0, i);
                    Trace.TraceInformation("Got session id " + sessionId);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("failed to download https://www.nyse.com/Morningstar.shtml: " + e.Message);
            }
            return sessionId;
        }

        private static string ComposeDownloadable(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.csv",
                Properties.Settings.Default.DownloadDir, name);
        }

        private const int PauseMillisecondsBeforeRetry = 1000;

        private static bool DownloadPost(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout,
            Dictionary<string, string> keyValueDictionary, string referer, string userAgent)
        {
            Trace.TraceInformation("Downloading (post) [{0}] from [{1}]", filePath, uri);
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            // ReSharper disable once PossibleNullReferenceException
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            if (!overwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > minimalLength)
                    {
                        Trace.TraceInformation("File {0} already exists, skipping", filePath);
                        return true;
                    }
                    Trace.TraceInformation("File {0} already exists but length {1} is smaller than the minimal length {2}, overwriting", filePath, fileInfo.Length, minimalLength);
                }
            }
            var builder = new StringBuilder(1024);
            foreach (var kvp in keyValueDictionary)
            {
                if (builder.Length != 0)
                    builder.Append("&");
                builder.Append(kvp.Key);
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(kvp.Value));
            }
            string postData = builder.ToString();
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            long bytesReceived = 0;
            while (0 < retries)
            {
                Thread.Sleep(PauseMillisecondsBeforeRetry);
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current 
                    // security context in which the application is running. For a client-side 
                    // application, these are usually the Windows credentials 
                    // (user name, password, and domain) of the user running the application. 
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Host = "quotespeed.morningstar.com";
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = postData.Length;
                    webRequest.Referer = referer;
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    webRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Headers.Add("DNT", "1");
                    webRequest.KeepAlive = true;
                    using (Stream writeStream = webRequest.GetRequestStream())
                    {
                        var encoding = new UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postData);
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                    WebResponse webResponse = webRequest.GetResponse();
                    using (var sourceStream = webResponse.GetResponseStream())
                    {
                        if (sourceStream != null)
                        {
                            using (var targetStream = new StreamWriter(filePath, false))
                            {
                                int bytesRead;
                                while (0 < (bytesRead = sourceStream.Read(buffer, 0, bufferSize)))
                                    targetStream.BaseStream.Write(buffer, 0, bytesRead);
                                bytesReceived = targetStream.BaseStream.Length;
                            }
                        }
                    }
                    if (bytesReceived > minimalLength)
                        retries = 0;
                    else
                    {
                        if (1 < retries)
                            Trace.TraceError("File {0}: downloaded length {1} is smaller than the minimal length {2}, retrying", filePath, bytesReceived, minimalLength);
                        else
                        {
                            Trace.TraceError("File {0}: downloaded length {1} is smaller than the minimal length {2}, giving up", filePath, bytesReceived, minimalLength);
                            File.Delete(filePath);
                        }
                        retries--;
                        bytesReceived = 0;
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(1 < retries ? "File {0}: download failed [{1}], retrying ({2})" : "file {0}: download failed [{1}], giving up ({2})", filePath, e.Message, retries);
                    retries--;
                    bytesReceived = 0;
                }
            }
            return (0 < bytesReceived);
        }
        #endregion

        #region Import
        private static void Import(Info info)
        {
            if (!Directory.Exists(Properties.Settings.Default.RepositoryPath))
                Directory.CreateDirectory(Properties.Settings.Default.RepositoryPath);
            List<Ohlcv> ohlcvList = new List<Ohlcv>(1024);
            List<OhlcvPriceOnly> ohlcList = new List<OhlcvPriceOnly>(1024);
            TraverseTree(info.FilePathCsv, s =>
            {
                FileInfo fi = new FileInfo(s);
                if (0 == fi.Length)
                    Trace.TraceInformation("Zero length file [{0}], skipping", fi.FullName);
                else if (s.EndsWith(".csv"))
                {
                    DataTimeFrame timeFrame = DataTimeFrame.Aperiodic;
                    if (fi.FullName.EndsWith("_1m.csv"))
                        timeFrame = DataTimeFrame.Minute1;
                    else if(fi.FullName.EndsWith("_1d.csv"))
                        timeFrame = DataTimeFrame.Day1;
                    if (DataTimeFrame.Aperiodic == timeFrame)
                        Trace.TraceWarning("Unknown filename [{0}], skipping", fi.FullName);
                    else
                    {
                        ohlcvList.Clear();
                        ohlcList.Clear();
                        Trace.TraceInformation("Parsing {0} bars from [{1}]", info.HasVolume ? "ohlcv" : "ohlcv price only", fi.FullName);
                        ConvertFileToCsv(fi.FullName, timeFrame == DataTimeFrame.Minute1, info.HasVolume);
                        ParseFile(fi.FullName, ohlcvList, ohlcList, timeFrame == DataTimeFrame.Minute1, info.HasVolume);
                        string h5FilePath = string.Concat(Properties.Settings.Default.RepositoryPath, "\\", info.FileNameH5);
                        int count = info.HasVolume ? ohlcvList.Count : ohlcList.Count;
                        if (0 < count)
                        {
                            Trace.TraceInformation("Importing [{0}] parsed bars to [{1}]:[{2}]", count, h5FilePath, info.InstrumentPath);
                            Repository repository = Repository.OpenReadWrite(h5FilePath, true, Properties.Settings.Default.CorkTheCache);
                            if (null == repository)
                                Trace.TraceError("Failed to open [{0}] for read-write access", h5FilePath);
                            else
                            {
                                Instrument instrument = repository.Open(info.InstrumentPath, true);
                                if (null == instrument)
                                    Trace.TraceError("Failed to open [{0}] in [{1}]", info.InstrumentPath, h5FilePath);
                                else
                                {
                                    if (info.HasVolume)
                                    {
                                        OhlcvData data = instrument.OpenOhlcv(OhlcvKind.Default, timeFrame, true);
                                        if (null == data)
                                            Trace.TraceError("Failed to open ohlcv data in [{0}] in [{1}]", info.InstrumentPath, h5FilePath);
                                        else
                                        {
                                            data.SpreadDuplicateTimeTicks(ohlcvList, true);
                                            if (!data.Add(ohlcvList, DuplicateTimeTicks.Skip, true))
                                                Trace.TraceError("Error importing [{0}] parsed ohlcvs to [{1}]:[{2}]", count, h5FilePath, info.InstrumentPath);
                                            data.Flush();
                                            data.Close();
                                        }
                                    }
                                    else
                                    {
                                        OhlcvPriceOnlyData data = instrument.OpenOhlcvPriceOnly(OhlcvKind.Default, timeFrame, true);
                                        if (null == data)
                                            Trace.TraceError("Failed to open ohlcv price only data in [{0}] in [{1}]", info.InstrumentPath, h5FilePath);
                                        else
                                        {
                                            data.SpreadDuplicateTimeTicks(ohlcList, true);
                                            if (!data.Add(ohlcList, DuplicateTimeTicks.Skip, true))
                                                Trace.TraceError("Error importing [{0}] parsed price only ohlcvs to [{1}]:[{2}]", count, h5FilePath, info.InstrumentPath);
                                            data.Flush();
                                            data.Close();
                                        }
                                    }
                                    instrument.Close();
                                }
                                repository.Close();
                            }
                        }
                        else
                            Trace.TraceWarning("No {0} bars parsed from [{1}]", info.HasVolume ? "ohlcv" : "ohlcv price only", fi.FullName);
                    }
                }
            });
        }

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

        private static double ConvertDouble(string s, string name, int lineNumber, string line)
        {
            if (string.IsNullOrEmpty(s) || "-" == s)
                return 0;
            double value;
            try
            {
                value = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                Trace.TraceError("Invalid csv {0}, line {1} [{2}]", name, lineNumber, line);
                value = 0;
            }
            return value;
        }

        private static void ConvertFileToCsv(string file, bool isMinute, bool hasVolume)
        {
            using (StreamReader csvStreamReader = new StreamReader(file, Encoding.UTF8))
            {
                // 1m:
                // Time,Open,High,Low,Close,Volume,
                // 20110801 09:30,5196.21,5207.7202,5196.21,5205.2998,0
                // 1d:
                // Time,Open,High,Low,Close,Volume,
                // 20040506,2923.01001,2926.439941,2889.070068,2912.51001,0
                string firstLine = csvStreamReader.ReadLine();
                if (firstLine != null && firstLine.StartsWith("Time,"))
                    return;
            }
            var fi = new FileInfo(file);
            if (fi.Length < 6)
            {
                Trace.TraceInformation("File {0} is  too small (file length {1} bytes), skipping.", file, fi.Length);
                return;
            }
            Trace.TraceInformation("File {0} is not in CSV format, attemting to convert from Excel.", file);
            List<string> csvList = new List<string>();
            using (var stream = new FileStream(file, FileMode.Open))
            {
                //IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream) ?? ExcelReaderFactory.CreateBinaryReader(stream);
                //IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(stream);
                if (reader == null)
                {
                    Trace.TraceError("Failed to open file {0} as an Excel file.", file);
                    return;
                }
                reader.IsFirstRowAsColumnNames = false;
                DataSet dataset = reader.AsDataSet();
                var tableList = (from object table in dataset.Tables select table.ToString()).ToList();
                tableList.Sort();
                bool isFirstTable = true;
                foreach (var tableName in tableList)
                {
                    bool isFirstRow = true;
                    foreach (DataRow row in dataset.Tables[tableName].Rows)
                    {
                        if (isFirstRow)
                        {
                            isFirstRow = false;
                            if (isFirstTable)
                                isFirstTable = false;
                            else
                                continue;
                        }
                        int length = row.ItemArray.Length, length1 = length - 1;
                        string s = "";
                        for (int i = 0; i < length; ++i)
                        {
                            s += row[i].ToString().Replace(',', '.');
                            if (i < length1)
                                s += ',';
                        }
                        csvList.Add(s);
                    }
                }
            }
            var f = new FileInfo(file);
            f.MoveTo(file + ".xls");
            File.WriteAllLines(file, csvList);
        }

        private static void ParseFile(string file, List<Ohlcv> list, List<OhlcvPriceOnly> listPriceOnly, bool isMinute, bool hasVolume)
        {
            using (StreamReader csvStreamReader = new StreamReader(file, Encoding.UTF8))
            {
                const string errorFormat = "Invalid csv, line {0}, {1} [{2}], file {3}, skipping";
                // 1m:
                // Time,Open,High,Low,Close,Volume,
                // 20110801 09:30,5196.21,5207.7202,5196.21,5205.2998,0
                // 1d:
                // Time,Open,High,Low,Close,Volume,
                // 20040506,2923.01001,2926.439941,2889.070068,2912.51001,0
                csvStreamReader.ReadLine();
                string line;
                int lineNumber = 1;
                Ohlcv ohlcv = new Ohlcv();
                OhlcvPriceOnly ohlc = new OhlcvPriceOnly();
                while (null != (line = csvStreamReader.ReadLine()))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var splitted = line.Split(',');
                    if (6 == splitted.Length)
                    {
                        DateTime dt;
                        if (!DateTime.TryParseExact(splitted[0], isMinute ? "yyyyMMdd HH:mm" : "yyyyMMdd",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            Trace.TraceError(errorFormat, lineNumber,
                                (isMinute ? "cannot parse date-time [" : "cannot parse date [") + splitted[0] + "]",
                                line, Path.GetFileName(file));
                        }
                        var open = ConvertDouble(splitted[1], "open", lineNumber, line);
                        var high = ConvertDouble(splitted[2], "high", lineNumber, line);
                        var low = ConvertDouble(splitted[3], "low", lineNumber, line);
                        var close = ConvertDouble(splitted[4], "close", lineNumber, line);
                        if (hasVolume)
                        {
                            var volume = ConvertDouble(splitted[5], "volume", lineNumber, line);
                            ohlcv.dateTimeTicks = dt.Ticks;
                            ohlcv.open = open;
                            ohlcv.high = high;
                            ohlcv.low = low;
                            ohlcv.close = close;
                            ohlcv.volume = volume;
                            list.Add(ohlcv);
                        }
                        else
                        {
                            ohlc.dateTimeTicks = dt.Ticks;
                            ohlc.open = open;
                            ohlc.high = high;
                            ohlc.low = low;
                            ohlc.close = close;
                            listPriceOnly.Add(ohlc);
                        }
                    }
                    else
                        Trace.TraceError(errorFormat, lineNumber, "must have 6 comma-splitted parts", line, Path.GetFileName(file));
                    lineNumber++;
                }
            }
        }
        #endregion

        private static void Cvt(string file)
        {
            using (StreamReader csvStreamReader = new StreamReader(file, Encoding.UTF8))
            {
                csvStreamReader.ReadLine();
                string line;
                while (null != (line = csvStreamReader.ReadLine()))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var splitted = line.Split(',');
                        DateTime dt = DateTime.ParseExact(splitted[0], "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                        Console.WriteLine("{0},{1},{2},{3},{4},{5}",
                            dt.ToString("yyyyMMdd"), splitted[1], splitted[2], splitted[3], splitted[4], splitted[5]);
                }
            }
        }

    }
}
