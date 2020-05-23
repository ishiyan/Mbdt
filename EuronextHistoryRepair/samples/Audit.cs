using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using mbdt.Utils;

namespace mbdt.InstrumentFileXmlStatistics
{
    internal sealed class InstrumentFileXmlAuditor
    {
        internal sealed class XmlInstrument
        {
            internal string Path, Currency, Isin, Mep, Name, Symbol, Type;
            internal int CountDays;
            internal List<DateTime> MissingDaysList = new List<DateTime>();
            internal DateTime DateFirst = new DateTime(0L), DateLast = new DateTime(0L);
            internal List<string> ProblemList = new List<string>();
            internal XmlInstrument(string path) { Path = path; }
            public override string ToString()
            {
                return string.Format("{0}_{1}_{2} ({3}) [{4}] C{5} {6}:{7} [{8}]", null == Mep ? "" : Mep, null == Symbol ? "" : Symbol, null == Isin ? "" : Isin, null == Name ? "" : Name, Path, CountDays, DateFirst.ToShortDateString(), DateLast.ToShortDateString(), MissingDaysList.Count);
            }
            internal string ToStringMissingDates()
            {
                string s = string.Format("{0}_{1}_{2} ({3}) [{4}] C{5} {6}:{7} [", null == Mep ? "" : Mep, null == Symbol ? "" : Symbol, null == Isin ? "" : Isin, null == Name ? "" : Name, Path, CountDays, DateFirst.ToShortDateString(), DateLast.ToShortDateString());
                MissingDaysList.ForEach(x => s = string.Concat(s, ",", x.ToShortDateString()));
                return string.Concat(s, "]");
            }
        }
        internal sealed class XmlTrade
        {
            public string Price, Sec, Time, Volume;
            public double PriceValue = -1;
            public long VolumeValue = -1;
            public int SecValue = -1;
        }
        internal sealed class XmlOhlcv
        {
            public string Open, High, Low, Close, Date, Volume;
            public double OpenValue = -1, HighValue = -1, LowValue = -1, CloseValue = -1;
            public long VolumeValue = -1;
            public DateTime DateValue = new DateTime(0L);
        }
        internal sealed class XmlScalar
        {
            public string Value, Date;
            public double ValueValue = -1;
            public DateTime DateValue = new DateTime(0L);
        }
        static private XmlReaderSettings xmlReaderSettings;
        static private bool validate = false;
        static InstrumentFileXmlAuditor()
        {
            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.CheckCharacters = true;
            xmlReaderSettings.CloseInput = true;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.IgnoreComments = false;
            xmlReaderSettings.IgnoreProcessingInstructions = false;
            xmlReaderSettings.IgnoreWhitespace = false;
            if (validate)
            {
                xmlReaderSettings.ValidationType = ValidationType.Schema;
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                xmlReaderSettings.Schemas.Add(null, "instrumentFile.xsd");
            }
        }
        internal InstrumentFileXmlAuditor()
        {
            if (validate)
                xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
        }
        private List<string> problemList;
        private StringBuilder stringBuilder = new StringBuilder();
        private string file;
        private IXmlLineInfo xmlLineInfo;
        private void ValidationEventHandler(object sender, ValidationEventArgs args)
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
        static internal List<List<XmlInstrument>> TotalInstrumentFileList = new List<List<XmlInstrument>>();
        static internal List<List<XmlInstrument>> MultiInstrumentFileList = new List<List<XmlInstrument>>();
        static internal Dictionary<DateTime, List<XmlInstrument>> MissingDatesDictionary = new Dictionary<DateTime, List<XmlInstrument>>();
        public List<string> Audit(string filePath)
        {
            file = filePath;
            problemList = new List<string>();
            XmlInstrument instrument = null;
            XmlTrade tradePrevious = null, tradeCurrent = null;
            List<XmlInstrument> instrumentList = new List<XmlInstrument>();
            TotalInstrumentFileList.Add(instrumentList);
            bool endofday = false, intraday = false, firstInstrument = false, firstSampleParsed = false;
            int prevJdn = 0, thisJdn = 0;
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
                                    #region t element
                                    if ("t" == str)
                                    {
                                        tradePrevious = tradeCurrent;
                                        tradeCurrent = new XmlTrade();
                                        tradeCurrent.Sec = xmlReader.GetAttribute("s");
                                        tradeCurrent.Time = xmlReader.GetAttribute("t");
                                        tradeCurrent.Price = xmlReader.GetAttribute("p");
                                        tradeCurrent.Volume = xmlReader.GetAttribute("v");
                                        if (null == tradeCurrent.Sec)
                                            problemList.Add(NoAttribute("s"));
                                        else
                                        {
                                            int value = -1;
                                            if (int.TryParse(tradeCurrent.Sec, out value))
                                                tradeCurrent.SecValue = value;
                                            else
                                                problemList.Add(BadAttribute("s"));
                                        }
                                        if (null == tradeCurrent.Price)
                                            problemList.Add(NoAttribute("p"));
                                        else
                                        {
                                            double value = -1;
                                            if (double.TryParse(tradeCurrent.Price, out value))
                                                tradeCurrent.PriceValue = value;
                                            else
                                                problemList.Add(BadAttribute("p"));
                                        }
                                        if (null == tradeCurrent.Volume)
                                            problemList.Add(NoAttribute("v"));
                                        else
                                        {
                                            long value = -1;
                                            if (long.TryParse(tradeCurrent.Volume, out value))
                                                tradeCurrent.VolumeValue = value;
                                            else
                                                problemList.Add(BadAttribute("v"));
                                        }
                                        if (null != tradePrevious)
                                        {
                                            if (tradePrevious.SecValue > tradeCurrent.SecValue)
                                                problemList.Add(string.Format("file {0} line {1} t: s {2} is less than previous s {3}", file, xmlLineInfo.LineNumber, tradeCurrent.SecValue, tradePrevious.SecValue));
                                            else if (tradePrevious.SecValue == tradeCurrent.SecValue)
                                            {
                                                if (tradePrevious.PriceValue == tradeCurrent.PriceValue)
                                                    problemList.Add(string.Format("file {0} line {1} t: not properly collapsed, s and price are the same as previous", file, xmlLineInfo.LineNumber));
                                            }
                                        }
                                    }
                                    #endregion
                                    #region q element
                                    else if ("q" == str)
                                    {
                                        string ds = xmlReader.GetAttribute("d");
                                        if (null == ds)
                                        {
                                            problemList.Add(NoAttribute("d"));
                                            break;
                                        }
                                        int jdn = JulianDayNumber.FromYYYYMMDD(ds);
                                        if (!firstSampleParsed)
                                        {
                                            firstSampleParsed = true;
                                            instrument.DateFirst = JulianDayNumber.ToDateTime(jdn);
                                        }
                                        else
                                        {
                                            prevJdn = thisJdn;
                                            if (jdn <= thisJdn)
                                                problemList.Add(string.Format("file {0} line {1}: illegal date order: {2} -> {3}", file, xmlLineInfo.LineNumber, JulianDayNumber.ToYYYYMMDD(thisJdn), JulianDayNumber.ToYYYYMMDD(jdn)));
                                            for (int i = thisJdn + 1; i < jdn; i++)
                                            {
                                                if (JulianDayNumber.IsWeekend(i))
                                                    continue;
                                                DateTime dt = JulianDayNumber.ToDateTime(i);
                                                instrument.MissingDaysList.Add(dt);
                                                List<XmlInstrument> l;
                                                if (!MissingDatesDictionary.TryGetValue(dt, out l))
                                                {
                                                    l = new List<XmlInstrument>();
                                                    MissingDatesDictionary.Add(dt, l);
                                                }
                                                l.Add(instrument);
                                            }
                                        }
                                        thisJdn = jdn;
                                        instrument.CountDays++;
                                        //if (endofday)
                                        //{
                                        //}
                                        //else if (intraday)
                                        //{
                                        //    tradePrevious = null;
                                        //    tradeCurrent = null;
                                        //}
                                    }
                                    #endregion
                                    #region instrument element
                                    else if ("instrument" == str)
                                    {
                                        if (firstInstrument)
                                        {
                                            MultiInstrumentFileList.Add(instrumentList);
                                            firstSampleParsed = false;
                                            instrument.DateLast = JulianDayNumber.ToDateTime(thisJdn);
                                            thisJdn = 0;
                                            prevJdn = 0;
                                        }
                                        else
                                            firstInstrument = true;
                                        instrument = new XmlInstrument(filePath);
                                        instrument.Mep = xmlReader.GetAttribute("mep");
                                        instrument.Isin = xmlReader.GetAttribute("isin");
                                        instrument.Symbol = xmlReader.GetAttribute("symbol");
                                        instrument.Name = xmlReader.GetAttribute("name");
                                        instrument.Currency = xmlReader.GetAttribute("currency");
                                        instrument.Type = xmlReader.GetAttribute("type");
                                        instrument.ProblemList = problemList;
                                        bool mep = false;
                                        if (null == instrument.Mep)
                                            problemList.Add(NoAttribute("mep"));
                                        else
                                            mep = instrumentList.Exists(s =>
                                            {
                                                return null == s.Mep ? false : s.Mep.Equals(instrument.Mep);
                                            });
                                        bool isin = false;
                                        if (null == instrument.Isin)
                                            problemList.Add(NoAttribute("isin"));
                                        else
                                            isin = instrumentList.Exists(s =>
                                            {
                                                return null == s.Isin ? false : s.Isin.Equals(instrument.Isin);
                                            });
                                        bool symbol = false;
                                        if (null == instrument.Symbol)
                                            problemList.Add(NoAttribute("symbol"));
                                        else
                                            symbol = instrumentList.Exists(s =>
                                            {
                                                return null == s.Symbol ? false : s.Symbol.Equals(instrument.Symbol);
                                            });
                                        bool name = false;
                                        if (null == instrument.Name)
                                            problemList.Add(NoAttribute("name"));
                                        else
                                            name = instrumentList.Exists(s =>
                                            {
                                                return null == s.Name ? false : s.Name.Equals(instrument.Name);
                                            });
                                        bool currency = false;
                                        if (null == instrument.Currency)
                                            problemList.Add(NoAttribute("currency"));
                                        else
                                            currency = instrumentList.Exists(s =>
                                            {
                                                return null == s.Currency ? currency : s.Currency.Equals(instrument.Currency);
                                            });
                                        bool type = false;
                                        if (null == instrument.Type)
                                            problemList.Add(NoAttribute("type"));
                                        else
                                            type = instrumentList.Exists(s =>
                                            {
                                                return null == s.Type ? false : s.Type.Equals(instrument.Type);
                                            });
                                        instrumentList.Add(instrument);
                                        if (mep || isin || symbol || name | currency | type)
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
                                            if (currency)
                                                stringBuilder.Append(" currency");
                                            if (type)
                                                stringBuilder.Append(" type");
                                            problemList.Add(stringBuilder.ToString());
                                        }
                                    }
                                    #endregion
                                    #region endofday element
                                    else if ("endofday" == str)
                                    {
                                        endofday = true;
                                        if (intraday)
                                            problemList.Add(string.Format("file {0} line {1}: both endofday and intraday defined", file, xmlLineInfo.LineNumber));
                                    }
                                    #endregion
                                    #region intraday element
                                    else if ("intraday" == str)
                                    {
                                        intraday = true;
                                        if (endofday)
                                            problemList.Add(string.Format("file {0} line {1}: both intraday and endofday defined", file, xmlLineInfo.LineNumber));
                                    }
                                    #endregion
                                    //problemList.Add("element " + s);
                                    //if (xmlReader.HasAttributes)
                                    //{
                                    //    while (xmlReader.MoveToNextAttribute())
                                    //        problemList.Add(">>attr " + xmlReader.Name + "=" + xmlReader.Value);
                                    //}
                                    break;
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
                        instrument.DateLast = JulianDayNumber.ToDateTime(thisJdn);
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
    }

    class Program
    {
        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                {
                    if (entry.EndsWith(".xml"))
                        action(entry);
                    else
                        Console.WriteLine("Skipping file [{0}]", entry);
                }
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
            {
                if (root.EndsWith(".xml"))
                    action(root);
                else
                    Console.WriteLine("Skipping file [{0}]", root);
            }
            else
                Console.WriteLine("Directory or file [{0}] is not found", root);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Arguments: [dir_or_file_name]");
            else
            {
                InstrumentFileXmlAuditor auditor = new InstrumentFileXmlAuditor();
                TraverseTree(args[0], s =>
                {
                    auditor.Audit(s).ForEach(x => Console.WriteLine(x));
                });
                Console.WriteLine("======================================================================");
                Console.WriteLine("Multi-instrument files");
                Console.WriteLine("======================================================================");
                InstrumentFileXmlAuditor.MultiInstrumentFileList.ForEach(x =>
                {
                    Console.WriteLine("{0} ------------------------------------------------------------");
                    x.ForEach(y => Console.WriteLine(y));
                });
                Console.WriteLine("======================================================================");
                Console.WriteLine("Files without data last N days");
                Console.WriteLine("======================================================================");
                DateTime dt = DateTime.Now.Date, d = dt.AddDays(-100);
                Console.WriteLine("-100 ------------------------------------------------------------");
                InstrumentFileXmlAuditor.TotalInstrumentFileList.ForEach(x =>
                {
                    x.ForEach(y =>
                    {
                        if (y.DateLast <= d)
                            Console.WriteLine(y);
                    });
                });
                d = dt.AddDays(-50);
                Console.WriteLine("-50 ------------------------------------------------------------");
                InstrumentFileXmlAuditor.TotalInstrumentFileList.ForEach(x =>
                {
                    x.ForEach(y =>
                    {
                        if (y.DateLast <= d)
                            Console.WriteLine(y);
                    });
                });
                d = dt.AddDays(-10);
                Console.WriteLine("-10 ------------------------------------------------------------");
                InstrumentFileXmlAuditor.TotalInstrumentFileList.ForEach(x =>
                {
                    x.ForEach(y =>
                    {
                        if (y.DateLast <= d)
                            Console.WriteLine(y);
                    });
                });
                Console.WriteLine("======================================================================");
                Console.WriteLine("Files with missing days");
                Console.WriteLine("======================================================================");
                InstrumentFileXmlAuditor.TotalInstrumentFileList.ForEach(x =>
                {
                    x.ForEach(y =>
                    {
                        if (y.MissingDaysList.Count > 0)
                            Console.WriteLine(y.ToStringMissingDates());
                    });
                });
                Console.WriteLine("======================================================================");
            }
        }
    }
}
