using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace EuronextInstrumentIndexConverter
{
    public class Parser
    {
        #region Members and accessors
        static private XmlReaderSettings xmlReaderSettings;
        private string file;
        private IXmlLineInfo xmlLineInfo;
        private List<string> keyList = new List<string>(1024);
        private string lastComment = null;

        #region ProblemList
        private List<string> problemList = new List<string>(1024);
        public List<string> ProblemList { get { return problemList; } }
        #endregion

        #region OriginalInstrumentList
        private List<OriginalInstrument> originalInstrumentList = new List<OriginalInstrument>(1024);
        public List<OriginalInstrument> OriginalInstrumentList { get { return originalInstrumentList; } }
        #endregion

        #region ConvertedInstrumentList
        private List<ConvertedInstrument> convertedInstrumentList = new List<ConvertedInstrument>(1024);
        public List<ConvertedInstrument> ConvertedInstrumentList { get { return convertedInstrumentList; } }
        #endregion

        #region Count
        public int Count { get { return originalInstrumentList.Count; } }
        #endregion

        #region ProblemCount
        public int ProblemCount { get { return problemList.Count; } }
        #endregion
        #endregion

        #region Construction
        static Parser()
        {
            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = false;
            //xmlReaderSettings.ValidationType = ValidationType.Schema;
            //xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            //xmlReaderSettings.Schemas.Add(null, "instrumentIndex.xsd");
        }

        public Parser()
        {
        }
        #endregion

        #region Error strings
        public void ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            problemList.Add(string.Format("file {0} line {1} pos {2}: XML Validation failed: {3}", file, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition, args.Message));
        }
        private string NoAttribute(string attribute)
        {
            return string.Format("file {0} line {1}: {2} attribute not found", file, xmlLineInfo.LineNumber, attribute);
        }
        private string BadAttribute(string attribute)
        {
            return string.Format("file {0} line {1}: failed to parse attribute {2}", file, xmlLineInfo.LineNumber, attribute);
        }
        #endregion

        #region Add tag
        private string AddTag(XmlReader xmlReader, string tag, ref OriginalInstrument instrument, bool mustExist)
        {
             string value = xmlReader.GetAttribute(tag);
             if ("?" == value)
                 value = null;
             if (null != value)
             {
                 instrument.Dictionary.Add(tag, value);
                 instrument.List.Add(string.Concat(tag, " = ", value));
                 return value;
             }
             else if (mustExist)
                 problemList.Add(NoAttribute(tag));
             return "";
        }

        private string AddTag(XmlReader xmlReader, string tag, ref OriginalInstrument instrument, bool mustExist, string prefix)
        {
             string value = xmlReader.GetAttribute(tag);
            tag = string.Concat(prefix, ".", tag);
             if ("?" == value)
                 value = null;
             if (null != value)
             {
                 instrument.Dictionary.Add(tag, value);
                 instrument.List.Add(string.Concat(tag, " = ", value));
                 return value;
             }
             else if (mustExist)
                 problemList.Add(NoAttribute(tag));
             return "";
        }
        #endregion

        #region Save
        public void Save(string fileName, int index)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                file.WriteLine("<instruments>");
                convertedInstrumentList[index].Save(file);
                file.WriteLine();
                file.WriteLine("</instruments>");
            }
        }

        public void Save(string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                file.WriteLine("<instruments>");
                convertedInstrumentList.ForEach(t => t.Save(file));
                file.WriteLine();
                file.WriteLine("</instruments>");
            }
        }
        #endregion

        #region Convert
        private void Convert()
        {
            originalInstrumentList.ForEach(i => { convertedInstrumentList.Add(new ConvertedInstrument(i)); });
        }
        #endregion

        #region Parse
        public void Parse(string filePath, bool validate)
        {
            lastComment = null;
            file = filePath;
            originalInstrumentList.Clear();
            convertedInstrumentList.Clear();
            problemList.Clear();
            keyList.Clear();
            if (validate)
                xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
            OriginalInstrument instrument = null;
            string str, key, mode = null;
            try
            {
                // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
                using (XmlReader xmlReader = validate ? XmlReader.Create(filePath, xmlReaderSettings) : XmlReader.Create(filePath))
                {
                    if (null != xmlReader)
                    {
                        xmlLineInfo = (IXmlLineInfo)xmlReader;
                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    str = xmlReader.LocalName;
                                    if ("instrument" == str)
                                    {
                                        if (null != instrument)
                                        {
                                            originalInstrumentList.Add(instrument);
                                            instrument = null;
                                            mode = null;
                                        }
                                        key = xmlReader.GetAttribute("type");
                                        if (null == key)
                                        {
                                            problemList.Add(string.Format("line {0}: instrument has no type attribute, skipping", xmlLineInfo.LineNumber));
                                            break;
                                        }
                                        key = xmlReader.GetAttribute("vendor");
                                        if (null == key)
                                            key = "Euronext";
                                        if ("Euronext" != key)
                                        {
                                            problemList.Add(string.Format("line {0}: instrument has non-Euronext vendor [{1}], skipping", xmlLineInfo.LineNumber, key));
                                            break;
                                        }
                                        mode = "instrument";
                                        instrument = new OriginalInstrument();
                                        instrument.List.Add(string.Concat("**line*** = ", xmlLineInfo.LineNumber.ToString()));
                                        instrument.Dictionary.Add("vendor", "Euronext");
                                        instrument.List.Add("vendor = Euronext");
                                        key = string.Format("{0}_{1}_{2}",
                                            AddTag(xmlReader, "mep", ref instrument, true),
                                            AddTag(xmlReader, "isin", ref instrument, true),
                                            AddTag(xmlReader, "symbol", ref instrument, true));
                                        if (keyList.Exists(s =>
                                        {
                                            return s == key;
                                        }))
                                            problemList.Add(string.Concat("instrument ", key, " already exists"));
                                        AddTag(xmlReader, "name", ref instrument, true);
                                        AddTag(xmlReader, "type", ref instrument, false); // Already checked
                                        AddTag(xmlReader, "currency", ref instrument, false);
                                        AddTag(xmlReader, "description", ref instrument, false);
                                        AddTag(xmlReader, "file", ref instrument, true);
                                        AddTag(xmlReader, "state", ref instrument, false);
                                        AddTag(xmlReader, "notes", ref instrument, false);
                                        AddTag(xmlReader, "underlyingSymbol", ref instrument, false);
                                        AddTag(xmlReader, "underlyingName", ref instrument, false);
                                        AddTag(xmlReader, "inavSymbol", ref instrument, false);
                                        AddTag(xmlReader, "inavIsin", ref instrument, false);
                                        AddTag(xmlReader, "inavMep", ref instrument, false);
                                        AddTag(xmlReader, "inavName", ref instrument, false);
                                        AddTag(xmlReader, "etfSymbol", ref instrument, false);
                                        AddTag(xmlReader, "etfIsin", ref instrument, false);
                                        AddTag(xmlReader, "etfMep", ref instrument, false);
                                        AddTag(xmlReader, "etfName", ref instrument, false);
                                        AddTag(xmlReader, "cfi", ref instrument, false);
                                        AddTag(xmlReader, "compartment", ref instrument, false);
                                        AddTag(xmlReader, "icb1", ref instrument, false);
                                        AddTag(xmlReader, "icb2", ref instrument, false);
                                        AddTag(xmlReader, "icb3", ref instrument, false);
                                        AddTag(xmlReader, "icb4", ref instrument, false);
                                        AddTag(xmlReader, "tradingmode", ref instrument, false);
                                        AddTag(xmlReader, "mMC", ref instrument, false);
                                        AddTag(xmlReader, "mEURLS", ref instrument, false);
                                        AddTag(xmlReader, "mEURLSfor", ref instrument, false);
                                        AddTag(xmlReader, "mEURLSspe", ref instrument, false);
                                        AddTag(xmlReader, "mEURLSloc", ref instrument, false);
                                        AddTag(xmlReader, "iN150", ref instrument, false);
                                        AddTag(xmlReader, "iCM100", ref instrument, false);
                                        AddTag(xmlReader, "iMS190", ref instrument, false);
                                        AddTag(xmlReader, "iPX4", ref instrument, false);
                                        AddTag(xmlReader, "iPX5", ref instrument, false);
                                        AddTag(xmlReader, "iPX8", ref instrument, false);
                                        AddTag(xmlReader, "iPAX", ref instrument, false);
                                        AddTag(xmlReader, "iCS90", ref instrument, false);
                                        AddTag(xmlReader, "mRADMR", ref instrument, false);
                                        AddTag(xmlReader, "mLISUS", ref instrument, false);
                                        AddTag(xmlReader, "mBRUCI", ref instrument, false);
                                        AddTag(xmlReader, "mBRUTF", ref instrument, false);
                                        AddTag(xmlReader, "mAMSTL", ref instrument, false);
                                        AddTag(xmlReader, "mALTXpub", ref instrument, false);
                                        AddTag(xmlReader, "mALTXpri", ref instrument, false);
                                        AddTag(xmlReader, "mALTX", ref instrument, false);
                                        AddTag(xmlReader, "iBVLGR", ref instrument, false);
                                        AddTag(xmlReader, "iPSITR", ref instrument, false);
                                        AddTag(xmlReader, "iPSI20", ref instrument, false);
                                        AddTag(xmlReader, "iSIIC", ref instrument, false);
                                        AddTag(xmlReader, "iIAS", ref instrument, false);
                                        AddTag(xmlReader, "iPXT", ref instrument, false);
                                        AddTag(xmlReader, "iCIT20", ref instrument, false);
                                        AddTag(xmlReader, "iCN20", ref instrument, false);
                                        AddTag(xmlReader, "iPX1", ref instrument, false);
                                        AddTag(xmlReader, "iBELSC", ref instrument, false);
                                        AddTag(xmlReader, "iBELMC", ref instrument, false);
                                        AddTag(xmlReader, "iBELCU", ref instrument, false);
                                        AddTag(xmlReader, "iBELAR", ref instrument, false);
                                        AddTag(xmlReader, "iBELAS", ref instrument, false);
                                        AddTag(xmlReader, "iBELS", ref instrument, false);
                                        AddTag(xmlReader, "iBELM", ref instrument, false);
                                        AddTag(xmlReader, "iBEL2P", ref instrument, false);
                                        AddTag(xmlReader, "iBEL2I", ref instrument, false);
                                        AddTag(xmlReader, "iBEL20", ref instrument, false);
                                        AddTag(xmlReader, "iAAX", ref instrument, false);
                                        AddTag(xmlReader, "iASCX", ref instrument, false);
                                        AddTag(xmlReader, "iAMX", ref instrument, false);
                                        AddTag(xmlReader, "iAEX", ref instrument, false);
                                        AddTag(xmlReader, "iALASI", ref instrument, false);
                                        AddTag(xmlReader, "iN100", ref instrument, false);
                                        AddTag(xmlReader, "iNC70", ref instrument, false);
                                        if (!string.IsNullOrEmpty(lastComment))
                                        {
                                            //instrument.Dictionary.Add("lastComment", lastComment);
                                            instrument.List.Add(string.Concat("lastComment = ", lastComment));
                                        }
                                    }
                                    else if ("index" == str)
                                    {
                                        if ("instrument" != mode)
                                            break;
                                        mode = "index";
                                        AddTag(xmlReader, "kind", ref instrument, false, mode);
                                        AddTag(xmlReader, "calcFreq", ref instrument, false, mode);
                                        AddTag(xmlReader, "baseDate", ref instrument, false, mode);
                                        AddTag(xmlReader, "baseLevel", ref instrument, false, mode);
                                        AddTag(xmlReader, "weighting", ref instrument, false, mode);
                                        AddTag(xmlReader, "capFactor", ref instrument, false, mode);
                                        AddTag(xmlReader, "family", ref instrument, false, mode);
                                        AddTag(xmlReader, "baseCap", ref instrument, false, mode);
                                        AddTag(xmlReader, "baseCapCurrency", ref instrument, false, mode);
                                    }
                                    else if ("icb" == str)
                                    {
                                        AddTag(xmlReader, "icb1", ref instrument, false, mode);
                                        AddTag(xmlReader, "icb2", ref instrument, false, mode);
                                        AddTag(xmlReader, "icb3", ref instrument, false, mode);
                                        AddTag(xmlReader, "icb4", ref instrument, false, mode);
                                    }
                                    else if ("stock" == str)
                                    {
                                        if ("instrument" != mode)
                                            break;
                                        mode = "stock";
                                        AddTag(xmlReader, "cfi", ref instrument, false, mode);
                                        AddTag(xmlReader, "tradingMode", ref instrument, false, mode);
                                        AddTag(xmlReader, "compartment", ref instrument, false, mode);
                                        AddTag(xmlReader, "currency", ref instrument, false, mode);
                                    }
                                    else if ("target" == str)
                                    {
                                        string modus;
                                        if (instrument.Dictionary.TryGetValue("inav.target0.vendor", out modus) ||
                                            instrument.Dictionary.TryGetValue("inav.target0.mep", out modus) ||
                                            instrument.Dictionary.TryGetValue("inav.target0.isin", out modus) ||
                                            instrument.Dictionary.TryGetValue("inav.target0.symbol", out modus) ||
                                            instrument.Dictionary.TryGetValue("inav.target0.name", out modus))
                                            modus = string.Concat(mode, ".target1");
                                        else
                                            modus = string.Concat(mode, ".target0");
                                        AddTag(xmlReader, "vendor", ref instrument, false, modus);
                                        AddTag(xmlReader, "mep", ref instrument, false, modus);
                                        AddTag(xmlReader, "isin", ref instrument, false, modus);
                                        AddTag(xmlReader, "symbol", ref instrument, false, modus);
                                        AddTag(xmlReader, "name", ref instrument, false, modus);
                                    }
                                    else if ("inav" == str)
                                    {
                                        if ("instrument" == mode)
                                        {
                                            mode = "inav";
                                            AddTag(xmlReader, "currency", ref instrument, false, mode);
                                        }
                                        else
                                        {
                                            string modus = string.Concat(mode, ".inav");
                                            AddTag(xmlReader, "vendor", ref instrument, false, modus);
                                            AddTag(xmlReader, "mep", ref instrument, false, modus);
                                            AddTag(xmlReader, "isin", ref instrument, false, modus);
                                            AddTag(xmlReader, "symbol", ref instrument, false, modus);
                                            AddTag(xmlReader, "name", ref instrument, false, modus);
                                        }
                                    }
                                    else if ("underlying" == str)
                                    {
                                        string modus = string.Concat(mode, ".underlying");
                                        AddTag(xmlReader, "vendor", ref instrument, false, modus);
                                        AddTag(xmlReader, "mep", ref instrument, false, modus);
                                        AddTag(xmlReader, "isin", ref instrument, false, modus);
                                        AddTag(xmlReader, "symbol", ref instrument, false, modus);
                                        AddTag(xmlReader, "name", ref instrument, false, modus);
                                    }
                                    else if ("etf" == str)
                                    {
                                        if ("instrument" != mode)
                                            break;
                                        mode = "etf";
                                        AddTag(xmlReader, "cfi", ref instrument, false, mode);
                                        AddTag(xmlReader, "mar", ref instrument, false, mode);
                                        AddTag(xmlReader, "launchDate", ref instrument, false, mode);
                                        AddTag(xmlReader, "currency", ref instrument, false, mode);
                                        AddTag(xmlReader, "issuer", ref instrument, false, mode);
                                        AddTag(xmlReader, "fraction", ref instrument, false, mode);
                                        AddTag(xmlReader, "dividendFrequency", ref instrument, false, mode);
                                        AddTag(xmlReader, "indexFamily", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg1", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg2", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg3", ref instrument, false, mode);
                                    }
                                    else if ("etv" == str)
                                    {
                                        if ("instrument" != mode)
                                            break;
                                        mode = "etv";
                                        AddTag(xmlReader, "cfi", ref instrument, false, mode);
                                        AddTag(xmlReader, "mar", ref instrument, false, mode);
                                        AddTag(xmlReader, "launchDate", ref instrument, false, mode);
                                        AddTag(xmlReader, "currency", ref instrument, false, mode);
                                        AddTag(xmlReader, "issuer", ref instrument, false, mode);
                                        AddTag(xmlReader, "fraction", ref instrument, false, mode);
                                        AddTag(xmlReader, "dividendFrequency", ref instrument, false, mode);
                                        AddTag(xmlReader, "indexFamily", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg1", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg2", ref instrument, false, mode);
                                        AddTag(xmlReader, "seg3", ref instrument, false, mode);
                                    }
                                    else if ("fund" == str)
                                    {
                                        if ("instrument" != mode)
                                            break;
                                        mode = "fund";
                                        AddTag(xmlReader, "cfi", ref instrument, false, mode);
                                        AddTag(xmlReader, "tradingMode", ref instrument, false, mode);
                                        AddTag(xmlReader, "currency", ref instrument, false, mode);
                                    }
                                    break;
                                case XmlNodeType.Attribute:
                                    problemList.Add("******attribute " + xmlReader.Name + "=" + xmlReader.Value);
                                    break;
                                case XmlNodeType.Document:
                                    problemList.Add("******root " + xmlReader.LocalName);
                                    break;
                                case XmlNodeType.Text:
                                    problemList.Add("******text " + xmlReader.Value);
                                    break;
                                case XmlNodeType.ProcessingInstruction:
                                    problemList.Add("******instruction " + xmlReader.Value);
                                    break;
                                case XmlNodeType.Comment:
                                    lastComment = xmlReader.Value;
                                    break;
                            }
                        }
                        // Explicitly call Close on the XmlReader to reduce strain on the GC.
                        xmlReader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                problemList.Add(string.Format("Exception: {0}", e.Message));
            }
            finally
            {
                if (validate)
                    xmlReaderSettings.ValidationEventHandler -= ValidationEventHandler;
            }
            if (null != instrument)
                originalInstrumentList.Add(instrument);
            Convert();
        }
        #endregion

    }
}
