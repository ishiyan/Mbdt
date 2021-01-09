using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;
using System.Threading;
using mbdt.Utils;

namespace mbdt.Euronext
{
    /// <summary>
    /// Fetches the actual instrument lists from the Euronext.
    /// </summary>
    internal static class EuronextActualInstruments
    {
        #region InstrumentInfo
        internal class InstrumentInfo
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

        private static bool firstTime = true;

        static EuronextActualInstruments()
        {
            // Skip validation of SSL/TLS certificate
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Ssl3;
        }

        #region CategoryInfo
        private class CategoryInfo
        {
            internal string Type;
            internal string Uri;
            internal string Referer;
            internal string FileName;
        }
        #endregion

        #region KnownMicToMepDictionary
        internal static Dictionary<string, string> KnownMicToMepDictionary
        {
            get { return knownMicToMepDictionary; }
        }
        private static readonly Dictionary<string, string> knownMicToMepDictionary = CreateKnownMicToMepDictionary();
        #endregion

        #region UnknownMicDictionary
        internal static Dictionary<string, string> UnknownMicDictionary
        {
            get { return unknownMicDictionary; }
        }
        private static Dictionary<string, string> unknownMicDictionary = new Dictionary<string, string>();
        #endregion

        #region DownloadTimeout
        /// <summary>
        /// In milliseconds.
        /// </summary>
        internal static int DownloadTimeout = 180000;
        #endregion

        #region DownloadOverwriteExisting
        internal static bool DownloadOverwriteExisting;
        #endregion

        #region DownloadRetries
        internal static int DownloadRetries = 12;
        #endregion

        #region PauseBeforeRetry
        /// <summary>
        /// In milliseconds.
        /// </summary>
        private const int PauseBeforeRetry = 3000;
        #endregion

        private static Dictionary<string, InstrumentInfo> instrumentInfoDictionary = new Dictionary<string, InstrumentInfo>();
        private static readonly string[] splitter = { @""",""" };
        private const string defaultUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/16.0";
        private static Dictionary<string, string> bodyDictionary = new Dictionary<string, string>
        {
            {"start", "0" },
            {"length", "30000" },
            {"iDisplayStart", "0" },
            {"iDisplayLength", "30000" }
        };

        #region CategoryList
        private static readonly List<CategoryInfo> categoryList = CreateCategoryList();

        private static List<CategoryInfo> CreateCategoryList()
        {
            var list = new List<CategoryInfo>
            {
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + AmsMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_ams"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + BruMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_bru"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + ParMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_par"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + LisMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_lis"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + DubMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_dub"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + OslMics,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_osl"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + LonMics1,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_lon1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + LonMics2,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_lon2"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + OthMics1,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_oth1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Stock,
                    Uri = "https://live.euronext.com/en/pd/data/stocks?mics=" + OthMics2,
                    Referer = "https://live.euronext.com/products/equities/list",
                    FileName = "stocks_oth2"
                },

                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + AmsMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_ams"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + BruMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_bru"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + ParMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_par"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + LisMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_lis"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + DubMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_dub"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + OslMics,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_osl"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + LonMics1,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_lon1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + LonMics2,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_lon2"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + OthMics1,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_oth1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Index,
                    Uri = "https://live.euronext.com/en/pd/data/index?mics=" + OthMics2,
                    Referer = "https://live.euronext.com/products/indices/list",
                    FileName = "indices_oth2"
                },

                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + AmsMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_ams"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + BruMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_bru"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + ParMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_par"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + LisMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_lis"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + DubMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_dub"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + OslMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_osl"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + LonMics1,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_lon1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + LonMics2,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_lon2"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + OthMics1,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_oth1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etv,
                    Uri = "https://live.euronext.com/en/pd/data/etv?mics=" + OthMics2,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etvs_oth2"
                },

                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + AmsMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_ams"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + BruMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_bru"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + ParMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_par"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + LisMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_lis"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + DubMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_dub"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + OslMics,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_osl"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + LonMics1,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_lon1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + LonMics2,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_lon2"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + OthMics1,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_oth1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Etf,
                    Uri = "https://live.euronext.com/en/pd/data/track?mics=" + OthMics2,
                    Referer = "https://live.euronext.com/products/etfs/list",
                    FileName = "etfs_oth2"
                },

                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + AmsMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_ams"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + BruMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_bru"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + ParMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_par"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + LisMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_lis"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + DubMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_dub"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + OslMics,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_osl"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + LonMics1,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_lon1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + LonMics2,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_lon2"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + OthMics1,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_oth1"
                },
                new CategoryInfo
                {
                    Type = EuronextInstrumentXml.Fund,
                    Uri = "https://live.euronext.com/en/pd/data/funds?mics=" + OthMics2,
                    Referer = "https://live.euronext.com/products/funds/list",
                    FileName = "funds_oth2"
                }
            };
            return list;
        }
        #endregion

        #region CreateKnownMicToMepDictionary
        private static Dictionary<string, string> CreateKnownMicToMepDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "ALXA", "AMS" }, { "ALXB", "BRU" }, { "ALXL", "LIS" }, { "ALXP", "PAR" },
                { "ENXB", "BRU" }, { "ENXL", "LIS" }, { "MLXB", "BRU" }, { "TNLA", "AMS" },
                { "TNLB", "BRU" }, { "XMLI", "PAR" }, { "XAMS", "AMS" }, { "XBRU", "BRU" },
                { "XLIS", "LIS" }, { "XPAR", "PAR" }, { "XHFT", "OTH" }, //{ "DLON", "LON" },
                { "XLIF", "LON" }, { "XLDN", "LON" },
                { "XETR", "OTH" }, { "XLON", "OTH" }, { "XMCE", "OTH" }, { "XVTX", "OTH" },
                { "MTAA", "OTH" }, { "FRAA", "OTH" }, { "XCSE", "OTH" }, { "XSTO", "OTH" },
                { "XHEL", "OTH" }, { "XMAD", "OTH" }, { "XIST", "OTH" },
                { "XESM", "DUB" }, { "XMSM", "DUB" }, {"XDUB", "DUB"},
                { "XOSL", "OSL" }, { "XOAS", "OSL" }, { "MERK", "OSL" }, { "VPXB", "OSL" },
            };
            return dictionary;
        }

        private const string AmsMics = "XAMS,ALXA,TNLA";
        private const string BruMics = "XBRU,ALXB,ENXB,MLXB,TNLB";
        private const string ParMics = "XPAR,ALXP,XMLI";
        private const string LisMics = "XLIS,ALXL,ENXL";
        private const string DubMics = "XDUB,XMSM,XESM";
        private const string OslMics = "XOSL,XOAS,MERK,VPXB";
        private const string LonMics1 = "XLDN,XLIF";
        private const string LonMics2 = "XLON";
        private const string OthMics1 = "XETR";
        private const string OthMics2 = "XMCE,XVTX,MTAA,FRAA,XCSE,XSTO,XHEL,XMAD,XIST,XHFT";
        #endregion

        #region RetrieveTotalRecords
        private static int RetrieveTotalRecords(string filename)
        {
            // {"iTotalRecords":null,"iTotalDisplayRecords":null,"aaData":[]}
            // {"iTotalRecords":56,"iTotalDisplayRecords":56,"aaData":[[
            const string prefix = @"{""iTotalRecords"":";
            const string prefix2 = @"{""iTotalRecords"":0,";
            const string prefix3 = @"{""iTotalRecords"":null,";
            int totalRecords = 0;
            string s = File.ReadAllText(filename);
            if (s.StartsWith(prefix2) || s.StartsWith(prefix3))
                return totalRecords;
            if (s.StartsWith(prefix))
            {
                string sub = s.Substring(prefix.Length); // null,"iTotalDisplayRecords"  or  56,"iTotalDisplayRecords"
                totalRecords = sub.TakeWhile(char.IsDigit).Aggregate(totalRecords, (current, c) => 10 * current + (c - '0'));
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
                while ((i = content.IndexOf("],[", StringComparison.Ordinal)) >= 0)
                {
                    ParseJson(content.Substring(0, i), type);
                    content = content.Substring(i + 3);
                }
                i = content.IndexOf("]]", StringComparison.Ordinal);
                if (i >= 0)
                    ParseJson(content.Substring(0, i), type);
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

        #region ParseJson
        private static void ParseJson(string s, string type)
        {
            bool containsNull = s.Contains("\"aaData\": []");
            if (containsNull)
                return;
            if (s.Contains(",null,"))
            {
                s = s.Replace(",null,", ",\"null\",");
            }

            string[] splitted = s.Split(splitter, StringSplitOptions.None);
            if (splitted.Length < 7)
            {
                Trace.TraceError("splitted array has length {0} instead of 7, skipping {1}", splitted.Length, s);
                return;
            }
            var ii = new InstrumentInfo
            {
                Isin = StripTrailingChars(splitted[1]), Symbol = StripTrailingChars(splitted[2]), Name = "",
                MicDescription = StripTrailingChars(splitted[3]).Replace(@"\u00e9", "é"), Type = type
            };
            if (ii.Isin == "null")
                ii.Isin = string.Empty;
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
            const string pattern = @"\u0027\u003E";
            i = splitted[0].IndexOf(pattern, StringComparison.Ordinal);
            if (i > 0)
            {
                s = splitted[0].Substring(i + pattern.Length);
                i = s.IndexOf(@"\u003C\/a\u003E", StringComparison.Ordinal);
                if (i > 0)
                    ii.Name = s.Substring(0, i).Replace(@"\u00E9", "é").Replace(@"\u0026", "&").Replace("&#039;", "'").Replace("&amp;", "&");
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
            if (firstTime)
            {
                firstTime = false;
                if (DownloadPost(uri, filePath, minimalLength, overwrite, retries, timeout, keyValueDictionary, referer, userAgent, accept))
                    return true;
            }
            Debug.WriteLine(string.Concat("Downloading (post)", filePath, " from ", uri));
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
                Thread.Sleep(PauseBeforeRetry);
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
                        userAgent = defaultUserAgent;
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    if (!string.IsNullOrEmpty(accept))
                        webRequest.Accept = accept;
                    //webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Headers.Add("DNT", "1");
                    webRequest.KeepAlive = true;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
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
        private static void DownloadAndParse(string type, string uri, string referer, string folderPath, string fileName, string userAgent = defaultUserAgent)
        {
            const int downloadMinimalLength = 2;
            int totalRecords = 0;
            string filename = string.Format("{0}{1}.json", folderPath, fileName);
            bool status = DownloadPost(uri, filename, downloadMinimalLength, DownloadOverwriteExisting, DownloadRetries,
                DownloadTimeout, bodyDictionary,
                referer, userAgent, "application/json, text/javascript, */*");
            if (!status)
                Trace.TraceError("Failed to download \"{0}\" to \"{1}\"", uri, filename);
            totalRecords = RetrieveTotalRecords(filename);
            Trace.TraceInformation("{0}: total records = {1}", fileName, totalRecords);
            ParseFile(filename, type);
        }
        #endregion

        #region Fetch
        /// <summary>
        /// Downloads a list of actual instruments from the Euronext.
        /// </summary>
        /// <param name="downloadPath">The folder to download to.</param>
        /// <param name="zipDownloadPath">Whether to zip downloaded folder.</param>
        /// <param name="deleteDownloadPath">Whether to delete downloaded folder.</param>
        internal static Dictionary<string, InstrumentInfo> Fetch(string downloadPath, bool zipDownloadPath = true, bool deleteDownloadPath = true, string userAgent = null)
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

            unknownMicDictionary = new Dictionary<string, string>();
            instrumentInfoDictionary = new Dictionary<string, InstrumentInfo>();
            foreach (var category in categoryList)
            {
                DownloadAndParse(category.Type, category.Uri, category.Referer, folderPath, category.FileName, userAgent);
            }

            if (zipDownloadPath)
            {
                string zipName = string.Concat(downloadPath, folder, "_eop.zip");
                Trace.TraceInformation("Zipping \"{0}\" to \"{1}\": {2}", folderPath, zipName, DateTime.Now);
                Packager.ZipJsDirectory(zipName, folderPath, deleteDownloadPath);
            }
            else if (deleteDownloadPath)
                Directory.Delete(folderPath, true);

            return instrumentInfoDictionary;
        }
        #endregion
    }
}
