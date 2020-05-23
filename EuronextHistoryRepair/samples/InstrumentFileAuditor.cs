using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace mbdt.InstrumentFileAuditor
{
    class InstrumentFileAuditor
    {
        class Instrument
        {
            public string currency = null, isin = null, mep = null, name = null, symbol = null, type = null;
        }
        class Trade
        {
            public string price = null, sec = null, time = null, volume = null;
            public decimal priceValue = -1;
            public long volumeValue = -1;
            public int secValue = -1;
        }
        static private XmlReaderSettings xmlReaderSettings;
        static InstrumentFileAuditor()
        {
            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = false;
            xmlReaderSettings.IgnoreProcessingInstructions = false;
            xmlReaderSettings.IgnoreWhitespace = false;
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.Schemas.Add(null, "instrumentFile.xsd");
        }

        StringBuilder stringBuilder = new StringBuilder();
        private List<string> problemList;
        private string file;
        private IXmlLineInfo xmlLineInfo;
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
        public List<string> Audit(string filePath, bool validate)
        {
            file = filePath;
            if (validate)
                xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
            problemList = new List<string>();
            Instrument instrument;
            Trade tradePrevious = null, tradeCurrent = null;
            List<Instrument> instrumentList = new List<Instrument>();
            bool endofday = false, intraday = false;
            string str;
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
                                    if ("t" == str)
                                    {
                                        tradePrevious = tradeCurrent;
                                        tradeCurrent = new Trade();
                                        tradeCurrent.sec = xmlReader.GetAttribute("s");
                                        tradeCurrent.time = xmlReader.GetAttribute("t");
                                        tradeCurrent.price = xmlReader.GetAttribute("p");
                                        tradeCurrent.volume = xmlReader.GetAttribute("v");
                                        if (null == tradeCurrent.sec)
                                            problemList.Add(NoAttribute("s"));
                                        else
                                        {
                                            int value = -1;
                                            if (int.TryParse(tradeCurrent.sec, out value))
                                                tradeCurrent.secValue = value;
                                            else
                                                problemList.Add(BadAttribute("s"));
                                        }
                                        if (null == tradeCurrent.price)
                                            problemList.Add(NoAttribute("p"));
                                        else
                                        {
                                            decimal value = -1;
                                            if (decimal.TryParse(tradeCurrent.price, out value))
                                                tradeCurrent.priceValue = value;
                                            else
                                                problemList.Add(BadAttribute("p"));
                                        }
                                        if (null == tradeCurrent.volume)
                                            problemList.Add(NoAttribute("v"));
                                        else
                                        {
                                            long value = -1;
                                            if (long.TryParse(tradeCurrent.volume, out value))
                                                tradeCurrent.volumeValue = value;
                                            else
                                                problemList.Add(BadAttribute("v"));
                                        }
                                        if (null != tradePrevious)
                                        {
                                            if (tradePrevious.secValue > tradeCurrent.secValue)
                                                problemList.Add(string.Format("file {0} line {1} t: s {2} is less than previous s {3}", file, xmlLineInfo.LineNumber, tradeCurrent.secValue, tradePrevious.secValue));
                                            else if (tradePrevious.secValue == tradeCurrent.secValue)
                                            {
                                                if (tradePrevious.priceValue == tradeCurrent.priceValue)
                                                    problemList.Add(string.Format("file {0} line {1} t: not properly collapsed, s and price are the same as previous", file, xmlLineInfo.LineNumber));
                                            }
                                        }
                                    }
                                    else if ("q" == str)
                                    {
                                        tradePrevious = null;
                                        tradeCurrent = null;
                                        //if (endofday)
                                    }
                                    else if ("instrument" == str)
                                    {
                                        instrument = new Instrument();
                                        instrument.mep = xmlReader.GetAttribute("mep");
                                        instrument.isin = xmlReader.GetAttribute("isin");
                                        instrument.symbol = xmlReader.GetAttribute("symbol");
                                        instrument.name = xmlReader.GetAttribute("name");
                                        instrument.currency = xmlReader.GetAttribute("currency");
                                        instrument.type = xmlReader.GetAttribute("type");
                                        bool mep = false;
                                        if (null == instrument.mep)
                                            problemList.Add(NoAttribute("mep"));
                                        else
                                            mep = instrumentList.Exists(s =>
                                            {
                                                return null == s.mep ? false : s.mep.Equals(instrument.mep);
                                            });
                                        bool isin = false;
                                        if (null == instrument.isin)
                                            problemList.Add(NoAttribute("isin"));
                                        else
                                            isin = instrumentList.Exists(s =>
                                            {
                                                return null == s.isin ? false : s.isin.Equals(instrument.isin);
                                            });
                                        bool symbol = false;
                                        if (null == instrument.symbol)
                                            problemList.Add(NoAttribute("symbol"));
                                        else
                                            symbol = instrumentList.Exists(s =>
                                            {
                                                return null == s.symbol ? false : s.symbol.Equals(instrument.symbol);
                                            });
                                        bool name = false;
                                        if (null == instrument.name)
                                            problemList.Add(NoAttribute("name"));
                                        else
                                            name = instrumentList.Exists(s =>
                                            {
                                                return null == s.name ? false : s.name.Equals(instrument.name);
                                            });
                                        //bool currency = false;
                                        //if (null == instrument.currency)
                                        //    problemList.Add(NoAttribute("currency"));
                                        //else
                                        //    currency = instrumentList.Exists(s =>
                                        //    {
                                        //        return null == s.currency ? currency : s.isin.Equals(instrument.currency);
                                        //    });
                                        //bool type = false;
                                        //if (null == instrument.type)
                                        //    problemList.Add(NoAttribute("type"));
                                        //else
                                        //    type = instrumentList.Exists(s =>
                                        //    {
                                        //        return null == s.type ? false : s.type.Equals(instrument.type);
                                        //    });
                                        instrumentList.Add(instrument);
                                        if (mep || isin || symbol || name)// | currency | type)
                                        {
                                            stringBuilder.Length = 0;
                                            stringBuilder.Append(string.Format("file {0} line {1}: duplicate instrument with identical:", file, xmlLineInfo.LineNumber));
                                            if (mep)
                                                stringBuilder.Append(" mep");
                                            if (isin)
                                                stringBuilder.Append(" isin");
                                            if (symbol)
                                                stringBuilder.Append(" symbol");
                                            if (name)
                                                stringBuilder.Append(" name");
                                            //if (currency)
                                            //    stringBuilder.Append(" currency");
                                            //if (type)
                                            //    stringBuilder.Append(" type");
                                            problemList.Add(stringBuilder.ToString());
                                        }

                                    }
                                    else if ("endofday" == str)
                                    {
                                        endofday = true;
                                        if (intraday)
                                            problemList.Add(string.Format("file {0} line {1}: both endofday and intraday defined", file, xmlLineInfo.LineNumber));
                                    }
                                    else if ("intraday" == str)
                                    {
                                        intraday = true;
                                        if (endofday)
                                            problemList.Add(string.Format("file {0} line {1}: both intraday and endofday defined", file, xmlLineInfo.LineNumber));
                                    }
                                    //problemList.Add("element " + s);
                                    //if (xmlReader.HasAttributes)
                                    //{
                                    //    while (xmlReader.MoveToNextAttribute())
                                    //        problemList.Add(">>attr " + xmlReader.Name + "=" + xmlReader.Value);
                                    //}
                                    break;
                                //case XmlNodeType.EndElement:
                                //    problemList.Add("element end " + xmlReader.LocalName);
                                //    break;
                                case XmlNodeType.Attribute:
                                    problemList.Add("attribute " + xmlReader.Name + "=" + xmlReader.Value);
                                    break;
                                case XmlNodeType.Document:
                                    problemList.Add("root " + xmlReader.LocalName);
                                    break;
                                case XmlNodeType.Text:
                                    problemList.Add("text " + xmlReader.Value);
                                    break;
                                case XmlNodeType.ProcessingInstruction:
                                    problemList.Add("instruction " + xmlReader.Value);
                                    break;
                                case XmlNodeType.Comment:
                                    problemList.Add("comment " + xmlReader.Value);
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
            return problemList;
        }

        public List<string> Validate(string filePath)
        {
            file = filePath;
            xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
            problemList = new List<string>();
            try
            {
                // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
                using (XmlReader xmlReader = XmlReader.Create(filePath, xmlReaderSettings))
                {
                    if (null != xmlReader)
                    {
                        while (xmlReader.Read())
                        {
                            // Empty loop.
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
                xmlReaderSettings.ValidationEventHandler -= ValidationEventHandler;
            }
            return problemList;
        }
    }
}
