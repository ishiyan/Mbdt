using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Linq;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext instrument enrichment utilities.
    /// </summary>
    internal static class EuronextInstrumentEnrichment
    {
        private static bool firstTime = true;

        static EuronextInstrumentEnrichment()
        {
            // Skip validation of SSL/TLS certificate
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Ssl3;
        }

        #region DownloadTimeout
        /// <summary>
        /// In milliseconds.
        /// </summary>
        internal static int DownloadTimeout = 180000;
        #endregion

        #region DownloadRetries
        internal static int DownloadRetries = 5;
        #endregion

        #region PauseBeforeRetry
        /// <summary>
        /// In milliseconds.
        /// </summary>
        internal static int PauseBeforeRetry = 3000;
        #endregion

        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:16.0) Gecko/20100101 Firefox/16.0";

        #region NewInstrumentElement
        /// <summary>
        /// Creates a new normalized instrument element from the specified instrument info and optionally enriches it.
        /// </summary>
        internal static XElement NewInstrumentElement(this EuronextActualInstruments.InstrumentInfo instrumentInfo, bool enrich)
        {
            var xel = new XElement(EuronextInstrumentXml.Instrument);
            xel.AttributeValue(EuronextInstrumentXml.Currency, "EUR");
            //xel.AttributeValue(EuronextInstrumentXml.File, string.Concat(instrumentInfo.Type, "/", instrumentInfo.Symbol, ".xml"));
            xel.AttributeValue(EuronextInstrumentXml.File, string.Concat(instrumentInfo.Mic.ToLowerInvariant(), "/", instrumentInfo.Type, "/", instrumentInfo.Symbol, ".h5:/", instrumentInfo.Mic, "_", instrumentInfo.Symbol, "_", instrumentInfo.Isin));
            xel.AttributeValue(EuronextInstrumentXml.Isin, instrumentInfo.Isin);
            xel.AttributeValue(EuronextInstrumentXml.Mic, instrumentInfo.Mic);
            xel.AttributeValue(EuronextInstrumentXml.Mep, instrumentInfo.Mep);
            xel.AttributeValue(EuronextInstrumentXml.Name, instrumentInfo.Name);
            xel.AttributeValue(EuronextInstrumentXml.Symbol, instrumentInfo.Symbol);
            xel.AttributeValue(EuronextInstrumentXml.Type, instrumentInfo.Type);
            xel.AttributeValue(EuronextInstrumentXml.Description, "");
            xel.AttributeValue(EuronextInstrumentXml.Vendor, EuronextInstrumentXml.Euronext);
            switch (instrumentInfo.Type)
            {
                case EuronextInstrumentXml.Stock:
                    //EnrichStock(xel);
                    break;
                case EuronextInstrumentXml.Index:
                    //NormalizeIndex(xel);
                    break;
                case EuronextInstrumentXml.Etf:
                    //EnrichEtf(xel);
                    break;
                case EuronextInstrumentXml.Etv:
                    //EnrichEtv(xel);
                    break;
            }
            return xel;
        }
        #endregion

        #region ValidateInstrumentElement
        /// <summary>
        /// Validates and normalized the given instrument element from the specified instrument info and optionally enriches it.
        /// </summary>
        internal static void ValidateInstrumentElement(this EuronextActualInstruments.InstrumentInfo ii, XElement xel, bool enrich, string userAgent)
        {
            XAttribute xatr = xel.Attribute(EuronextInstrumentXml.Mic);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Mic, ii.Mic));
            else if (xatr.Value != ii.Mic)
                xel.AttributeValue(EuronextInstrumentXml.Mic, ii.Mic);

            xatr = xel.Attribute(EuronextInstrumentXml.Mep);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Mep, ii.Mep));
            else if (xatr.Value != ii.Mep)
                xel.AttributeValue(EuronextInstrumentXml.Mep, ii.Mep);

            xatr = xel.Attribute(EuronextInstrumentXml.Symbol);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Symbol, ii.Symbol));
            else if (xatr.Value != ii.Symbol)
                xel.AttributeValue(EuronextInstrumentXml.Symbol, ii.Symbol);

            xatr = xel.Attribute(EuronextInstrumentXml.Name);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Name, ii.Name));
            else if (xatr.Value != ii.Name)
                xel.AttributeValue(EuronextInstrumentXml.Name, ii.Name);

            xatr = xel.Attribute(EuronextInstrumentXml.Description);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Description, ""));

            xatr = xel.Attribute(EuronextInstrumentXml.Type);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Type, ii.Type));
            else if (xatr.Value != ii.Type)
                xel.AttributeValue(EuronextInstrumentXml.Type, ii.Type);

            xatr = xel.Attribute(EuronextInstrumentXml.File);
            if (null == xatr)
                Trace.TraceError("File attribute is not defined in element [{0}]", xel.ToString(SaveOptions.None));

            xatr = xel.Attribute(EuronextInstrumentXml.Vendor);
            if (null == xatr)
                xel.Add(new XAttribute(EuronextInstrumentXml.Vendor, EuronextInstrumentXml.Euronext));
            else if (xatr.Value != EuronextInstrumentXml.Euronext)
                xel.AttributeValue(EuronextInstrumentXml.Vendor, EuronextInstrumentXml.Euronext);

            switch (ii.Type)
            {
                case EuronextInstrumentXml.Stock:
                    xel.EnrichStockElement(userAgent);
                    break;
                case EuronextInstrumentXml.Index:
                    xel.EnrichIndexElement(userAgent);
                    break;
                case EuronextInstrumentXml.Etf:
                    xel.EnrichEtfElement(userAgent);
                    break;
                case EuronextInstrumentXml.Etv:
                    xel.EnrichEtvElement(userAgent);
                    break;
                case EuronextInstrumentXml.Inav:
                    xel.EnrichInavElement(userAgent);
                    break;
                case EuronextInstrumentXml.Fund:
                    xel.EnrichFundElement(userAgent);
                    break;
            }
        }
        #endregion

        #region EnrichElement
        internal static void EnrichElement(this XElement xel, string userAgent)
        {
            //xel.EnrichSearchInstrument();
            string type = xel.AttributeValue(EuronextInstrumentXml.Type);
            if ("" == type)
                return;
            switch (type)
            {
                case EuronextInstrumentXml.Stock:
                    xel.EnrichStockElement(userAgent);
                    break;
                case EuronextInstrumentXml.Index:
                    xel.EnrichIndexElement(userAgent);
                    break;
                case EuronextInstrumentXml.Etf:
                    xel.EnrichEtfElement(userAgent);
                    break;
                case EuronextInstrumentXml.Etv:
                    xel.EnrichEtvElement(userAgent);
                    break;
                case EuronextInstrumentXml.Inav:
                    xel.EnrichInavElement(userAgent);
                    break;
                case EuronextInstrumentXml.Fund:
                    xel.EnrichFundElement(userAgent);
                    break;
            }
        }
        #endregion

        #region EnrichIndexElement
        internal static void EnrichIndexElement(this XElement xel, string userAgent)
        {
            xel.NormalizeIndexElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Index, userAgent);
        }
        #endregion

        #region EnrichStockElement
        /// <summary>
        /// Normalizes and enriches the stock element.
        /// </summary>
        internal static void EnrichStockElement(this XElement xel, string userAgent)
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

            xel.NormalizeStockElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Stock);
            XElement xelStock = xel.Element(EuronextInstrumentXml.Stock);
            // ReSharper disable PossibleNullReferenceException
            XElement xelIcb = xelStock.Element(EuronextInstrumentXml.Icb);
            // ReSharper restore PossibleNullReferenceException

            const string uriFormat = "https://live.euronext.com/en/product/equities/{0}-{1}/market-information";
            const string refererFormat = "https://live.euronext.com/en/product/equities/{0}-{1}";
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string marketInformation = DownloadTextString("market-information", uri, DownloadRetries, DownloadTimeout, referer, userAgent);
            if (string.IsNullOrEmpty(marketInformation))
                return;

            // <td>CFI:ESVUFN</td>
            string value = Extract(marketInformation, "Classification Financial Instrument", "<td>CFI:", "</td>");
            if (!string.IsNullOrEmpty(value))
                xelStock.AttributeValue(EuronextInstrumentXml.Cfi, value.ToUpperInvariant());

            // <td>Trading currency</td>
            // <td><strong>EUR</strong></td>
            value = Extract(marketInformation, "<td>Trading currency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelStock.AttributeValue(EuronextInstrumentXml.Currency, value.ToUpperInvariant());

            // <td>Trading type</td>
            // <td><strong>Continuous</strong></td>
            value = Extract(marketInformation, "<td>Trading type</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                value = value.ToLowerInvariant();
                // ReSharper disable once StringLiteralTypo
                if (value == "continous")
                    value = "continuous";
                xelStock.AttributeValue(EuronextInstrumentXml.TradingMode, value);
            }

            // <strong>Compartment A (Large Cap)</strong>
            if (marketInformation.Contains("<strong>Compartment A "))
                value = "A";
            else if (marketInformation.Contains("<strong>Compartment B "))
                value = "B";
            else if (marketInformation.Contains("<strong>Compartment C "))
                value = "C";
            else
                value = "";
            if (0 < value.Length)
                xelStock.AttributeValue(EuronextInstrumentXml.Compartment, value);

            // <td>Shares outstanding</td>
            // <td><strong>270,045,923</strong></td>
            value = Extract(marketInformation, "<td>Shares outstanding</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelStock.AttributeValue(EuronextInstrumentXml.Shares, value.ToLowerInvariant());

            // <td>Industry</td>
            // <td><strong>5000, Consumer Services</strong></td>
            value = Extract(marketInformation, "<td>Industry</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                int i = value.IndexOf(",", StringComparison.Ordinal);
                if (i > 0)
                    value = value.Substring(0, i).Trim();
                if ("-" == value)
                    value = "";
                if (!string.IsNullOrEmpty(value))
                    xelIcb.AttributeValue(EuronextInstrumentXml.Icb1, value);
            }

            // <td>SuperSector</td>
            // <td><strong>5700, Travel &amp; Leisure</strong></td>
            value = Extract(marketInformation, "<td>SuperSector</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                int i = value.IndexOf(",", StringComparison.Ordinal);
                if (i > 0)
                    value = value.Substring(0, i).Trim();
                if ("-" == value)
                    value = "";
                if (!string.IsNullOrEmpty(value))
                    xelIcb.AttributeValue(EuronextInstrumentXml.Icb2, value);
            }

            // <td>Sector</td>
            // <td><strong>5750, Travel &amp; Leisure</strong></td>
            value = Extract(marketInformation, "<td>Sector</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                int i = value.IndexOf(",", StringComparison.Ordinal);
                if (i > 0)
                    value = value.Substring(0, i).Trim();
                if ("-" == value)
                    value = "";
                if (!string.IsNullOrEmpty(value))
                    xelIcb.AttributeValue(EuronextInstrumentXml.Icb3, value);
            }

            // <td>Subsector</td>
            // <td><strong>5753, Hotels</strong></td>
            value = Extract(marketInformation, "<td>Subsector</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                int i = value.IndexOf(",", StringComparison.Ordinal);
                if (i > 0)
                    value = value.Substring(0, i).Trim();
                if ("-" == value)
                    value = "";
                if (!string.IsNullOrEmpty(value))
                    xelIcb.AttributeValue(EuronextInstrumentXml.Icb4, value);
            }

            // <td>Price multiplier</td>
            // <td><strong>1</strong></td>
            value = Extract(marketInformation, "<td>Price multiplier</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                // xelStock.AttributeValue(EuronextInstrumentXml.PriceMultiplier, value.ToLowerInvariant());
            }
        }
        #endregion

        #region EnrichEtfElement
        /// <summary>
        /// Normalizes and enriches the ETF element.
        /// </summary>
        internal static void EnrichEtfElement(this XElement xel, string userAgent)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="FR0010754135" symbol="C13" name="AMUNDI ETF EMTS1-3" type="etf"
            //     file="etf/C13.xml"
            //     description="Amundi ETF Govt Bond EuroMTS Broad 1-3"
            //     >
            //     <etf cfi="EUOM" ter="0.14" tradingMode="continuous" launchDate="20100316" currency="EUR" issuer="AMUNDI" fraction="1" dividendFrequency="Annually" indexFamily="EuroMTS" expositionType="synthetic">
            //         <inav vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011161377" symbol="INC13" name="AMUNDI C13 INAV"/>
            //         <underlying vendor="Euronext" mep="PAR" mic="XPAR" isin="QS0011052618" symbol="EMTSAR" name="EuroMTS Eurozone Government Broad 1-3"/>
            //     </etf>
            // </instrument>

            xel.NormalizeEtfElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Etf, userAgent);
            XElement xelEtf = xel.Element(EuronextInstrumentXml.Etf);
            // ReSharper disable PossibleNullReferenceException
            XElement xelInav = xelEtf.Element(EuronextInstrumentXml.Inav);
            XElement xelUnderlying = xelEtf.Element(EuronextInstrumentXml.Underlying);
            // ReSharper restore PossibleNullReferenceException

            const string uriFormat = "https://live.euronext.com/en/product/etfs/{0}-{1}/market-information";
            const string refererFormat = "https://live.euronext.com/en/product/etfs/{0}-{1}";
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string marketInformation = DownloadTextString("market-information", uri, DownloadRetries, DownloadTimeout, referer, userAgent);
            if (string.IsNullOrEmpty(marketInformation))
                return;

            // <td>ETF Legal Name</td>
            // <td><strong>AMUNDI INDEX FTSE EPRA NAREIT GLOBAL UCITS ETF DR</strong></td>
            string value = Extract(marketInformation, "<td>ETF Legal Name</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xel.AttributeValue(EuronextInstrumentXml.Description, value.ToUpperInvariant());

            // <tr><td>CFI:CEOJU</td></tr>
            value = Extract(marketInformation, "<tr><td>CFI:", "</td></tr>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.Cfi, value.ToUpperInvariant());

            // <td>TER</td>
            // <td><strong>0.24%</strong></td>
            value = Extract(marketInformation, "<td>TER</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.Ter, value.ToUpperInvariant());

            // <td>Trading type</td>
            // <td><strong>Continuous</strong></td>
            value = Extract(marketInformation, "<td>Trading type</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                value = value.ToLowerInvariant();
                // ReSharper disable once StringLiteralTypo
                if (value == "continous")
                    value = "continuous";
                xelEtf.AttributeValue(EuronextInstrumentXml.TradingMode, value);
            }

            // <td>Launch Date</td>
            // <td><strong>17/11/2016</strong></td>
            value = Extract(marketInformation, "<td>Launch Date</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                // TODO: convert to yyyyMMdd
                xelEtf.AttributeValue(EuronextInstrumentXml.LaunchDate, value.ToUpperInvariant());

            // <td>Trading currency</td>
            // <td><strong>EUR</strong></td>
            value = Extract(marketInformation, "<td>Trading currency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.Currency, value.ToUpperInvariant());

            // <td>Dividend frequency</td>
            // <td><strong>Annually</strong></td>
            value = Extract(marketInformation, "<td>Dividend frequency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.DividendFrequency, value.ToLowerInvariant());

            // <td>Issuer Name</td>
            // <td><strong>Amundi Asset Management</strong></td>
            value = Extract(marketInformation, "<td>Issuer Name</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.Issuer, value);

            // <td>Exposition type</td>
            // <td><strong>Synthetic</strong></td>
            value = Extract(marketInformation, "<td>Exposition type</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtf.AttributeValue(EuronextInstrumentXml.ExpositionType, value.ToLowerInvariant());

            // <td>INAV ISIN code</td>
            // <td><strong>QS0011161377</strong></td>
            value = Extract(marketInformation, "<td>INAV ISIN code</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelInav.AttributeValue(EuronextInstrumentXml.Isin, value);

            // <td>Ticker INAV (Euronext)</td>
            // <td><strong>INC13</strong></td>
            value = Extract(marketInformation, "<td>Ticker INAV (Euronext)</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelInav.AttributeValue(EuronextInstrumentXml.Symbol, value);

            // <td>INAV Name</td>
            // <td><strong>AMUNDI C13 INAV</strong></td>
            value = Extract(marketInformation, "<td>INAV Name</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelInav.AttributeValue(EuronextInstrumentXml.Name, value);

            // <td>Underlying index</td>
            // <td><strong>EuroMTS Investment 1-3</strong></td>
            value = Extract(marketInformation, "<td>Underlying index</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelUnderlying.AttributeValue(EuronextInstrumentXml.Name, value);

            // <td>Index</td>
            // <td><strong>EMIGA5</strong></td>
            /*value = Extract(marketInformation, "<td>Index</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelUnderlying.AttributeValue("???????", value);*/

            //xelInav.EnrichSearchInstrument(EuronextInstrumentXml.Inav, userAgent);
            //xelUnderlying.EnrichSearchInstrument();
        }
        #endregion

        #region EnrichEtvElement
        internal static void EnrichEtvElement(this XElement xel, string userAgent)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="GB00B15KXP72" symbol="COFFP" name="ETFS COFFEE" type="etv"
            //     file="etf/COFFP.xml"
            //     description=""
            //     >
            //     <etv cfi="DTZSPR" tradingMode="continuous" allInFees="0,49%" expenseRatio="" dividendFrequency="yearly" currency="EUR" issuer="ETFS COMMODITY SECURITIES LTD" shares="944,000">
            // </instrument>

            xel.NormalizeEtvElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Etv, userAgent);
            XElement xelEtv = xel.Element(EuronextInstrumentXml.Etv);

            const string uriFormat = "https://live.euronext.com/en/product/etvs/{0}-{1}/market-information";
            const string refererFormat = "https://live.euronext.com/en/product/etvs/{0}-{1}";
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string marketInformation = DownloadTextString("market-information", uri, DownloadRetries, DownloadTimeout, referer, userAgent);
            if (string.IsNullOrEmpty(marketInformation))
                return;

            // <td>CFI:DEMMRC</td>
            string value = Extract(marketInformation, "Classification Financial Instrument", "<td>CFI:", "</td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.Cfi, value.ToUpperInvariant());

            // <td>All In Fees</td>
            // <td><strong>0,49%</strong></td>
            value = Extract(marketInformation, "<td>All In Fees</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.AllInFees, value);

            // <td>Trading type</td>
            // <td><strong>Continuous</strong></td>
            value = Extract(marketInformation, "<td>Trading type</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                value = value.ToLowerInvariant();
                // ReSharper disable once StringLiteralTypo
                if (value == "continous")
                    value = "continuous";
                xelEtv.AttributeValue(EuronextInstrumentXml.TradingMode, value);
            }

            // <td>Trading currency</td>
            // <td><strong>EUR</strong></td>
            value = Extract(marketInformation, "<td>Trading currency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.Currency, value.ToUpperInvariant());

            // <td>Dividend frequency</td>
            // <td><strong>Annually</strong></td>
            value = Extract(marketInformation, "<td>Dividend frequency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.DividendFrequency, value.ToLowerInvariant());

            // <td>Expense Ratio</td>
            // <td><strong>Annually</strong></td>
            value = Extract(marketInformation, "<td>Expense Ratio</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.ExpenseRatio, value.ToLowerInvariant());

            // <td>Issuer Name</td>
            // <td><strong>BNP PARIBAS ARBITRAGE ISSUANCE</strong></td>
            value = Extract(marketInformation, "<td>Issuer Name</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.Issuer, value);

            // <td>Shares Outstanding</td>
            // <td><strong>500</strong></td>
            value = Extract(marketInformation, "<td>Shares Outstanding</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelEtv.AttributeValue(EuronextInstrumentXml.Shares, value);
        }
        #endregion

        #region EnrichFundElement
        /// <summary>
        /// Normalizes and enriches the element.
        /// </summary>
        internal static void EnrichFundElement(this XElement xel, string userAgent)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" mic="XAMS" isin="NL0006259996" symbol="AWAF" name="ACH WERELD AANDFD3" type="fund"
            //     file="fund/AWAF.xml"
            //     description=""
            //     >
            //     <fund cfi="EUOISB" tradingmode="fixing" currency="EUR" issuer="ACHMEA BELEGGINGSFONDSEN" shares="860,248">
            // </instrument>

            xel.NormalizeFundElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Fund, string userAgent);
            XElement xelFund = xel.Element(EuronextInstrumentXml.Fund);

            const string uriFormat = "https://live.euronext.com/en/product/funds/{0}-{1}/market-information";
            const string refererFormat = "https://live.euronext.com/en/product/funds/{0}-{1}";
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            string uri = string.Format(uriFormat, isin, mic);
            string referer = string.Format(refererFormat, isin, mic);
            string marketInformation = DownloadTextString("market-information", uri, DownloadRetries, DownloadTimeout, referer, userAgent);
            if (string.IsNullOrEmpty(marketInformation))
                return;

            // <tr><td>CFI:CIOS</td></tr>
            string value = Extract(marketInformation, "<tr><td>CFI:", "</td></tr>");
            if (!string.IsNullOrEmpty(value))
                xelFund.AttributeValue(EuronextInstrumentXml.Cfi, value.ToUpperInvariant());

            // <td>Trading Type</td>
            // <td><strong>Fixing</strong></td>
            value = Extract(marketInformation, "<td>Trading type</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
            {
                value = value.ToLowerInvariant();
                // ReSharper disable once StringLiteralTypo
                if (value == "continous")
                    value = "continuous";
                xelFund.AttributeValue(EuronextInstrumentXml.TradingMode, value);
            }

            // <td>Trading currency</td>
            // <td><strong>EUR</strong></td>
            value = Extract(marketInformation, "<td>Trading currency</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelFund.AttributeValue(EuronextInstrumentXml.Currency, value.ToUpperInvariant());

            // <p>Name :  <strong>ACTIAM BELEGGINGSFONDSEN NV</strong></p>
            value = Extract(marketInformation, "<p>Name :  <strong>", "</strong></p>");
            if (!string.IsNullOrEmpty(value))
                xelFund.AttributeValue(EuronextInstrumentXml.Issuer, value);

            // <td>Shares Outstanding</td>
            // <td><strong>2,422,386</strong></td>
            value = Extract(marketInformation, "<td>Shares Outstanding</td>", "<td><strong>", "</strong></td>");
            if (!string.IsNullOrEmpty(value))
                xelFund.AttributeValue(EuronextInstrumentXml.Shares, value);
        }
        #endregion

        #region EnrichInavElement
        /// <summary>
        /// Normalizes and enriches the iNAV element.
        /// </summary>
        internal static void EnrichInavElement(this XElement xel, string userAgent)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" isin="QS0011161385" symbol="INC33" name="AMUNDI C33 INAV" type="inav"
            //     file="etf/INC33.xml"
            //     description="iNav Amundi ETF Govt Bond EuroMTS Broad 3-5"
            //     >
            //     <inav currency="EUR">
            //         <target vendor="Euronext" mep="PAR" mic="XPAR" isin="FR0010754168" symbol="C33" name="AMUNDI ETF GOV 3-5"/>
            //     </inav>
            // </instrument>

            xel.NormalizeInavElement();
            //xel.EnrichSearchInstrument(EuronextInstrumentXml.Inav, userAgent);
        }
        #endregion

        #region Extract
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
        private static string Extract(string text, string prefix1, string prefix2, string suffix2)
        {
            int i = text.IndexOf(prefix1, StringComparison.Ordinal);
            if (i >= 0)
            {
                string s = text.Substring(i + prefix1.Length);
                i = s.IndexOf(prefix2, StringComparison.Ordinal);
                if (i > 0)
                {
                    s = s.Substring(i + prefix2.Length);
                    i = s.IndexOf(suffix2, StringComparison.Ordinal);
                    if (i > 0)
                    {
                        s = s.Substring(0, i).Trim();
                        if (s == "-")
                            s = "";
                        return s;
                    }
                }
            }
            return "";
        }
        #endregion

        #region DownloadTextString
        private static string DownloadTextString(string what, string uri, int retries, int timeout, string referer = null, string userAgent = null)
        {
            if (firstTime)
            {
                firstTime = false;
                string s = DownloadTextString(what, uri, retries, timeout, referer, userAgent);
                if (s != null)
                    return s;
            }
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
                        userAgent = DefaultUserAgent;
                    webRequest.UserAgent = userAgent;
                    webRequest.Timeout = timeout;
                    webRequest.Accept = "text/html, */*";
                    webRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
                    webRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                    webRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    webRequest.Headers.Add("DNT", "1");
                    webRequest.KeepAlive = true;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
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
                        "Download failed [{0}], retrying ({1})" : "Download failed [{0}], giving up ({1})",
                        exception.Message, retries);
                    retries--;
                    Thread.Sleep(PauseBeforeRetry);
                }
            }
            return null;
        }
        #endregion

        #region SearchFirstInstrument
        /// <summary>
        /// Searches a <c>what</c> argument which has a type <c>whatType</c>.
        /// Returns <c>true</c> if at least one of output arguments is not null, <c>false</c> otherwise.
        /// <c>What</c> can be: symbol, isin or name.
        /// <c>WhatType</c> can be: index, inav, stock, etf, etv, fund.
        /// If <c>whatType</c> is null or empty, all types mentioned above will be searched.
        /// </summary>
        internal static bool SearchFirstInstrument(string what, string whatType, out string isin, out string mic, out string micName, out string symbol, out string name, out string type, string userAgent = null)
        {
            string uri, referer;
            isin = null;
            mic = null;
            micName = null;
            symbol = null;
            name = null;
            type = null;
            switch (whatType)
            {
                case EuronextInstrumentXml.Index:
                case EuronextInstrumentXml.Inav:
                {
                    const string indexSearchFormat = "https://live.euronext.com/en/search_instruments/{0}?type=Index";
                    const string indexReferrerFormat = "https://live.euronext.com/en";
                    uri = string.Format(indexSearchFormat, what);
                    referer = indexReferrerFormat;
                    break;
                }
                case EuronextInstrumentXml.Stock:
                {
                    const string indexSearchFormat = "https://live.euronext.com/en/search_instruments/{0}?type=Stock";
                    const string indexReferrerFormat = "https://live.euronext.com/en";
                    uri = string.Format(indexSearchFormat, what);
                    referer = indexReferrerFormat;
                    break;
                }
                case EuronextInstrumentXml.Etf:
                case EuronextInstrumentXml.Etv:
                {
                    const string indexSearchFormat = "https://live.euronext.com/en/search_instruments/{0}?type=Trackers";
                    const string indexReferrerFormat = "https://live.euronext.com/en";
                    uri = string.Format(indexSearchFormat, what);
                    referer = indexReferrerFormat;
                    break;
                }
                case EuronextInstrumentXml.Fund:
                {
                    const string indexSearchFormat = "https://live.euronext.com/en/search_instruments/{0}?type=Funds";
                    const string indexReferrerFormat = "https://live.euronext.com/en";
                    uri = string.Format(indexSearchFormat, what);
                    referer = indexReferrerFormat;
                    break;
                }
                default:
                {
                    const string indexSearchFormat = "https://live.euronext.com/en/search_instruments/{0}?type=All";
                    const string indexReferrerFormat = "https://live.euronext.com/en";
                    uri = string.Format(indexSearchFormat, what);
                    referer = indexReferrerFormat;
                    break;
                }
            }
            string searchSheet = DownloadTextString("search-sheet", uri, DownloadRetries, DownloadTimeout, referer, userAgent);
            if (string.IsNullOrEmpty(searchSheet))
                return false;
            if (searchSheet.Contains("No Instrument corresponds to your search"))
                return false;

            // <table id="awl-lookup-instruments-directory-table" class="responsive-enabled table table-striped" data-striping="1">
            //
            //    <thead class=''>
            //    <tr>
            //    <th>Symbol</th>
            //    <th>Name</th>
            //    <th>ISIN</th>
            //    <th>Exchange</th>
            //    <th>Market</th>
            //    <th>Type</th>
            //    </tr>
            //    </thead>
            //
            //    <tbody>
            //    <tr class="odd">
            //    <td>NLFIN</td>
            //
            //    <td><a href = "/en/product/indices/qs0011016605-xams/aex-financials/nlfin" target="">AEX FINANCIALS</a></td>
            //    <td>QS0011016605</td>
            //    <td>Euronext Amsterdam</td>
            //    <td>XAMS</td>
            //    <td>Index</td>
            //    </tr>
            string value = Extract(searchSheet, "<table id=\"awl-lookup-instruments-directory-table\"", "</table>");
            if (!string.IsNullOrEmpty(value))
            {
                value = Extract(value, "<tbody>", "</tbody>");
                if (!string.IsNullOrEmpty(value))
                {
                    value = Extract(value, "<tr", "</tr>");
                    if (!string.IsNullOrEmpty(value))
                    {
                        string[] splitted = value.Split(new[] {"</td>"}, StringSplitOptions.None);
                        if (splitted.Length > 5)
                        {
                            value = splitted[0];
                            int i = value.LastIndexOf('>');
                            if (i > 0)
                                symbol = value.Substring(i + 1);
                            if ("-" == symbol)
                                symbol = null;
                            value = splitted[1];
                            if (value.Contains("<td><a href"))
                            {
                                i = value.IndexOf("<td><a href", StringComparison.Ordinal);
                                value = value.Substring(i + "<td><a href".Length);
                                i = value.IndexOf('>');
                                if (i > 0)
                                {
                                    value = value.Substring(i + 1);
                                    i = value.IndexOf('<');
                                    value = i > 0 ? value.Substring(0, i) : null;
                                }
                                else
                                    value = null;
                                if ("-" == value)
                                    value = null;
                                if (!string.IsNullOrEmpty(value))
                                    name = value.Replace("&amp;", "&").Replace("&#039;", "'");
                            }
                            value = splitted[2];
                            if (value.Contains("<td>"))
                            {
                                i = value.IndexOf("<td>", StringComparison.Ordinal);
                                value = value.Substring(i + "<td>".Length);
                                if ("-" == value)
                                    value = null;
                                if (!string.IsNullOrEmpty(value))
                                    isin = value;
                            }
                            value = splitted[3];
                            if (value.Contains("<td>"))
                            {
                                i = value.IndexOf("<td>", StringComparison.Ordinal);
                                value = value.Substring(i + "<td>".Length);
                                if ("-" == value)
                                    value = null;
                                if (!string.IsNullOrEmpty(value))
                                    micName = value;
                            }
                            value = splitted[4];
                            if (value.Contains("<td>"))
                            {
                                i = value.IndexOf("<td>", StringComparison.Ordinal);
                                value = value.Substring(i + "<td>".Length);
                                if ("-" == value)
                                    value = null;
                                if (!string.IsNullOrEmpty(value))
                                    mic = value;
                            }
                            value = splitted[5];
                            if (value.Contains("<td>"))
                            {
                                i = value.IndexOf("<td>", StringComparison.Ordinal);
                                value = value.Substring(i + "<td>".Length);
                                if ("-" == value)
                                    value = null;
                                if (!string.IsNullOrEmpty(value))
                                    type = value.Trim(' ').ToLowerInvariant();
                            }
                        }
                    }
                }
            }
            else
            {
                // ,"instrument":{"product_data":"NL0000009538-XAMS","type":"STOCK","name":"PHILIPS KON","symbol":"PHIA","isin":"NL0000009538","mic":"XAMS"},
                value = Extract(searchSheet, ",\"instrument\":{", "},");
                if (!string.IsNullOrEmpty(value))
                {
                    string s = Extract(value, "\"type\":\"", "\"");
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (s.ToLowerInvariant().StartsWith("ind"))
                            type = "index";
                        else if (s.ToLowerInvariant().StartsWith("stock"))
                            type = "stock";
                        // ReSharper disable once StringLiteralTypo
                        else if (s.ToLowerInvariant().StartsWith("equit"))
                            type = "stock";
                        else if (s.ToLowerInvariant().StartsWith("etf"))
                            type = "etf";
                        else if (s.ToLowerInvariant().StartsWith("etv"))
                            type = "etv";
                        else if (s.ToLowerInvariant().StartsWith("fund"))
                            type = "fund";
                    }
                    s = Extract(value, "\"name\":\"", "\"");
                    if (!string.IsNullOrEmpty(s))
                        name = s.Replace("&amp;", "&").Replace("&#039;", "'");
                    s = Extract(value, "\"symbol\":\"", "\"");
                    if (!string.IsNullOrEmpty(s))
                        symbol = s;
                    s = Extract(value, "\"isin\":\"", "\"");
                    if (!string.IsNullOrEmpty(s))
                        isin = s;
                    s = Extract(value, "\"mic\":\"", "\"");
                    if (!string.IsNullOrEmpty(s))
                        mic = s;
                }
            }
            return null != isin || null != symbol || null != name || null != micName || null != mic || null != type;
        }
        #endregion

        #region EnrichSearchInstrument
        internal static void EnrichSearchInstrument(this XElement xel, string type = null, string userAgent = null)
        {
            string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
            string symbol = xel.AttributeValue(EuronextInstrumentXml.Symbol);
            string name = xel.AttributeValue(EuronextInstrumentXml.Name);
            string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
            bool isSomethingMissing = "" == mic || "" == isin; //|| "" == symbol || "" == name;
            if (!isSomethingMissing)
                return;
            string what = null;
            if ("" != isin)
                what = isin;
            else if ("" != symbol)
                what = symbol;
            else if ("" != name)
                what = name;
            if (null != what)
            {
                if (string.IsNullOrEmpty(type))
                    type = xel.AttributeValue(EuronextInstrumentXml.Type);
                if ("" == type)
                    type = "All";
                if (SearchFirstInstrument(what, type, out var searchIsin, out var searchMic, out var searchMicName, out var searchSymbol, out var searchName, out var searchType, userAgent))
                {
                    if (!string.IsNullOrEmpty(searchIsin) && "" == isin)
                        xel.AttributeValue(EuronextInstrumentXml.Isin, searchIsin);
                    if (!string.IsNullOrEmpty(searchMic) && "" == mic)
                    {
                        xel.AttributeValue(EuronextInstrumentXml.Mic, searchMic);
                        if (EuronextActualInstruments.KnownMicToMepDictionary.ContainsKey(searchMic))
                            xel.AttributeValue(EuronextInstrumentXml.Mep, EuronextActualInstruments.KnownMicToMepDictionary[searchMic]);
                        xel.AttributeValue(EuronextInstrumentXml.Vendor, EuronextInstrumentXml.Euronext);
                    }
                    if (!string.IsNullOrEmpty(searchSymbol) && "" == symbol)
                        xel.AttributeValue(EuronextInstrumentXml.Symbol, searchSymbol);
                    if (!string.IsNullOrEmpty(searchName) && "" == name)
                        xel.AttributeValue(EuronextInstrumentXml.Name, searchName);
                    if (!string.IsNullOrEmpty(searchType) && "" == xel.AttributeValue(EuronextInstrumentXml.Type))
                        xel.AttributeValue(EuronextInstrumentXml.Type, searchType);
                }
            }
        }
        #endregion
    }
}
