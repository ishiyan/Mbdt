using System;
using System.Collections.Generic;
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
            problemList.Add(
                $"file {file} line {xmlLineInfo.LineNumber} pos {xmlLineInfo.LinePosition}: XML Validation failed: {args.Message}");
        }
        private string NoAttribute(string attribute)
        {
            return $"file {file} line {xmlLineInfo.LineNumber}: {attribute} attribute not found";
        }
        private string BadAttribute(string attribute)
        {
            return $"file {file} line {xmlLineInfo.LineNumber}: failed to parse attribute {attribute}";
        }
        public List<string> Audit(string filePath, bool validate)
        {
            file = filePath;
            if (validate)
                xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
            problemList = new List<string>();
            Instrument instrument;
            Trade tradeCurrent = null;
            List<Instrument> instrumentList = new List<Instrument>();
            bool endofday = false, intraday = false;
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
                                    var str = xmlReader.LocalName;
                                    Trade tradePrevious = null;
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
                                                problemList.Add(
                                                    $"file {file} line {xmlLineInfo.LineNumber} t: s {tradeCurrent.secValue} is less than previous s {tradePrevious.secValue}");
                                            else if (tradePrevious.secValue == tradeCurrent.secValue)
                                            {
                                                if (tradePrevious.priceValue == tradeCurrent.priceValue)
                                                    problemList.Add(
                                                        $"file {file} line {xmlLineInfo.LineNumber} t: not properly collapsed, s and price are the same as previous");
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
                                        instrument = new Instrument
                                        {
                                            mep = xmlReader.GetAttribute("mep"),
                                            isin = xmlReader.GetAttribute("isin"),
                                            symbol = xmlReader.GetAttribute("symbol"),
                                            name = xmlReader.GetAttribute("name"),
                                            currency = xmlReader.GetAttribute("currency"),
                                            type = xmlReader.GetAttribute("type")
                                        };
                                        bool mep = false;
                                        if (null == instrument.mep)
                                            problemList.Add(NoAttribute("mep"));
                                        else
                                            mep = instrumentList.Exists(s => s.mep?.Equals(instrument.mep) ?? false);
                                        bool isin = false;
                                        if (null == instrument.isin)
                                            problemList.Add(NoAttribute("isin"));
                                        else
                                            isin = instrumentList.Exists(s => s.isin?.Equals(instrument.isin) ?? false);
                                        bool symbol = false;
                                        if (null == instrument.symbol)
                                            problemList.Add(NoAttribute("symbol"));
                                        else
                                            symbol = instrumentList.Exists(s => s.symbol?.Equals(instrument.symbol) ?? false);
                                        bool name = false;
                                        if (null == instrument.name)
                                            problemList.Add(NoAttribute("name"));
                                        else
                                            name = instrumentList.Exists(s => s.name?.Equals(instrument.name) ?? false);
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
                                            stringBuilder.Append(
                                                $"file {file} line {xmlLineInfo.LineNumber}: duplicate instrument with identical:");
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
                                            problemList.Add(
                                                $"file {file} line {xmlLineInfo.LineNumber}: both endofday and intraday defined");
                                    }
                                    else if ("intraday" == str)
                                    {
                                        intraday = true;
                                        if (endofday)
                                            problemList.Add(
                                                $"file {file} line {xmlLineInfo.LineNumber}: both intraday and endofday defined");
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
                problemList.Add($"Exception: {e.Message}");
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
