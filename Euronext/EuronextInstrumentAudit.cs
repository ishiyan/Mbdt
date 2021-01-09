using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext instrument auditing utilities.
    /// </summary>
    internal static class EuronextInstrumentAudit
    {
        #region Foo List<XElement> xelist
        #endregion

        #region InstrumentsWithoutMic
        internal static void InstrumentsWithoutMic(string indexPath, string indexType, bool addMic = true, bool addFromSearch = true)
        {
            DateTime dateTime = DateTime.Now;

            XDocument xdoc = XDocument.Load(indexPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();

            long count = 0;
            foreach (var xel in xelist)
            {
                string attr = xel.AttributeValue(EuronextInstrumentXml.Mic);
                if (attr == "")
                {
                    Trace.TraceError("Missing mic in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                    if (addMic)
                    {
                        if (addFromSearch)
                        {
                            string searchSymbol, searchIsin, searchName, searchMicName, searchMic, searchType;
                            if (EuronextInstrumentEnrichment.SearchFirstInstrument(xel.AttributeValue(EuronextInstrumentXml.Isin), xel.AttributeValue(EuronextInstrumentXml.Type), out searchIsin, out searchMic, out searchMicName, out searchSymbol, out searchName, out searchType))
                            {
                                if (!string.IsNullOrEmpty(searchMic))
                                    xel.SetAttributeValue(EuronextInstrumentXml.Mic, searchMic);
                            }
                        }
                        attr = xel.AttributeValue(EuronextInstrumentXml.Mic);
                        if (attr == "")
                        {
                            switch (xel.AttributeValue(EuronextInstrumentXml.Mep))
                            {
                                case "PAR":
                                    attr = "XPAR";
                                    break;
                                case "AMS":
                                    attr = "XAMS";
                                    break;
                                case "BRU":
                                    attr = "XBRU";
                                    break;
                                case "LIS":
                                    attr = "XLIS";
                                    break;
                                case "DUB":
                                    attr = "XDUB";
                                    break;
                            }
                            if (attr != "")
                                xel.SetAttributeValue(EuronextInstrumentXml.Mic, attr);
                        }
                        if (attr != "")
                            ++count;
                    }
                }
            }
            if (count > 0)
            {
                EuronextInstrumentXml.BackupXmlFile(indexPath, dateTime);
                xdoc.Save(indexPath, SaveOptions.None);
            }
        }
        #endregion

        #region InstrumentsWithEqualIsin
        internal static void InstrumentsWithEqualIsin(string approvedIndexPath, string discoveredIndexPath, string deadIndexPath)
        {
            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Scanning instruments with duplicate isin");
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            const string indexType = "_index_type_";
            var xelist = new List<XElement>();
            if (!string.IsNullOrEmpty(approvedIndexPath))
            {
                XDocument xdocA = XDocument.Load(approvedIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement>  xelistA = xdocA.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistA)
                    v.SetAttributeValue(indexType, "approved");
                xelist.AddRange(xelistA);
            }
            if (!string.IsNullOrEmpty(discoveredIndexPath))
            {
                XDocument xdocD = XDocument.Load(discoveredIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistD = xdocD.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistD)
                    v.SetAttributeValue(indexType, "discovered");
                xelist.AddRange(xelistD);
            }
            if (!string.IsNullOrEmpty(deadIndexPath))
            {
                XDocument xdocX = XDocument.Load(deadIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistX = xdocX.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistX)
                    v.SetAttributeValue(indexType, "dead");
                xelist.AddRange(xelistX);
            }
            var dictionary = new Dictionary<string, List<XElement>>();
            foreach (var v in xelist)
            {
                List<XElement> list;
                string isin = v.AttributeValue(EuronextInstrumentXml.Isin);
                if (dictionary.TryGetValue(isin, out list))
                    list.Add(v);
                else
                {
                    list = new List<XElement> {v};
                    dictionary.Add(isin, list);
                }
            }
            foreach (var kvp in dictionary)
            {
                if (kvp.Value.Count > 1)
                {
                    Trace.WriteLine("");
                    Trace.TraceError("Duplicate isin [{0}], found {1} times:", kvp.Key, kvp.Value.Count);
                    foreach (var v in kvp.Value)
                    {
                        Trace.WriteLine(v.ToString(SaveOptions.None));
                    }
                    Trace.WriteLine("");
                }
            }
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
        }
        #endregion

        #region InstrumentsWithEqualFile
        internal static void InstrumentsWithEqualFile(string approvedIndexPath, string discoveredIndexPath, string deadIndexPath)
        {
            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Scanning instruments with duplicate file");
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            const string indexType = "_index_type_";
            var xelist = new List<XElement>();
            if (!string.IsNullOrEmpty(approvedIndexPath))
            {
                XDocument xdocA = XDocument.Load(approvedIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistA = xdocA.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistA)
                    v.SetAttributeValue(indexType, "approved");
                xelist.AddRange(xelistA);
            }
            if (!string.IsNullOrEmpty(discoveredIndexPath))
            {
                XDocument xdocD = XDocument.Load(discoveredIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistD = xdocD.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistD)
                    v.SetAttributeValue(indexType, "discovered");
                xelist.AddRange(xelistD);
            }
            if (!string.IsNullOrEmpty(deadIndexPath))
            {
                XDocument xdocX = XDocument.Load(deadIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistX = xdocX.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistX)
                    v.SetAttributeValue(indexType, "dead");
                xelist.AddRange(xelistX);
            }
            var dictionary = new Dictionary<string, List<XElement>>();
            foreach (var v in xelist)
            {
                List<XElement> list;
                string key = v.AttributeValue(EuronextInstrumentXml.File);
                if (dictionary.TryGetValue(key, out list))
                    list.Add(v);
                else
                {
                    list = new List<XElement> { v };
                    dictionary.Add(key, list);
                }
            }
            foreach (var kvp in dictionary)
            {
                if (kvp.Value.Count > 1)
                {
                    Trace.WriteLine("");
                    Trace.TraceError("Duplicate file [{0}], found {1} times:", kvp.Key, kvp.Value.Count);
                    foreach (var v in kvp.Value)
                    {
                        Trace.WriteLine(v.ToString(SaveOptions.None));
                    }
                    Trace.WriteLine("");
                }
            }
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
        }
        #endregion

        #region InstrumentsWithEqualSymbol
        internal static void InstrumentsWithEqualSymbol(string approvedIndexPath, string discoveredIndexPath, string deadIndexPath)
        {
            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Scanning instruments with duplicate symbol");
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            const string indexType = "_index_type_";
            var xelist = new List<XElement>();
            if (!string.IsNullOrEmpty(approvedIndexPath))
            {
                XDocument xdocA = XDocument.Load(approvedIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistA = xdocA.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistA)
                    v.SetAttributeValue(indexType, "approved");
                xelist.AddRange(xelistA);
            }
            if (!string.IsNullOrEmpty(discoveredIndexPath))
            {
                XDocument xdocD = XDocument.Load(discoveredIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistD = xdocD.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistD)
                    v.SetAttributeValue(indexType, "discovered");
                xelist.AddRange(xelistD);
            }
            if (!string.IsNullOrEmpty(deadIndexPath))
            {
                XDocument xdocX = XDocument.Load(deadIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistX = xdocX.XPathSelectElements("/instruments/instrument").ToList();
                foreach (var v in xelistX)
                    v.SetAttributeValue(indexType, "dead");
                xelist.AddRange(xelistX);
            }
            var dictionary = new Dictionary<string, List<XElement>>();
            foreach (var v in xelist)
            {
                List<XElement> list;
                string key = v.AttributeValue(EuronextInstrumentXml.Symbol);
                if (dictionary.TryGetValue(key, out list))
                    list.Add(v);
                else
                {
                    list = new List<XElement> { v };
                    dictionary.Add(key, list);
                }
            }
            foreach (var kvp in dictionary)
            {
                if (kvp.Value.Count > 1)
                {
                    Trace.WriteLine("");
                    Trace.TraceError("Duplicate symbol [{0}], found {1} times:", kvp.Key, kvp.Value.Count);
                    foreach (var v in kvp.Value)
                    {
                        Trace.WriteLine(v.ToString(SaveOptions.None));
                    }
                    Trace.WriteLine("");
                }
            }
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
        }
        #endregion

        #region InstrumentCluster
        internal static void InstrumentCluster(string approvedIndexPath, string discoveredIndexPath, string deadIndexPath)
        {
            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Scanning instruments for clusters");
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            var xelist = new List<XElement>();
            if (!string.IsNullOrEmpty(approvedIndexPath))
            {
                XDocument xdocA = XDocument.Load(approvedIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistA = xdocA.XPathSelectElements("/instruments/instrument").ToList();
                //foreach (var v in xelistA)
                //    v.SetAttributeValue(indexType, "approved");
                xelist.AddRange(xelistA);
            }
            if (!string.IsNullOrEmpty(discoveredIndexPath))
            {
                XDocument xdocD = XDocument.Load(discoveredIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistD = xdocD.XPathSelectElements("/instruments/instrument").ToList();
                //foreach (var v in xelistD)
                //    v.SetAttributeValue(indexType, "discovered");
                xelist.AddRange(xelistD);
            }
            if (!string.IsNullOrEmpty(deadIndexPath))
            {
                XDocument xdocX = XDocument.Load(deadIndexPath
                    /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
                List<XElement> xelistX = xdocX.XPathSelectElements("/instruments/instrument").ToList();
                //foreach (var v in xelistX)
                //    v.SetAttributeValue(indexType, "dead");
                xelist.AddRange(xelistX);
            }
            var listOfLists = new List<List<XElement>>();
            for (int i = 0; i < xelist.Count; ++i)
            {
                XElement v = xelist[i];
                string isin = v.AttributeValue(EuronextInstrumentXml.Isin);
                string symbol = v.AttributeValue(EuronextInstrumentXml.Symbol);
                string name = v.AttributeValue(EuronextInstrumentXml.Name);
                //string file = v.AttributeValue(EuronextInstrumentXml.File);
                var list = new List<XElement> {v};
                for (int j = i + 1; j < xelist.Count; ++j)
                {
                    XElement w = xelist[j];
                    if (isin == w.AttributeValue(EuronextInstrumentXml.Isin) || symbol == w.AttributeValue(EuronextInstrumentXml.Symbol) || name == w.AttributeValue(EuronextInstrumentXml.Name))
                    {
                        list.Add(w);
                        xelist.Remove(w);
                    }
                }
                if (list.Count > 1)
                    listOfLists.Add(list);
            }
            listOfLists.Sort((a, b) => b.Count - a.Count);
            foreach (var list in listOfLists)
            {
                    Trace.WriteLine("");
                    Trace.TraceError("Cluster with {0} instruments", list.Count);
                    Trace.WriteLine("");
                    foreach (var v in list)
                    {
                        Trace.WriteLine(v.ToString(SaveOptions.None));
                    }
                    Trace.WriteLine("");
                    Trace.WriteLine("");
            }
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
        }
        #endregion

        #region PerformSearchScanning
        /// <summary>
        /// Searches every instrument on Euronext website and inserts an attribute <c>foundInSearch</c>
        /// with possible values <c>true</c>, <c>false</c>, <c>changes</c>.
        /// </summary>
        /// <param name="allowAll">Allow 'All' search.</param>
        /// <param name="allowUpdateName">Allow to update the name attribute if its value differs.</param>
        /// <param name="approvedIndexPath">Approved index path. May be null.</param>
        /// <param name="discoveredIndexPath">Discovered index path. May be null.</param>
        /// <param name="deadIndexPath">Dead index path. May be null.</param>
        internal static void PerformSearchScanning(bool allowAll, bool allowUpdateName, string approvedIndexPath, string discoveredIndexPath, string deadIndexPath)
        {
            if (!string.IsNullOrEmpty(approvedIndexPath))
                Scan(approvedIndexPath, "approved", allowAll, allowUpdateName);
            if (!string.IsNullOrEmpty(discoveredIndexPath))
                Scan(discoveredIndexPath, "discovered", allowAll, allowUpdateName);
            if (!string.IsNullOrEmpty(deadIndexPath))
                Scan(deadIndexPath, "dead", allowAll, allowUpdateName);
        }
        private static void Scan(string indexPath, string indexType, bool allowAll = true, bool allowUpdateName = true, string userAgent = null)
        {
            var searchResultList = new List<string>();
            DateTime dateTime = DateTime.Now;
            EuronextInstrumentXml.BackupXmlFile(indexPath, dateTime);

            XDocument xdoc = XDocument.Load(indexPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();

            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Search scan on {0} index", indexType);
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            int i = 10;
            foreach (var xel in xelist)
            {
                xel.NormalizeElement(false, userAgent);
                
                string attr = xel.AttributeValue(EuronextInstrumentXml.FoundInSearch);
                if (attr != "true")
                {
                    xel.SetAttributeValue(EuronextInstrumentXml.FoundInSearch, IsFoundInSearch(xel, indexType, searchResultList, allowAll, allowUpdateName));
                    if (++i % 10 == 0)
                        xdoc.Save(indexPath, SaveOptions.None);
                }
            }
            xdoc.Save(indexPath, SaveOptions.None);
            Trace.TraceInformation("========================================================================================================");
            Trace.TraceInformation("Search results on {0} index", indexType);
            Trace.TraceInformation("------------------------------------------------------------------------------------------------------");
            foreach (var v in searchResultList)
                Trace.WriteLine(v);
            Trace.TraceInformation("========================================================================================================");
        }
        private static string IsFoundInSearch(XElement xel, string indexType, List<string> searchResultList, bool allowAll, bool allowUpdateName)
        {
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string symbol = xel.AttributeValue(EuronextInstrumentXml.Symbol);
            string name = xel.AttributeValue(EuronextInstrumentXml.Name);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            string type = xel.AttributeValue(EuronextInstrumentXml.Type);
            bool isSomethingMissing = "" == isin && "" == symbol && "" == name;
            if (isSomethingMissing)
            {
                Trace.TraceError("Missing what to search in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                searchResultList.Add(";;;;;");
                return "false";
            }
            string what = null;
            if ("" != isin)
                what = isin;
            else if ("" != symbol)
            {
                // ReSharper disable once RedundantToStringCall
                Trace.TraceWarning("Searching symbol because isin is not present in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                what = symbol;
            }
            else if ("" != name)
            {
                // ReSharper disable once RedundantToStringCall
                Trace.TraceWarning("Searching name because symbol and isin are not present in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                what = name;
            }

            string searchSymbol, searchIsin, searchName, searchMicName, searchMic, searchType;
            if (EuronextInstrumentEnrichment.SearchFirstInstrument(what, type, out searchIsin, out searchMic, out searchMicName, out searchSymbol, out searchName, out searchType))
            {
                bool hasChanges = false;
                string st = searchType.ToLowerInvariant();
                if (type != st)
                {
                    bool isInavIndex = type == EuronextInstrumentXml.Inav && st == EuronextInstrumentXml.Index;
                    if (!isInavIndex)
                    {
                        Trace.TraceError("Mismatched type: original [{0}], searched [{1}]", type, st);
                        hasChanges = true;
                    }
                }
                if (isin != searchIsin)
                {
                    Trace.TraceError("Mismatched isin: original [{0}], searched [{1}]", isin, searchIsin);
                    hasChanges = true;
                }
                if (mic != searchMic)
                {
                    Trace.TraceError("Mismatched mic: original [{0}], searched [{1}] ({2})", mic, searchMic, searchMicName);
                    hasChanges = true;
                }
                if (symbol != searchSymbol)
                {
                    Trace.TraceError("Mismatched symbol: original [{0}], searched [{1}]", symbol, searchSymbol);
                    hasChanges = true;
                }
                if (name != searchName)
                {
                    Trace.TraceError("Mismatched name: original [{0}], searched [{1}]", name, searchName);
                    if (allowUpdateName &&
                        (type == EuronextInstrumentXml.Index || type == EuronextInstrumentXml.Stock || type == EuronextInstrumentXml.Fund || type == EuronextInstrumentXml.Etv))
                    {
                        xel.AttributeValue(EuronextInstrumentXml.Name, searchName, false);
                    }
                    else
                        hasChanges = true;
                }
                if (hasChanges)
                    Trace.TraceError("in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                string result = string.Format("{0};{1};{2};{3};{4};{5}", searchSymbol, searchName, searchIsin, searchMicName, searchMic, searchType);
                searchResultList.Add(result);
                return hasChanges ? "changes" : "true";
            }
            //Trace.TraceError("Searching all types with what=[{0}] because direct search with type=[{1}] found nothing", what, type);
            if (allowAll && EuronextInstrumentEnrichment.SearchFirstInstrument(what, "", out searchIsin, out searchMic, out searchMicName, out searchSymbol, out searchName, out searchType))
            {
                bool hasChanges = false;
                string st = searchType.ToLowerInvariant();
                if (type != st)
                {
                    bool isInavIndex = type == EuronextInstrumentXml.Inav && st == EuronextInstrumentXml.Index;
                    if (!isInavIndex)
                    {
                        Trace.TraceError("Mismatched type: original [{0}], searched [{1}]", type, st);
                        hasChanges = true;
                    }
                }
                if (isin != searchIsin)
                {
                    Trace.TraceError("Mismatched isin: original [{0}], searched [{1}]", isin, searchIsin);
                    hasChanges = true;
                }
                if (mic != searchMic)
                {
                    Trace.TraceError("Mismatched mic: original [{0}], searched [{1}] ({2})", mic, searchMic, searchMicName);
                    hasChanges = true;
                }
                if (symbol != searchSymbol)
                {
                    Trace.TraceError("Mismatched symbol: original [{0}], searched [{1}]", symbol, searchSymbol);
                    hasChanges = true;
                }
                if (name != searchName)
                {
                    Trace.TraceError("Mismatched name: original [{0}], searched [{1}]", name, searchName);
                    if (allowUpdateName &&
                        (type == EuronextInstrumentXml.Index || type == EuronextInstrumentXml.Stock || type == EuronextInstrumentXml.Fund || type == EuronextInstrumentXml.Etv))
                    {
                        xel.AttributeValue(EuronextInstrumentXml.Name, searchName, false);
                    }
                    else
                        hasChanges = true;
                }
                if (hasChanges)
                    Trace.TraceError("in {0} element:{1}{2}", indexType, Environment.NewLine, xel.ToString());
                string result = string.Format("{0};{1};{2};{3};{4};{5}", searchSymbol, searchName, searchIsin, searchMicName, searchMic, searchType);
                searchResultList.Add(result);
                return hasChanges ? "changes" : "true";
            }
            Trace.TraceError("Searching what=[{0}] found nothing in {1} element:{2}{3}", what, indexType, Environment.NewLine, xel.ToString());
            searchResultList.Add(";;;;;");
            return "false";
        }
        #endregion

    }
}
