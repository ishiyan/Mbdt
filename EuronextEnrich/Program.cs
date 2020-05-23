using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using mbdt.Euronext;

namespace mbdt.EuronextEnrich
{
    static class Program
    {
        private static void Enrich(string indexPath)
        {
            EuronextInstrumentXml.BackupXmlFile(indexPath, DateTime.Now);

            XDocument xdoc = XDocument.Load(indexPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();

            var list = new List<XAttribute>();
            int i = 10;
            foreach (var xel in xelist)
            {
                xel.NormalizeElement(false);

                XAttribute attr = xel.Attribute("foo");
                if (null != attr)
                    attr.Remove();
                string isin, mic, micName, symbol, name, type;
                if (!EuronextInstrumentEnrichment.SearchFirstInstrument(xel.AttributeValue(EuronextInstrumentXml.Isin), xel.AttributeValue(EuronextInstrumentXml.Type), out isin, out mic, out micName, out symbol, out name, out type))
                {
                    xel.SetAttributeValue(EuronextInstrumentXml.FoundInSearch, "false");
                }
                else
                {
                    if (null != type)
                        type = type.ToLowerInvariant();
                    if (xel.AttributeValue(EuronextInstrumentXml.Type) == EuronextInstrumentXml.Etv && type == EuronextInstrumentXml.Etf)
                        type = EuronextInstrumentXml.Etv;
                    else if (xel.AttributeValue(EuronextInstrumentXml.Type) == EuronextInstrumentXml.Inav && type == EuronextInstrumentXml.Index)
                        type = EuronextInstrumentXml.Inav;

                    string s = "";
                    s += xel.AttributeValue(EuronextInstrumentXml.Isin) == isin ? "" : ("-isin(" + isin + ")");
                    s += xel.AttributeValue(EuronextInstrumentXml.Mic) == mic ? "" : ("-mic(" + mic + ")");
                    s += xel.AttributeValue(EuronextInstrumentXml.Symbol) == symbol ? "" : ("-sym(" + symbol + ")");
                    s += xel.AttributeValue(EuronextInstrumentXml.Name) == name ? "" : ("-nam(" + name + ")");
                    s += xel.AttributeValue(EuronextInstrumentXml.Type) == type ? "" : ("-typ(" + type + ")");
                    if (s == "")
                        s = "true";
                    xel.SetAttributeValue(EuronextInstrumentXml.FoundInSearch, s);
                    xel.EnrichElement();
                }

                list.Clear();
                foreach (var a in xel.Attributes())
                {
                    list.Add(a);
                }
                xel.RemoveAttributes();

                attr = list.Find(a => a.Name == EuronextInstrumentXml.FoundInSearch);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Mic);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Isin);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Symbol);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Name);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Type);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.File);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Description);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Notes);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Mep);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                attr = list.Find(a => a.Name == EuronextInstrumentXml.Vendor);
                if (null != attr)
                {
                    xel.SetAttributeValue(attr.Name, attr.Value);
                    list.Remove(attr);
                }
                foreach (var a in list)
                {
                    xel.SetAttributeValue(a.Name, a.Value);
                }


                if (++i % 10 == 0)
                    xdoc.Save(indexPath, SaveOptions.None);
            }
            xdoc.Save(indexPath, SaveOptions.None);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Argument: indexFile.xml {indexFileDelisted.xml}");
                return;
            }
            

            Enrich(args[0]);
            string delisted = args.Length > 1 ? args[1] : null;
            if (null != delisted)
                Enrich(delisted);

            EuronextInstrumentAudit.InstrumentsWithEqualIsin(args[0], delisted, null);
            EuronextInstrumentAudit.InstrumentsWithEqualSymbol(args[0], delisted, null);
            EuronextInstrumentAudit.InstrumentsWithEqualFile(args[0], delisted, null);
            //EuronextInstrumentAudit.InstrumentCluster(args[0], delisted, null);
        }
    }
}
