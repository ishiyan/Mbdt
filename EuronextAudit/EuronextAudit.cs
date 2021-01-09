using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using mbdt.Euronext;

namespace mbdt.EuronextAudit
{
    /// <summary>
    /// Euronext Audit utility.
    /// </summary>
    internal static class EuronextAudit
    {

        internal static void AuditTask()
        {
            EuronextInstrumentEnrichment.DownloadRetries = Properties.Settings.Default.DownloadRetries;
            EuronextInstrumentEnrichment.DownloadTimeout = Properties.Settings.Default.DownloadTimeout;
            EuronextInstrumentEnrichment.PauseBeforeRetry = Properties.Settings.Default.PauseBeforeRetry;

            EuronextInstrumentAudit.InstrumentsWithEqualFile(Properties.Settings.Default.ApprovedIndexPath, Properties.Settings.Default.DiscoveredIndexPath, null);
            EuronextInstrumentAudit.InstrumentsWithEqualIsin(Properties.Settings.Default.ApprovedIndexPath, Properties.Settings.Default.DiscoveredIndexPath, null);
            EuronextInstrumentAudit.InstrumentsWithEqualSymbol(Properties.Settings.Default.ApprovedIndexPath, Properties.Settings.Default.DiscoveredIndexPath, null);
            EuronextInstrumentAudit.InstrumentCluster(Properties.Settings.Default.ApprovedIndexPath, Properties.Settings.Default.DiscoveredIndexPath, null);

            //Audit(Properties.Settings.Default.ApprovedIndexPath);
            //Audit(Properties.Settings.Default.DiscoveredIndexPath);
            //EuronextInstrumentAudit.InstrumentsWithoutMic(Properties.Settings.Default.ApprovedIndexPath, "approved");
            //EuronextInstrumentAudit.InstrumentsWithoutMic(Properties.Settings.Default.DiscoveredIndexPath, "discovered");
            //SeparateMics("euronext.xml",
            //    new[] { "XLON", "XETR", "XMCE", "XVTX", "MTAA", "FRAA", "XCSE", "XSTO", "XOSL", "XHEL", "XMAD", "XIST", "XLIF", "XLDN" });
        }

        private static void Audit(string indexPath)
        {
            EuronextInstrumentXml.BackupXmlFile(indexPath, DateTime.Now);

            XDocument xdoc = XDocument.Load(indexPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();

            int i = 10;
            foreach (var xel in xelist)
            {
                xel.NormalizeElement(false);
                xel.EnrichElement(Properties.Settings.Default.UserAgent);
                if (++i % 10 == 0)
                        xdoc.Save(indexPath, SaveOptions.None);
            }
            xdoc.Save(indexPath, SaveOptions.None);
        }

        private static void SeparateMics(string indexPath, string[] mics)
        {
            EuronextInstrumentXml.BackupXmlFile(indexPath, DateTime.Now);

            XDocument xdoc = XDocument.Load(indexPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();

            int count = mics.Length;
            var lst = new XElement[count];
            // ReSharper disable once IdentifierTypo
            var lstd = new XDocument[count];
            for (int i = 0; i < count; ++i)
            {
                lstd[i] = new XDocument();
                var xelNew = new XElement("instruments");
                lst[i] = xelNew;
                lstd[i].Add(xelNew);
            }
            foreach (var xel in xelist)
            {
                string mic = xel.AttributeValue("mic");
                for (int i = 0; i < count; ++i)
                {
                    if (mic == mics[i])
                    {
                        xel.NormalizeElement(false);
                        xel.Remove();
                        lst[i].Add(xel);
                        break;
                    }
                }
            }
            xdoc.Save(indexPath, SaveOptions.None);
            for (int i = 0; i < count; ++i)
            {
                lstd[i].Save(indexPath+"."+mics[i]+".xml", SaveOptions.None);
            }
        }
    }
}
