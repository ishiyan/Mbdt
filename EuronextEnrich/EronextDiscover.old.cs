using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using mbdt.Utils;

namespace mbdt.EuronextDiscover
{
    /// <summary>
    /// Euronext discover utilities.
    /// </summary>
    internal static class EuronextDiscover
    {
        #region InstrumentInfo
        private class InstrumentInfo
        {
            public string Mic;
            public string MicDescription;
            public string Mep;
            public string Isin;
            public string Name;
            public string Symbol;
            public string Key;
            public string Type;
            public bool IsApproved;
            public bool IsDiscovered;
        }
        #endregion

        #region CategoryInfo
        private class CategoryInfo
        {
            public string Type;
            public string Uri;
            public string Referer;
        }
        #endregion

        private static readonly Dictionary<string, InstrumentInfo> instrumentInfoDictionary = new Dictionary<string, InstrumentInfo>();
        private static readonly Dictionary<string, string> unknownMicDictionary = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> knownMicToMepDictionary = KnownMicToMepDictionary();
        private static readonly List<CategoryInfo> categoryList = CategoryList();
        private static readonly string[] splitter = new[] {@""","""};

        #region Constants
        private const string fund = "fund";
        private const string etv = "etv";
        private const string etf = "etf";
        private const string index = "index";
        private const string stock = "stock";
        private const string inav = "inav";
        #endregion

        #region CategoryList
        private static List<CategoryInfo> CategoryList()
        {
            var list = new List<CategoryInfo>
            {
                new CategoryInfo {Type = stock, Uri = "https://europeanequities.nyx.com/pd/stocks/data?formKey=nyx_pd_filter_values:2a6335bafdc8900530cfcbc5652287b5", Referer = "https://europeanequities.nyx.com/nl/equities-directory"}, new CategoryInfo {Type = index, Uri = "https://indices.nyx.com/pd/indices/data?formKey=nyx_pd_filter_values:40cd750bba9870f18aada2478b24840a", Referer = "https://indices.nyx.com/en/directory/european-indices"}, new CategoryInfo {Type = etf, Uri = "https://etp.nyx.com/en/pd/etps/data/4?formKey=nyx_pd_filter_values:dcca48101505dd86b703689a604fe3c4", Referer = "https://etp.nyx.com/en/etps/etfs/etf-directory"}, new CategoryInfo {Type = etv, Uri = "https://etp.nyx.com/en/pd/etps/data/6?formKey=nyx_pd_filter_values:dcca48101505dd86b703689a604fe3c4", Referer = "https://etp.nyx.com/en/etps/etvs/etv-directory"}, new CategoryInfo {Type = fund, Uri = "https://etp.nyx.com/en/pd/etps/data/5?formKey=nyx_pd_filter_values:dcca48101505dd86b703689a604fe3c4", Referer = "https://etp.nyx.com/en/etps/investments-funds/fund-directory"}
            };
            return list;
        }
        #endregion

        #region KnownMicToMepDictionary
        private static Dictionary<string, string> KnownMicToMepDictionary()
        {
            var dictionary = new Dictionary<string, string> { { "ALXA", "AMS" }, { "ALXB", "BRU" }, { "ALXL", "LIS" }, { "ALXP", "PAR" }, { "ENXB", "BRU" }, { "ENXL", "LIS" }, { "MLXB", "BRU" }, { "TNLA", "AMS" }, { "TNLB", "BRU" }, { "XMLI", "PAR" }, { "XAMS", "AMS" }, { "XBRU", "BRU" }, { "XLIS", "LIS" }, { "XPAR", "PAR" }, { "XHFT", "OTH" } };
            return dictionary;
        }
        #endregion

        #region RetrieveTotalRecords
        private static int RetrieveTotalRecords(string filename)
        {
            const string prefix = "{\"sEcho\":\"0\",\"iTotalRecords\":";
            int totalRecords = 0;
            string s = File.ReadAllText(filename);
            if (s.StartsWith(prefix))
            {
                string sub = s.Substring(prefix.Length);
                totalRecords = sub.TakeWhile(Char.IsDigit).Aggregate(totalRecords, (current, c) => 10 * current + (c - '0'));
            }
            return totalRecords;
        }
        #endregion

        #region ParseFile
        private static void ParseFile(string filename, string type)
        {
            string content = File.ReadAllText(filename);
            string original = content;
            try
            {
                int i = content.IndexOf("[[", StringComparison.Ordinal);
                content = content.Substring(i + 2);
                while ((i = content.IndexOf("],[", StringComparison.Ordinal)) > 0)
                {
                    ParseJs(content.Substring(0, i), type);
                    content = content.Substring(i + 3);
                }
                i = content.IndexOf("]]", StringComparison.Ordinal);
                ParseJs(content.Substring(0, i), type);
            }
            catch (Exception exception)
            {
                Trace.TraceError("Exception parsing filename \"{0}\" ({1}): {2}", filename, exception.Message, original);
                throw;
            }
        }
        #endregion

        #region StripTrailingChars
        private static string StripTrailingChars(string s)
        {
            return s.TrimStart('\"').TrimEnd('\"');
        }
        #endregion

        #region ParseJs
        private static void ParseJs(string s, string type)
        {
            bool containsNull = s.Contains(",null,");
            string[] splitted = containsNull ? s.Split(',') : s.Split(splitter, StringSplitOptions.None);
            var ii = new InstrumentInfo {Isin = StripTrailingChars(splitted[1]), Symbol = StripTrailingChars(splitted[2]), Name = "", MicDescription = StripTrailingChars(splitted[3]).Replace(@"\u00e9", "é"), Type = type};
            if (ii.MicDescription.EndsWith("Pari"))
                ii.MicDescription = ii.MicDescription + "s";
            string z = "/" + ii.Isin + "-";
            int i = splitted[0].IndexOf(z, StringComparison.Ordinal);
            s = splitted[0].Substring(i + z.Length);
            i = s.IndexOf('\\');
            ii.Mic = s.Substring(0, i);
            if (knownMicToMepDictionary.ContainsKey(ii.Mic))
                ii.Mep = knownMicToMepDictionary[ii.Mic];
            else
            {
                if (!unknownMicDictionary.ContainsKey(ii.Mic))
                    unknownMicDictionary.Add(ii.Mic, "OTH");
                ii.Mep = "OTH";
            }
            const string pattern = "target=\\\"_blank\\\"\\u003e";
            i = splitted[0].IndexOf(pattern, StringComparison.Ordinal);
            if (i > 0)
            {
                s = splitted[0].Substring(i + pattern.Length);
                i = s.IndexOf("\\u003c\\/a\\u003e", StringComparison.Ordinal);
                if (i > 0)
                    ii.Name = s.Substring(0, i);
            }
            ii.Key = string.Concat(ii.Mic, "_", ii.Symbol, "_", ii.Isin).ToUpperInvariant();
            if (instrumentInfoDictionary.ContainsKey(ii.Key))
            {
                var v = instrumentInfoDictionary[ii.Key];
                Trace.TraceError("Duplicate isin, skipping (2):");
                Trace.TraceError("(1)({0}) mep={1}, mic={2}, symbol={3}, isin={4}", v.Key, v.Mep, v.Mic, v.Symbol, v.Isin);
                Trace.TraceError("(2)({0}) mep={1}, mic={2}, symbol={3}, isin={4}", ii.Key, ii.Mep, ii.Mic, ii.Symbol, ii.Isin);
                if (v.Key != ii.Key || v.Mep != ii.Mep || v.Mic != ii.Mic || v.Symbol.ToUpperInvariant() != ii.Symbol.ToUpperInvariant() || v.Isin != ii.Isin)
                    instrumentInfoDictionary.Add(ii.Key, ii);
            }
            else
                instrumentInfoDictionary.Add(ii.Key, ii);
        }
        #endregion

        #region DownloadPost
        private static bool DownloadPost(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout, Dictionary<string, string> keyValueDictionary, string referer = null, string userAgent = null, string accept = null)
        {
            Debug.WriteLine(string.Concat("Downloading (post)", filePath, " from ", uri));
            var fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            // ReSharper disable PossibleNullReferenceException
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            // ReSharper restore PossibleNullReferenceException
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
                Thread.Sleep(1000);
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
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = postData.Length;
                    if (!string.IsNullOrEmpty(referer))
                        webRequest.Referer = referer;
                    if (string.IsNullOrEmpty(userAgent))
                        userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/16.0";
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    if (!string.IsNullOrEmpty(accept))
                        webRequest.Accept = accept;
                    webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Headers.Add("DNT", "1");
                    webRequest.KeepAlive = true;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
                    // Skip validation of SSL/TLS certificate
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;
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

        #region DownloadAndParse
        private static void DownloadAndParse(string type, string uri, string referer, string folderPath)
        {
            const int pageSize = 20;
            const string pageSizeStr = "20";
            int totalRecords = 0, page = 0;
            do
            {
                string filename = string.Format("{0}{1}_{2}.js", folderPath, type, page);
                bool status = DownloadPost(uri, filename, Properties.Settings.Default.DownloadMinimalLength, Properties.Settings.Default.DownloadOverwriteExisting, Properties.Settings.Default.DownloadRetries, Properties.Settings.Default.DownloadTimeout, new Dictionary<string, string> {{"sEcho", page.ToString(CultureInfo.InvariantCulture)}, {"iColumns", "7"}, {"sColumns", ""}, {"iDisplayStart", string.Format("{0}", page * pageSize)}, {"iDisplayLength", pageSizeStr}}, referer, null, "application/json, text/javascript, */*");
                if (!status)
                    Trace.TraceError("Failed to download \"{0}\" to \"{1}\"", uri, filename);
                if (0 == page)
                {
                    totalRecords = RetrieveTotalRecords(filename);
                    Trace.TraceInformation("{0}: total records = {1}", type, totalRecords);
                }
                ParseFile(filename, type);
                totalRecords -= pageSize;
                ++page;
            } while (totalRecords > 0);
        }
        #endregion

        #region BackupXmlFile
        private static void BackupXmlFile(string filePath, DateTime dateTime)
        {
            string suffix = dateTime.ToString("yyyyMMdd_HHmmss");
            string filePatheBackup = string.Concat(filePath, ".", suffix, ".xml");
            File.Copy(filePath, filePatheBackup);
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs an update task.
        /// </summary>
        /// <param name="downloadPath">The directory to download to.</param>
        public static void UpdateTask(string downloadPath)
        {
            DateTime dateTime = DateTime.Now;
            string separator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            string separatorAlternative = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            string folder = dateTime.ToString("yyyyMMdd");
            if (string.IsNullOrEmpty(downloadPath))
                downloadPath = "";
            else
            {
                if (!downloadPath.EndsWith(separator) && !downloadPath.EndsWith(separatorAlternative))
                    downloadPath = string.Concat(downloadPath, separator);
            }
            downloadPath = string.Concat(downloadPath, dateTime.Year.ToString(CultureInfo.InvariantCulture), separator);
            string folderPath = string.Concat(downloadPath, folder, separator);
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            Trace.TraceInformation("Downloading to {0}: {1}", folderPath, DateTime.Now);

            foreach (var category in categoryList)
            {
                DownloadAndParse(category.Type, category.Uri, category.Referer, folderPath);
            }

            string zipName = string.Concat(downloadPath, folder, "_eop.zip");
            Trace.TraceInformation("Zipping \"{0}\" to \"{1}\": {2}", folderPath, zipName, DateTime.Now);
            //Packager.ZipJsDirectory(zipName, folderPath, true);

            foreach (var v in unknownMicDictionary)
            {
                Trace.TraceError("Unknown MIC: {0}", v.Key);
                foreach (var isinInfo in instrumentInfoDictionary)
                {
                    if (isinInfo.Value.Mic == v.Key)
                        Trace.TraceError("------------ {0}, {1}", isinInfo.Key, isinInfo.Value.Type);
                }
            }

            //BackupXmlFile(Properties.Settings.Default.ApprovedIndexPath, dateTime);
            //BackupXmlFile(Properties.Settings.Default.DiscoveredIndexPath, dateTime);
            Test(dateTime);
        }
        #endregion

        private static string AttributeValue(XElement xel, string attributeName)
        {
            XAttribute attribute = xel.Attribute(attributeName);
            if (null == attribute)
                return "";
            return attribute.Value;
        }

        private static void AttributeValue(XElement xel, string attributeName, string attributeValue)
        {
            XAttribute attribute = xel.Attribute(attributeName);
            if (null == attribute)
            {
                attribute = new XAttribute(attributeName, attributeValue);
                xel.Add(attribute);
            }
            else
            {
                if (string.IsNullOrEmpty(attribute.Value))
                    attribute.Value = attributeValue;
                else if (attribute.Value != attributeValue)
                {
                    Trace.TraceWarning("Enrichment: attribute \"{0}\": replacing value: old \"{1}\" new \"{2}\", element [{3}]",
                        attributeName, attribute.Value, attributeValue, xel.ToString(SaveOptions.DisableFormatting));
                    attribute.Value = attributeValue;
                }
            }
        }

        private static bool IsMatch(XElement xel, InstrumentInfo ii)
        {
            return AttributeValue(xel, "isin") == ii.Isin && AttributeValue(xel, "mic") == ii.Mic && AttributeValue(xel, "symbol") == ii.Symbol;
        }

        private static XElement NewInstrument(InstrumentInfo ii)
        {
            var xel = new XElement("instrument");
            AttributeValue(xel, "currency", "EUR");
            AttributeValue(xel, "file", string.Concat(ii.Type, "/", ii.Symbol, ".xml"));
            AttributeValue(xel, "isin", ii.Isin);
            AttributeValue(xel, "mic", ii.Mic);
            AttributeValue(xel, "mep", ii.Mep);
            AttributeValue(xel, "name", ii.Name);
            AttributeValue(xel, "symbol", ii.Symbol);
            AttributeValue(xel, "type", ii.Type);
            AttributeValue(xel, "description", "");
            AttributeValue(xel, "vendor", "Euronext");
            switch (ii.Type)
            {
                case stock:
                    EnrichStock(xel);
                    break;
                case index:
                    NormalizeIndex(xel);
                    break;
                case etf:
                    EnrichEtf(xel);
                    break;
                case etv:
                    EnrichEtv(xel);
                    break;
            }
            return xel;
        }

        private static void ValidateInstrument(XElement xel, InstrumentInfo ii)
        {
            const string mic = "mic";
            const string mep = "mep";
            const string symbol = "symbol";
            const string name = "name";
            const string description = "description";
            const string type = "type";
            const string vendor = "vendor";

            XAttribute xatr = xel.Attribute(mic);
            if (null == xatr)
                xel.Add(new XAttribute(mic, ii.Mic));
            else if (xatr.Value != ii.Mic)
                AttributeValue(xel, mic, ii.Mic);

            xatr = xel.Attribute(mep);
            if (null == xatr)
                xel.Add(new XAttribute(mep, ii.Mep));
            else if (xatr.Value != ii.Mep)
                AttributeValue(xel, mep, ii.Mep);

            xatr = xel.Attribute(symbol);
            if (null == xatr)
                xel.Add(new XAttribute(symbol, ii.Symbol));
            else if (xatr.Value != ii.Symbol)
                AttributeValue(xel, symbol, ii.Symbol);

            xatr = xel.Attribute(name);
            if (null == xatr)
                xel.Add(new XAttribute(name, ii.Name));
            else if (xatr.Value != ii.Name)
                AttributeValue(xel, name, ii.Name);

            xatr = xel.Attribute(description);
            if (null == xatr)
                xel.Add(new XAttribute(description, ""));

            xatr = xel.Attribute(type);
            if (null == xatr)
                xel.Add(new XAttribute(type, ii.Type));
            else if (xatr.Value != ii.Type)
                AttributeValue(xel, type, ii.Type);

            xatr = xel.Attribute("file");
            if (null == xatr)
                Trace.TraceError("File attribute is not defined in element [{0}]", xel.ToString(SaveOptions.None));

            xatr = xel.Attribute(vendor);
            if (null == xatr)
                xel.Add(new XAttribute(vendor, "Euronext"));
            else if (xatr.Value != "Euronext")
                AttributeValue(xel, vendor, "Euronext");

            switch (ii.Type)
            {
                case stock:
                    EnrichStock(xel);
                    break;
                case index:
                    NormalizeIndex(xel);
                    break;
                case etf:
                    EnrichEtf(xel);
                    break;
                case etv:
                    EnrichEtv(xel);
                    break;
            }
        }

        private static string Extract(string text, string prefix, string suffix)
        {
            int i = text.IndexOf(prefix, StringComparison.Ordinal);
            if (i >= 0)
            {
                string s = text.Substring(i + prefix.Length);
                i = s.IndexOf(suffix, StringComparison.Ordinal);
                if (i > 0)
                {
                    s = s.Substring(0, i).Trim();
                    if (s == "-")
                        s = "";
                    return s;
                }
            }
            return "";
        }

        private static void NormalizeStock(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" isin="NL0000336543" symbol="BALNE" name="BALLAST NEDAM" type="stock" mic="XAMS"
            //     file="euronext/ams/stocks/eurls/loc/BALNE.xml"
            //     description="Ballast Nedam specializes in the ... sector."
            //     >
            //     <stock cfi="ES" compartment="B" tradingMode="continuous" currency="EUR" shares="1,431,522,482">
            //         <icb icb1="2000" icb2="2300" icb3="2350" icb4="2357"/>
            //     </stock>
            // </instrument>

            const string cfi = "cfi";
            const string compartment = "compartment";
            const string tradingMode = "tradingMode";
            const string currency = "currency";
            const string icb = "icb";
            const string icb1 = "icb1";
            const string icb2 = "icb2";
            const string icb3 = "icb3";
            const string icb4 = "icb4";
            const string shares = "shares";
            const string empty = "";

            XElement xelStock = xel.Element(stock);
            if (null == xelStock)
            {
                xelStock = new XElement(stock, new XAttribute(cfi, empty), new XAttribute(compartment, empty), new XAttribute(tradingMode, empty), new XAttribute(currency, empty), new XAttribute(shares, empty));
                xel.Add(xelStock);
            }
            else
            {
                if (null == xelStock.Attribute(cfi))
                    xelStock.Add(new XAttribute(cfi, empty));
                if (null == xelStock.Attribute(compartment))
                    xelStock.Add(new XAttribute(compartment, empty));
                if (null == xelStock.Attribute(tradingMode))
                    xelStock.Add(new XAttribute(tradingMode, empty));
                if (null == xelStock.Attribute(currency))
                    xelStock.Add(new XAttribute(currency, empty));
                if (null == xelStock.Attribute(shares))
                    xelStock.Add(new XAttribute(shares, empty));
            }
            XElement xelIcb = xelStock.Element(icb);
            if (null == xelIcb)
            {
                xelIcb = new XElement(icb, new XAttribute(icb1, empty), new XAttribute(icb2, empty), new XAttribute(icb3, empty), new XAttribute(icb4, empty));
                xelStock.Add(xelIcb);
            }
            else
            {
                if (null == xelIcb.Attribute(icb1))
                    xelIcb.Add(new XAttribute(icb1, empty));
                if (null == xelIcb.Attribute(icb2))
                    xelIcb.Add(new XAttribute(icb2, empty));
                if (null == xelIcb.Attribute(icb3))
                    xelIcb.Add(new XAttribute(icb3, empty));
                if (null == xelIcb.Attribute(icb4))
                    xelIcb.Add(new XAttribute(icb4, empty));
            }
            XAttribute xat = xel.Attribute(currency);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelStock.Attribute(currency);
                if (0 == xat.Value.Length)
                    xat.Value = value;
            }
        }

        private static void EnrichStock(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" isin="NL0000336543" symbol="BALNE" name="BALLAST NEDAM" type="stock" mic="XAMS"
            //     file="euronext/ams/stocks/eurls/loc/BALNE.xml"
            //     description="Ballast Nedam specializes in the ... sector."
            //     >
            //     <stock cfi="ES" compartment="B" tradingMode="continuous" currency="EUR" shares="1,431,522,482">
            //         <icb icb1="2000" icb2="2300" icb3="2350" icb4="2357"/>
            //     </stock>
            // </instrument>

            NormalizeStock(xel);
            XElement xelStock = xel.Element(stock);
            // ReSharper disable PossibleNullReferenceException
            XElement xelIcb = xelStock.Element("icb");
            // ReSharper restore PossibleNullReferenceException

            const string uriFormat = "https://europeanequities.nyx.com/en/factsheet-ajax?instrument_id={0}-{1}&instrument_type=equities";
            const string refererFormat = "https://europeanequities.nyx.com/en/products/equities/{0}-{1}";
            string isin = AttributeValue(xel, "isin");
            string mic = AttributeValue(xel, "mic");
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string factSheet = DownloadTextString("factsheet", uri,
                Properties.Settings.Default.DownloadRetries,
                Properties.Settings.Default.DownloadTimeout, referer);
            if (string.IsNullOrEmpty(factSheet))
                return;

            // <div>CFI: ESUFB</div>
            string value = Extract(factSheet, "<div>CFI:", "</div>");
            if (!string.IsNullOrEmpty(value))
                AttributeValue(xelStock, "cfi", value.ToUpperInvariant());

            // <div><span>Trading currency</span>
            // <strong>EUR</strong></div>
            value = Extract(factSheet, "<div><span>Trading currency</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelStock, "currency", value.ToUpperInvariant());
            }

            // <div><span>Trading type</span>
            // <strong>Continous</strong></div>
            value = Extract(factSheet, "<div><span>Trading type</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.ToLowerInvariant();
                    if (value == "continous")
                        value = "continuous";
                    AttributeValue(xelStock, "tradingMode", value);
                }
            }

            // <strong>Compartment A (Large Cap)</strong>
            if (factSheet.Contains("<strong>Compartment A "))
                value = "A";
            else if (factSheet.Contains("<strong>Compartment B "))
                value = "B";
            else if (factSheet.Contains("<strong>Compartment C "))
                value = "C";
            else
                value = "";
            if (0 < value.Length)
                AttributeValue(xelStock, "compartment", value);

            // <div><span>Shares outstanding</span>
            // <strong>1,431,522,482</strong></div>
            value = Extract(factSheet, "<div><span>Shares outstanding</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelStock, "shares", value.ToLowerInvariant());
            }

            // <div><span>Industry</span>
            // <strong>8000, Financials</strong></div>
            value = Extract(factSheet, "<div><span>Industry</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    int i = value.IndexOf(",", StringComparison.Ordinal);
                    if (i > 0)
                        value = value.Substring(0, i).Trim();
                    if (!string.IsNullOrEmpty(value))
                        AttributeValue(xelIcb, "icb1", value);
                }
            }

            // <div><span>SuperSector</span>
            // <strong>8300, Banks</strong></div>
            value = Extract(factSheet, "<div><span>SuperSector</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    int i = value.IndexOf(",", StringComparison.Ordinal);
                    if (i > 0)
                        value = value.Substring(0, i).Trim();
                    if (!string.IsNullOrEmpty(value))
                        AttributeValue(xelIcb, "icb2", value);
                }
            }

            // <div><span>Sector</span>
            // <strong>8350, Banks</strong></div>
            value = Extract(factSheet, "<div><span>Sector</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    int i = value.IndexOf(",", StringComparison.Ordinal);
                    if (i > 0)
                        value = value.Substring(0, i).Trim();
                    if (!string.IsNullOrEmpty(value))
                        AttributeValue(xelIcb, "icb3", value);
                }
            }

            // <div><span>Subsector</span>
            // <strong>8355, Banks</strong></div>
            value = Extract(factSheet, "<div><span>Subsector</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    int i = value.IndexOf(",", StringComparison.Ordinal);
                    if (i > 0)
                        value = value.Substring(0, i).Trim();
                    if (!string.IsNullOrEmpty(value))
                        AttributeValue(xelIcb, "icb4", value);
                }
            }
        }

        private static void NormalizeIndex(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" isin="NL0000000107" symbol="AEX" name="AEX-INDEX" type="index" mic="XAMS"
            //     file="euronext/ams/indices/AEX.xml"
            //     description="The best-known index of Euronext Amsterdam, the AEX (Price) index ... calender year."
            //     >
            //     <index kind="price" family="AEX" calcFreq="15s" baseDate="1983-01-03" baseLevel="45.378" weighting="float market cap" capFactor="0.15" currency="EUR"/>
            // </instrument>

            const string kind = "kind";
            const string family = "family";
            const string calcFreq = "calcFreq";
            const string baseDate = "baseDate";
            const string baseLevel = "baseLevel";
            const string weighting = "weighting";
            const string capFactor = "capFactor";
            const string currency = "currency";
            const string empty = "";

            XElement xelIndex = xel.Element(index);
            if (null == xelIndex)
            {
                xelIndex = new XElement(index, new XAttribute(kind, empty), new XAttribute(family, empty), new XAttribute(calcFreq, empty), new XAttribute(baseDate, empty), new XAttribute(baseLevel, empty), new XAttribute(weighting, empty), new XAttribute(capFactor, empty), new XAttribute(currency, empty));
                xel.Add(xelIndex);
            }
            else
            {
                if (null == xelIndex.Attribute(kind))
                    xelIndex.Add(new XAttribute(kind, empty));
                if (null == xelIndex.Attribute(family))
                    xelIndex.Add(new XAttribute(family, empty));
                if (null == xelIndex.Attribute(calcFreq))
                    xelIndex.Add(new XAttribute(calcFreq, empty));
                if (null == xelIndex.Attribute(baseDate))
                    xelIndex.Add(new XAttribute(baseDate, empty));
                if (null == xelIndex.Attribute(baseLevel))
                    xelIndex.Add(new XAttribute(baseLevel, empty));
                if (null == xelIndex.Attribute(weighting))
                    xelIndex.Add(new XAttribute(weighting, empty));
                if (null == xelIndex.Attribute(capFactor))
                    xelIndex.Add(new XAttribute(capFactor, empty));
                if (null == xelIndex.Attribute(currency))
                    xelIndex.Add(new XAttribute(currency, empty));
            }
            XAttribute xat = xel.Attribute(currency);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelIndex.Attribute(currency);
                if (0 == xat.Value.Length)
                    xat.Value = value;
            }
        }

        private static void NormalizeEtf(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="FR0010754135" symbol="C13" name="AMUNDI ETF EMTS1-3" type="etf"
            //     file="etf/C13.xml"
            //     description="Amundi ETF Govt Bond EuroMTS Broad 1-3"
            //     >
            //     <etf cfi="EUOM" ter="0.14" tradingMode="continuous" launchDate="20100316" currency="EUR" issuer="AMUNDI" fraction="1" dividendFrequency="Annually" indexFamily="EuroMTS" expositionType="syntetic">
            //         <inav vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011161377" symbol="INC13" name="AMUNDI C13 INAV"/>
            //         <underlying vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011052618" symbol="EMTSAR" name="EuroMTS Eurozone Government Broad 1-3"/>
            //     </etf>
            // </instrument>

            const string cfi = "cfi";
            const string tradingMode = "tradingMode";
            const string mer = "mer";
            const string ter = "ter";
            const string launchDate = "launchDate";
            const string issuer = "issuer";
            const string fraction = "fraction";
            const string dividendFrequency = "dividendFrequency";
            const string indexFamily = "indexFamily";
            const string expositionType = "expositionType";
            const string currency = "currency";
            const string vendor = "vendor";
            const string mep = "mep";
            const string mic = "mic";
            const string isin = "isin";
            const string symbol = "symbol";
            const string name = "name";
            const string euronext = "Euronext";
            const string underlying = "underlying";
            const string empty = "";

            XElement xelEtf = xel.Element(etf);
            if (null == xelEtf)
            {
                xelEtf = new XElement(etf, new XAttribute(cfi, empty), new XAttribute(tradingMode, empty), new XAttribute(ter, empty),
                    new XAttribute(launchDate, empty), new XAttribute(issuer, empty), new XAttribute(fraction, empty), new XAttribute(dividendFrequency, empty),
                    new XAttribute(indexFamily, empty), new XAttribute(expositionType, empty), new XAttribute(currency, empty));
                xel.Add(xelEtf);
            }
            else
            {
                if (null == xelEtf.Attribute(cfi))
                    xelEtf.Add(new XAttribute(cfi, empty));
                if (null == xelEtf.Attribute(tradingMode))
                    xelEtf.Add(new XAttribute(tradingMode, empty));
                if (null != xelEtf.Attribute(ter))
                    xelEtf.Add(new XAttribute(ter, empty));
                if (null == xelEtf.Attribute(launchDate))
                    xelEtf.Add(new XAttribute(launchDate, empty));
                if (null == xelEtf.Attribute(issuer))
                    xelEtf.Add(new XAttribute(issuer, empty));
                if (null == xelEtf.Attribute(fraction))
                    xelEtf.Add(new XAttribute(fraction, empty));
                if (null == xelEtf.Attribute(dividendFrequency))
                    xelEtf.Add(new XAttribute(dividendFrequency, empty));
                if (null == xelEtf.Attribute(indexFamily))
                    xelEtf.Add(new XAttribute(indexFamily, empty));
                if (null == xelEtf.Attribute(expositionType))
                    xelEtf.Add(new XAttribute(expositionType, empty));
                if (null == xelEtf.Attribute(currency))
                    xelEtf.Add(new XAttribute(currency, empty));
            }
            XAttribute xat = xel.Attribute(currency);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelEtf.Attribute(currency);
                if (0 == xat.Value.Length)
                    xat.Value = value;
            }
            xat = xel.Attribute(mer);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelEtf.Attribute(ter);
                if (0 == xat.Value.Length)
                    xat.Value = value;
            }
            XElement xelInav = xel.Element(inav);
            if (null == xelInav)
            {
                xelInav = new XElement(inav, new XAttribute(vendor, euronext), new XAttribute(mep, empty), new XAttribute(mic, empty),
                    new XAttribute(isin, empty), new XAttribute(symbol, empty), new XAttribute(name, empty));
                xelEtf.Add(xelInav);
            }
            else
            {
                if (null == xelInav.Attribute(vendor))
                    xelInav.Add(new XAttribute(vendor, euronext));
                if (null == xelInav.Attribute(mep))
                    xelInav.Add(new XAttribute(mep, empty));
                if (null != xelInav.Attribute(mic))
                    xelInav.Add(new XAttribute(mic, empty));
                if (null == xelInav.Attribute(isin))
                    xelInav.Add(new XAttribute(isin, empty));
                if (null == xelInav.Attribute(symbol))
                    xelInav.Add(new XAttribute(symbol, empty));
                if (null == xelInav.Attribute(name))
                    xelInav.Add(new XAttribute(name, empty));
            }
            XElement xelUnderlying = xel.Element(underlying);
            if (null == xelUnderlying)
            {
                xelUnderlying = new XElement(underlying, new XAttribute(vendor, euronext), new XAttribute(mep, empty), new XAttribute(mic, empty), new XAttribute(isin, empty), new XAttribute(symbol, empty), new XAttribute(name, empty));
                xelEtf.Add(xelUnderlying);
            }
            else
            {
                if (null == xelUnderlying.Attribute(vendor))
                    xelUnderlying.Add(new XAttribute(vendor, euronext));
                if (null == xelUnderlying.Attribute(mep))
                    xelUnderlying.Add(new XAttribute(mep, empty));
                if (null != xelUnderlying.Attribute(mic))
                    xelUnderlying.Add(new XAttribute(mic, empty));
                if (null == xelUnderlying.Attribute(isin))
                    xelUnderlying.Add(new XAttribute(isin, empty));
                if (null == xelUnderlying.Attribute(symbol))
                    xelUnderlying.Add(new XAttribute(symbol, empty));
                if (null == xelUnderlying.Attribute(name))
                    xelUnderlying.Add(new XAttribute(name, empty));
            }
        }

        private static void EnrichEtf(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="FR0010754135" symbol="C13" name="AMUNDI ETF EMTS1-3" type="etf"
            //     file="etf/C13.xml"
            //     description="Amundi ETF Govt Bond EuroMTS Broad 1-3"
            //     >
            //     <etf cfi="EUOM" ter="0.14" tradingMode="continuous" launchDate="20100316" currency="EUR" issuer="AMUNDI" fraction="1" dividendFrequency="Annually" indexFamily="EuroMTS" expositionType="syntetic">
            //         <inav vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011161377" symbol="INC13" name="AMUNDI C13 INAV"/>
            //         <underlying vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011052618" symbol="EMTSAR" name="EuroMTS Eurozone Government Broad 1-3"/>
            //     </etf>
            // </instrument>

            NormalizeEtf(xel);
            XElement xelEtf = xel.Element(etf);
            // ReSharper disable PossibleNullReferenceException
            XElement xelInav = xelEtf.Element(inav);
            XElement xelUnderlying = xelEtf.Element("underlying");
            // ReSharper restore PossibleNullReferenceException

            const string uriFormat = "https://etp.nyx.com/en/factsheet-ajax?instrument_id={0}-{1}&instrument_type=etfs";
            const string refererFormat = "https://etp.nyx.com/en/products/etfs/{0}-{1}";
            string isin = AttributeValue(xel, "isin");
            string mic = AttributeValue(xel, "mic");
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string factSheet = DownloadTextString("factsheet", uri,
                Properties.Settings.Default.DownloadRetries,
                Properties.Settings.Default.DownloadTimeout, referer);
            if (string.IsNullOrEmpty(factSheet))
                return;

            // <div><span>ETF Legal Name</span> <strong>AMUNDI ETF MSCI EUROPE BANKS</strong></div>
            string value = Extract(factSheet, "<div><span>ETF Legal Name</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xel, "description", value.ToUpperInvariant());
            }

            // <div>CFI: EUOMSN</div>
            value = Extract(factSheet, "<div>CFI:", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "cfi", value.ToUpperInvariant());
            }

            // <div><span>TER</span>
            // <strong>0.25%</strong></div>
            value = Extract(factSheet, "<div><span>TER</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "ter", value.ToUpperInvariant());
            }

            // <div><span>Trading type</span> <strong>Continous</strong></div>
            value = Extract(factSheet, "<div><span>Trading type</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.ToLowerInvariant();
                    if (value == "continous")
                        value = "continuous";
                    AttributeValue(xelEtf, "tradingMode", value);
                }
            }

            // <div><span>Launch Date</span> <strong>09 Dec 2008</strong></div>
            value = Extract(factSheet, "<div><span>Launch Date</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    // TODO: convert "09 Dec 2008" to yyyyMMdd
                    AttributeValue(xelEtf, "launchDate", value.ToUpperInvariant());
                }
            }

            // <div><span>Trading currency</span> <strong>EUR</strong></div>
            value = Extract(factSheet, "<div><span>Trading currency</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "currency", value.ToUpperInvariant());
            }

            // <div><span>Dividend frequency</span>
            // <strong>Annually</strong></div>
            value = Extract(factSheet, "<div><span>Dividend frequency</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "dividendFrequency", value.ToLowerInvariant());
            }

            // <div><span>Issuer Name</span> <strong>Amundi IS</strong></div>
            value = Extract(factSheet, "<div><span>Issuer Name</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "issuer", value);
            }

            // TODO: fraction ? indexFamily?

            // <div><span>Exposition type</span> <strong>Synthetic</strong></div>
            value = Extract(factSheet, "<div><span>Exposition type</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtf, "expositionType", value.ToLowerInvariant());
            }

            // <div><span>INAV ISIN code</span> <strong>QS0011146014</strong></div>
            value = Extract(factSheet, "<div><span>INAV ISIN code</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelInav, "isin", value);
            }

            // <div><span>Ticker INAV (Euronext)</span> <strong>INCB5</strong></div>
            value = Extract(factSheet, "<div><span>Ticker INAV (Euronext)</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelInav, "symbol", value);
            }

            // <div><span>INAV Name</span> <strong>AMUNDI CB5 INAV</strong></div>
            value = Extract(factSheet, "<div><span>INAV Name</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelInav, "name", value);
            }

            // <div><span>Underlying index</span> <strong>MSCI Europe Banks</strong></div>
            value = Extract(factSheet, "<div><span>Underlying index</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelUnderlying, "name", value);
            }

            // <div><span>Index</span> <strong>NDRUBANK</strong></div>
            //value = Extract(factSheet, "<div><span>Index</span>", "</div>");
            //if (!string.IsNullOrEmpty(value))
            //{
            //    value = Extract(value, "<strong>", "</strong>");
            //    if (!string.IsNullOrEmpty(value))
            //        AttributeValue(xelUnderlying, "?????", value);
            //}

            isin = AttributeValue(xelInav, "isin");
            if (string.IsNullOrEmpty(isin))
                return;

            const string searchFormat = "https://etp.nyx.com/en/search_instruments/{0}?type=Index";
            uri = string.Format(searchFormat, isin);
            string searchSheet = DownloadTextString("searchsheet", uri,
                Properties.Settings.Default.DownloadRetries,
                Properties.Settings.Default.DownloadTimeout, referer);
            if (string.IsNullOrEmpty(searchSheet))
                return;
            if (searchSheet.Contains("Showing 0 to 0 of 0"))
                return;
            string trSymbol = null, trIsin = null, trName = null, trMic = null;
            // <table id="nyx-lookup-instruments-directory-table" class="sticky-enabled">
            // <thead><tr><th>SYMBOL</th><th>NAME</th><th>ISIN</th><th>EXCHANGE</th><th>MARKET</th><th>TYPE</th> </tr></thead>
            // <tbody>
            // <tr class="odd"><td>ITAT</td><td><a href="http://indices.nyx.com/en/products/indices/QS0011245212-XPAR" target="_blank">THINKCAP TAT INAV</a></td><td>QS0011245212</td><td>NYSE Euronext Paris</td><td>XPAR</td><td>Index</td><td></td><td></td> </tr>
            // </tbody>
            // </table>
            value = Extract(searchSheet, "<table id=\"nyx-lookup-instruments-directory-table\"", "</table>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<tbody>", "</tbody>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = Extract(value, "<tr", "</tr>");
                    if (!string.IsNullOrEmpty(value))
                    {
                        string[] trSplitter = new[] { "</td><td>" };
                        string[] splitted = value.Split(trSplitter, StringSplitOptions.None);
                        if (splitted.Length > 3)
                        {
                            value = splitted[0];
                            int i = value.LastIndexOf('>');
                            if (i > 0)
                                trSymbol = value.Substring(i + 1);
                            value = splitted[1];
                            if (value.StartsWith("<a href"))
                            {
                                value = Extract(value, ">", "</a>");
                                if (!string.IsNullOrEmpty(value))
                                    trName = value;
                            }
                            value = splitted[2];
                            if (!string.IsNullOrEmpty(value))
                                trIsin = value;
                            value = splitted[4];
                            if (!string.IsNullOrEmpty(value))
                                trMic = value;
                        }
                    }
                }
            }
            if (isin != trIsin)
                return;
            if (!string.IsNullOrEmpty(trMic))
            {
                AttributeValue(xelInav, "mic", trMic);
                if (knownMicToMepDictionary.ContainsKey(trMic))
                    AttributeValue(xelInav, "mep", knownMicToMepDictionary[trMic]);
                AttributeValue(xelInav, "vendor", "Euronext");
            }
            if (!string.IsNullOrEmpty(trSymbol))
                AttributeValue(xelInav, "symbol", trSymbol);
            if (!string.IsNullOrEmpty(trName))
                AttributeValue(xelInav, "name", trName);
        }

        private static XElement InavFromEtf(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="FR0003504414" symbol="INAEX" name="SPDR AEX INAV" type="inav" currency="EUR"
            //     file="euronext/par/etf/marketIndices/nav/INAEX.xml"
            //     description="..."
            //     >
            //     <inav currency="EUR">
            //         <target vendor="Euronext" mep="AMS" isin="FR0000001893" symbol="AEXT" name="SPDR AEX ETF"/>
            //     </inav>
            // </instrument>

            XElement xelEtf = xel.Element(etf);
            if (null == xelEtf)
                return null;
            XElement xelInav = xelEtf.Element(inav);
            if (null == xelInav)
                return null;

            string inavVendor = AttributeValue(xelInav, "vendor");
            string inavMep = AttributeValue(xelInav, "mep");
            string inavMic = AttributeValue(xelInav, "mic");
            string inavIsin = AttributeValue(xelInav, "isin");
            string inavSymbol = AttributeValue(xelInav, "symbol");
            string inavName = AttributeValue(xelInav, "name");
            string inavCurrency = AttributeValue(xelEtf, "currency");

            string targetVendor = AttributeValue(xel, "vendor");
            string targetMep = AttributeValue(xel, "mep");
            string targetMic = AttributeValue(xel, "mic");
            string targetIsin = AttributeValue(xel, "isin");
            string targetSymbol = AttributeValue(xel, "symbol");
            string targetName = AttributeValue(xel, "name");
            string targetDescription = AttributeValue(xel, "description");

            if (string.IsNullOrEmpty(inavMic) || string.IsNullOrEmpty(inavIsin))
                return null;
            string inavFile = "inav/" + (string.IsNullOrEmpty(inavSymbol) ? inavIsin : inavSymbol);
            string inavDescription = "";
            if (!string.IsNullOrEmpty(targetDescription))
                inavDescription = "iNav " + targetDescription;

            var xelNew = new XElement("instrument",
                new XAttribute("file", inavFile),
                new XAttribute("mep", inavMep),
                new XAttribute("mic", inavMic),
                new XAttribute("isin", inavIsin),
                // ReSharper disable AssignNullToNotNullAttribute
                new XAttribute("symbol", inavSymbol),
                // ReSharper restore AssignNullToNotNullAttribute
                new XAttribute("name", inavName),
                new XAttribute("type", "inav"),
                new XAttribute("description", inavDescription),
                new XAttribute("vendor", inavVendor),
                new XElement("inav",
                    new XAttribute("currency", inavCurrency),
                    new XElement("target",
                        new XAttribute("vendor", targetVendor),
                        new XAttribute("mep", targetMep),
                        new XAttribute("mic", targetMic),
                        new XAttribute("isin", targetIsin),
                        new XAttribute("symbol", targetSymbol),
                        new XAttribute("name", targetName)
                )));
            return xelNew;
        }

        private static void NormalizeEtv(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="GB00B15KXP72" symbol="COFFP" name="ETFS COFFEE" type="etv"
            //     file="etf/COFFP.xml"
            //     description=""
            //     >
            //     <etv cfi="DTZSPR" tradingMode="continuous" allInFees="0,49%" expenseRatio="" dividendFrequency="yearly" currency="EUR" issuer="ETFS COMMODITY SECURITIES LTD" shares="944,000">
            // </instrument>

            const string cfi = "cfi";
            const string tradingMode = "tradingMode";
            const string allInFees = "allInFees";
            const string expenseRatio = "expenseRatio";
            const string issuer = "issuer";
            const string shares = "shares";
            const string dividendFrequency = "dividendFrequency";
            const string currency = "currency";
            const string type = "type";
            const string file = "file";
            const string empty = "";

            AttributeValue(xel, type, etv);
            string fileOld = AttributeValue(xel, file);
            if (fileOld.Contains("etf"))
            {
                Trace.TraceInformation("ETV: has ETF file [{0}] in [{1}]", fileOld, xel.ToString(SaveOptions.None));
            }
            /*if (fileOld.Contains("/etf/commodities/"))
            {
                string fileNew = fileOld.Replace("/etf/commodities/", "/etv/");
                Trace.TraceInformation("ETV: replacing file [{0}] with {{1}] inelement [{3}]", fileOld, fileNew, xel.ToString(SaveOptions.None));
                AttributeValue(xel, file, fileNew);
            }*/

            XElement xelEtv = xel.Element(etv);
            if (null == xelEtv)
            {
                xelEtv = new XElement(etv, new XAttribute(cfi, empty), new XAttribute(tradingMode, empty), new XAttribute(allInFees, empty),
                    new XAttribute(expenseRatio, empty), new XAttribute(dividendFrequency, empty), new XAttribute(currency, empty),
                    new XAttribute(issuer, empty), new XAttribute(shares, empty));
                xel.Add(xelEtv);
            }
            else
            {
                if (null == xelEtv.Attribute(cfi))
                    xelEtv.Add(new XAttribute(cfi, empty));
                if (null == xelEtv.Attribute(tradingMode))
                    xelEtv.Add(new XAttribute(tradingMode, empty));
                if (null == xelEtv.Attribute(allInFees))
                    xelEtv.Add(new XAttribute(allInFees, empty));
                if (null == xelEtv.Attribute(expenseRatio))
                    xelEtv.Add(new XAttribute(expenseRatio, empty));
                if (null == xelEtv.Attribute(dividendFrequency))
                    xelEtv.Add(new XAttribute(dividendFrequency, empty));
                if (null == xelEtv.Attribute(currency))
                    xelEtv.Add(new XAttribute(currency, empty));
                if (null == xelEtv.Attribute(issuer))
                    xelEtv.Add(new XAttribute(issuer, empty));
                if (null == xelEtv.Attribute(shares))
                    xelEtv.Add(new XAttribute(shares, empty));
            }
            XAttribute xat = xel.Attribute(currency);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelEtv.Attribute(currency);
                if (0 == xat.Value.Length)
                    xat.Value = value;
            }
        }

        private static void EnrichEtv(XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="GB00B15KXP72" symbol="COFFP" name="ETFS COFFEE" type="etv"
            //     file="etf/COFFP.xml"
            //     description=""
            //     >
            //     <etv cfi="DTZSPR" tradingMode="continuous" allInFees="0,49%" expenseRatio="" dividendFrequency="yearly" currency="EUR" issuer="ETFS COMMODITY SECURITIES LTD" shares="944,000">
            // </instrument>

            const string cfi = "cfi";
            const string tradingMode = "tradingMode";
            const string allInFees = "allInFees";
            const string expenseRatio = "expenseRatio";
            const string issuer = "issuer";
            const string shares = "shares";
            const string dividendFrequency = "dividendFrequency";
            const string currency = "currency";

            NormalizeEtv(xel);
            XElement xelEtv = xel.Element(etv);

            const string uriFormat = "https://etp.nyx.com/en/factsheet-ajax?instrument_id={0}-{1}&instrument_type=etvs";
            const string refererFormat = "https://etp.nyx.com/en/products/etvs/{0}-{1}";
            string isin = AttributeValue(xel, "isin");
            string mic = AttributeValue(xel, "mic");
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string factSheet = DownloadTextString("factsheet", uri,
                Properties.Settings.Default.DownloadRetries,
                Properties.Settings.Default.DownloadTimeout, referer);
            if (string.IsNullOrEmpty(factSheet))
                return;

            // <div>CFI: DTZSPR</div>
            string value = Extract(factSheet, "<div>CFI:", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, cfi, value.ToUpperInvariant());
            }

            // <div><span>All In Fees</span>
            // <strong>0,49%</strong></div>
            value = Extract(factSheet, "<div><span>All In Fees</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, allInFees, value);
            }

            // <div><span>Trading type</span> <strong>Continous</strong></div>
            value = Extract(factSheet, "<div><span>Trading type</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.ToLowerInvariant();
                    if (value == "continous")
                        value = "continuous";
                    AttributeValue(xelEtv, tradingMode, value);
                }
            }

            // <div><span>Trading currency</span> <strong>EUR</strong></div>
            value = Extract(factSheet, "<div><span>Trading currency</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, currency, value.ToUpperInvariant());
            }

            // <div><span>Dividend frequency</span>
            // <strong>Annually</strong></div>
            value = Extract(factSheet, "<div><span>Dividend frequency</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, dividendFrequency, value.ToLowerInvariant());
            }

            // <div><span>Expense Ratio</span>
            // <strong>-</strong></div>
            value = Extract(factSheet, "<div><span>Expense Ratio</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, expenseRatio, value);
            }

            // <div><span>Issuer Name</span> <strong>ETFS COMMODITY SECURITIES LTD</strong></div>
            value = Extract(factSheet, "<div><span>Issuer Name</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, issuer, value);
            }

            // <div><span>Shares Outstanding</span> <strong>944,000</strong></div>
            value = Extract(factSheet, "<div><span>Shares Outstanding</span>", "</div>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<strong>", "</strong>");
                if (!string.IsNullOrEmpty(value))
                    AttributeValue(xelEtv, shares, value);
            }

            const string searchFormat = "https://etp.nyx.com/en/search_instruments/{0}?type=Trackers";
            uri = string.Format(searchFormat, isin);
            string searchSheet = DownloadTextString("searchsheet", uri,
                Properties.Settings.Default.DownloadRetries,
                Properties.Settings.Default.DownloadTimeout, referer);
            if (string.IsNullOrEmpty(searchSheet))
                return;
            if (searchSheet.Contains("Showing 0 to 0 of 0"))
                return;
            string trSymbol = null, trIsin = null, trName = null, trMic = null;
            // <table id="nyx-lookup-instruments-directory-table" class="sticky-enabled">
            // <thead><tr><th>SYMBOL</th><th>NAME</th><th>ISIN</th><th>EXCHANGE</th><th>MARKET</th><th>TYPE</th> </tr></thead>
            // <tbody>
            // <tr class="odd"><td>ITAT</td><td><a href="http://indices.nyx.com/en/products/indices/QS0011245212-XPAR" target="_blank">THINKCAP TAT INAV</a></td><td>QS0011245212</td><td>NYSE Euronext Paris</td><td>XPAR</td><td>Index</td><td></td><td></td> </tr>
            // </tbody>
            // </table>
            value = Extract(searchSheet, "<table id=\"nyx-lookup-instruments-directory-table\"", "</table>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<tbody>", "</tbody>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = Extract(value, "<tr", "</tr>");
                    if (!string.IsNullOrEmpty(value))
                    {
                        string[] trSplitter = new[] { "</td><td>" };
                        string[] splitted = value.Split(trSplitter, StringSplitOptions.None);
                        if (splitted.Length > 3)
                        {
                            value = splitted[0];
                            int i = value.LastIndexOf('>');
                            if (i > 0)
                                trSymbol = value.Substring(i + 1);
                            value = splitted[1];
                            if (value.StartsWith("<a href"))
                            {
                                value = Extract(value, ">", "</a>");
                                if (!string.IsNullOrEmpty(value))
                                    trName = value;
                            }
                            value = splitted[2];
                            if (!string.IsNullOrEmpty(value))
                                trIsin = value;
                            value = splitted[4];
                            if (!string.IsNullOrEmpty(value))
                                trMic = value;
                        }
                    }
                }
            }
            if (isin != trIsin)
                return;
            if (!string.IsNullOrEmpty(trMic))
            {
                AttributeValue(xel, "mic", trMic);
                if (knownMicToMepDictionary.ContainsKey(trMic))
                    AttributeValue(xel, "mep", knownMicToMepDictionary[trMic]);
                AttributeValue(xel, "vendor", "Euronext");
            }
            if (!string.IsNullOrEmpty(trSymbol))
                AttributeValue(xel, "symbol", trSymbol);
            if (!string.IsNullOrEmpty(trName))
                AttributeValue(xel, "name", trName);
        }

        #region DownloadTextString
        private static string DownloadTextString(string what, string uri, int retries, int timeout, string referer = null, string userAgent = null)
        {
            Debug.WriteLine(string.Concat("Downloading (get) ", what, " from ", uri));
            while (0 < retries)
            {
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
                    if (!string.IsNullOrEmpty(referer))
                        webRequest.Referer = referer;
                    if (string.IsNullOrEmpty(userAgent))
                        userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/16.0";
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    webRequest.Accept = "text/html, */*";
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Headers.Add("DNT", "1");
                    webRequest.KeepAlive = true;
                    WebResponse webResponse = webRequest.GetResponse();
                    Stream responseStream = webResponse.GetResponseStream();
                    if (null == responseStream)
                    {
                        Trace.TraceError("Received null response stream.");
                        return null;
                    }
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
                catch (Exception exception)
                {
                    Trace.TraceError(1 < retries ?
                        "Dwnload failed [{0}], retrying ({1})" : "Download failed [{0}], giving up ({1})",
                        exception.Message, retries);
                    retries--;
                   Thread.Sleep(1000);
                }
            }
            return null;
        }
        #endregion

        private static void TestOrig(DateTime dateTime)
        {
            XDocument xdocApproved = XDocument.Load(Properties.Settings.Default.ApprovedIndexPath,
                /*LoadOptions.PreserveWhitespace |*/ LoadOptions.SetLineInfo);
            List<XElement> xelistApproved = xdocApproved.XPathSelectElements("/instruments/instrument").ToList();

            XDocument xdocDiscovered = XDocument.Load(Properties.Settings.Default.DiscoveredIndexPath,
                /*LoadOptions.PreserveWhitespace |*/ LoadOptions.SetLineInfo);
            List<XElement> xelistDiscovered = xdocDiscovered.XPathSelectElements("/instruments/instrument").ToList();

            xdocDiscovered.XPathSelectElement("/instruments").Add(new XComment(dateTime.ToString(" yyyyMMdd_HHmmss ")));

            foreach (var kvp in instrumentInfoDictionary)
            {
                var ii = kvp.Value;
                if (ii.Type != etv)
                    continue;
                //if (ii.Type != stock && ii.Type != index && ii.Type != etf)
                //    continue;
                List<XElement> matchListApproved = xelistApproved.FindAll(xel => IsMatch(xel, ii));
                ii.IsApproved = matchListApproved.Count > 0;
                if (matchListApproved.Count > 1)
                {
                    Trace.TraceError("Approved: {0} duplicate matches for \"{1}\":", matchListApproved.Count, ii.Key);
                    int i = 0;
                    foreach (var xElement in matchListApproved)
                    {
                        Trace.TraceError("{0}: element [{1}]", ++i, xElement.ToString(SaveOptions.None));
                    }
                }
                List<XElement> matchListDiscovered = xelistDiscovered.FindAll(xel => IsMatch(xel, ii));
                ii.IsDiscovered = matchListDiscovered.Count > 0;
                if (matchListDiscovered.Count > 1)
                {
                    Trace.TraceError("Discovered: {0} duplicate matches for \"{1}\":", matchListDiscovered.Count, ii.Key);
                    int i = 0;
                    foreach (var xElement in matchListDiscovered)
                    {
                        Trace.TraceError("{0}: element [{1}]", ++i, xElement.ToString(SaveOptions.None));
                    }
                }
                if (!ii.IsApproved && !ii.IsDiscovered)
                {
                    Trace.TraceInformation("Discovered {0}: \"{1}\":", ii.Type, ii.Key);
                    XElement xel = xdocDiscovered.XPathSelectElement("/instruments");
                    XElement xelNew = NewInstrument(ii);
                    xel.Add(xelNew);
                    if (ii.Type == etf)
                    {
                        XElement xelInav = InavFromEtf(xelNew);
                        if (null != xelInav)
                            xel.Add(xelInav);
                    }
                }
                else if (true)
                {
                    if (ii.IsApproved && ii.IsDiscovered)
                    {
                        Trace.TraceError("Found both {0} approved and {1} discovered match(es) for \"{1}\":", matchListApproved.Count, matchListDiscovered.Count, ii.Key);
                        int i = 0;
                        foreach (var xElement in matchListApproved)
                        {
                            Trace.TraceError("{0}: approved element [{1}]", ++i, xElement.ToString(SaveOptions.None));
                        }
                        foreach (var xElement in matchListDiscovered)
                        {
                            Trace.TraceError("{0}: discovered element [{1}]", ++i, xElement.ToString(SaveOptions.None));
                        }
                    }
                    if (ii.IsApproved)
                    {
                        Trace.TraceInformation("Normalizing/Enriching approved {0}: \"{1}\":", ii.Type, ii.Key);
                        ValidateInstrument(matchListApproved[0], ii);
                    }
                    if (ii.IsDiscovered)
                    {
                        Trace.TraceInformation("Normalizing/Enriching discovered {0}: \"{1}\":", ii.Type, ii.Key);
                        ValidateInstrument(matchListDiscovered[0], ii);
                    }
                }
            }
            xdocApproved.Save(Properties.Settings.Default.ApprovedIndexPath, SaveOptions.None);
            string foo = xdocApproved.ToString(SaveOptions.None);
            xdocDiscovered.Save(Properties.Settings.Default.DiscoveredIndexPath, SaveOptions.None);
            
        }

        private static void Test(DateTime dateTime)
        {
            XDocument xdocApproved = XDocument.Load(Properties.Settings.Default.ApprovedIndexPath,
                /*LoadOptions.PreserveWhitespace |*/ LoadOptions.SetLineInfo);
            List<XElement> xelistApproved = xdocApproved.XPathSelectElements("/instruments/instrument").ToList();

            XDocument xdocDiscovered = XDocument.Load(Properties.Settings.Default.DiscoveredIndexPath,
                /*LoadOptions.PreserveWhitespace |*/ LoadOptions.SetLineInfo);
            List<XElement> xelistDiscovered = xdocDiscovered.XPathSelectElements("/instruments/instrument").ToList();

            //xdocDiscovered.XPathSelectElement("/instruments").Add(new XComment(dateTime.ToString(" yyyyMMdd_HHmmss ")));

            foreach (var xel in xelistApproved)
            {
                string type = AttributeValue(xel, "type");

                XAttribute xatr = xel.Attribute("mic");
                if (null == xatr)
                    xel.Add(new XAttribute("mic", ""));

                xatr = xel.Attribute("mep");
                if (null == xatr)
                    xel.Add(new XAttribute("mep", ""));

                xatr = xel.Attribute("symbol");
                if (null == xatr)
                    xel.Add(new XAttribute("symbol", ""));

                xatr = xel.Attribute("name");
                if (null == xatr)
                    xel.Add(new XAttribute("name", ""));

                xatr = xel.Attribute("description");
                if (null == xatr)
                    xel.Add(new XAttribute("description", ""));

                xatr = xel.Attribute("file");
                if (null == xatr)
                    Trace.TraceError("File attribute is not defined in element [{0}]", xel.ToString(SaveOptions.None));

                xatr = xel.Attribute("vendor");
                if (null == xatr)
                    xel.Add(new XAttribute("vendor", "Euronext"));

                switch (type)
                {
                    case stock:
                        EnrichStock(xel);
                        break;
                    case index:
                        NormalizeIndex(xel);
                        break;
                    case etf:
                        EnrichEtf(xel);
                        break;
                    case etv:
                        EnrichEtv(xel);
                        break;
                }
            }
            xdocApproved.Save(Properties.Settings.Default.ApprovedIndexPath, SaveOptions.None);
            string foo = xdocApproved.ToString(SaveOptions.None);
            xdocDiscovered.Save(Properties.Settings.Default.DiscoveredIndexPath, SaveOptions.None);

        }
    }
}
