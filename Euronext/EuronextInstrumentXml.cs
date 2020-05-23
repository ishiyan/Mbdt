using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace mbdt.Euronext
{
    /// <summary>
    /// Fetches the actual instrument lists from the Euronext.
    /// </summary>
    internal static class EuronextInstrumentXml
    {
        #region Constants
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Global
        internal const string Fund = "fund";
        internal const string Etv = "etv";
        internal const string Etf = "etf";
        internal const string Index = "index";
        internal const string Stock = "stock";
        internal const string Inav = "inav";

        internal const string Type = "type";
        internal const string Instrument = "instrument";
        internal const string Isin = "isin";
        internal const string Mic = "mic";
        internal const string Mep = "mep";
        internal const string Symbol = "symbol";
        internal const string Name = "name";
        internal const string File = "file";
        internal const string Currency = "currency";
        internal const string Description = "description";
        internal const string Vendor = "vendor";
        internal const string Euronext = "Euronext";

        internal const string Cfi = "cfi";
        internal const string Compartment = "compartment";
        internal const string TradingMode = "tradingMode";
        internal const string Icb = "icb";
        internal const string Icb1 = "icb1";
        internal const string Icb2 = "icb2";
        internal const string Icb3 = "icb3";
        internal const string Icb4 = "icb4";
        internal const string Shares = "shares";
        internal const string Kind = "kind";
        internal const string Family = "family";
        internal const string CalcFreq = "calcFreq";
        internal const string BaseDate = "baseDate";
        internal const string BaseLevel = "baseLevel";
        internal const string Weighting = "weighting";
        internal const string CapFactor = "capFactor";
        internal const string Ter = "ter";
        internal const string LaunchDate = "launchDate";
        internal const string Issuer = "issuer";
        internal const string Fraction = "fraction";
        internal const string DividendFrequency = "dividendFrequency";
        internal const string IndexFamily = "indexFamily";
        internal const string ExpositionType = "expositionType";
        internal const string Underlying = "underlying";
        internal const string Target = "target";
        internal const string AllInFees = "allInFees";
        internal const string ExpenseRatio = "expenseRatio";

        internal const string Notes = "notes";
        internal const string FoundInSearch = "foundInSearch";
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore InconsistentNaming
        #endregion

        #region AttributeValue
        /// <summary>
        /// Returns the string value of the given attribute name or an empty string if the attribute does not exist.
        /// </summary>
        internal static string AttributeValue(this XElement xel, string attributeName)
        {
            XAttribute attribute = xel.Attribute(attributeName);
            if (null == attribute)
                return "";
            return attribute.Value;
        }

        /// <summary>
        /// Assigns the string value to the specified attribute name.
        /// The attribute will be created if it does not exist.
        /// Optionally issues a warning if an existing value differs from the specified value.
        /// </summary>
        internal static void AttributeValue(this XElement xel, string attributeName, string attributeValue,
            bool warnIfValuesAreDifferent = true)
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
                    if (warnIfValuesAreDifferent)
                        Trace.TraceWarning("Enrichment: attribute \"{0}\": replacing value: old \"{1}\" new \"{2}\", element [{3}]",
                            attributeName, attribute.Value, attributeValue, xel.ToString(SaveOptions.DisableFormatting));
                    attribute.Value = attributeValue;
                }
            }
        }
        #endregion

        #region Matches
        /// <summary>
        /// If this element matches the isin, the mic and the symbol of the specified instrument.
        /// </summary>
        internal static bool MatchesIsinMicSymbol(this XElement xel, EuronextActualInstruments.InstrumentInfo ii)
        {
            return xel.AttributeValue(Isin) == ii.Isin && xel.AttributeValue(Mic) == ii.Mic && xel.AttributeValue(Symbol) == ii.Symbol;
        }

        /// <summary>
        /// If this element matches the isin and the mic of the specified instrument.
        /// </summary>
        internal static bool MatchesIsinMic(this XElement xel, EuronextActualInstruments.InstrumentInfo ii)
        {
            return xel.AttributeValue(Isin) == ii.Isin && xel.AttributeValue(Mic) == ii.Mic;
        }

        /// <summary>
        /// If this element matches the isin of the specified instrument.
        /// </summary>
        internal static bool MatchesIsin(this XElement xel, EuronextActualInstruments.InstrumentInfo ii)
        {
            return xel.AttributeValue(Isin) == ii.Isin;
        }

        /// <summary>
        /// If this element matches the mic of the specified instrument.
        /// </summary>
        internal static bool MatchesMic(this XElement xel, EuronextActualInstruments.InstrumentInfo ii)
        {
            return xel.AttributeValue(Mic) == ii.Isin;
        }
        #endregion

        #region NormalizeElement
        internal static void NormalizeElement(this XElement xel, bool enrichSearch = true)
        {
            if (enrichSearch)
                xel.EnrichSearchInstrument();
            string type = xel.AttributeValue(Type);
            if ("" == type)
                return;
            switch (type)
            {
                case Stock:
                    xel.NormalizeStockElement();
                    break;
                case Index:
                    xel.NormalizeIndexElement();
                    break;
                case Etf:
                    xel.NormalizeEtfElement();
                    break;
                case Etv:
                    xel.NormalizeEtvElement();
                    break;
                case Inav:
                    xel.NormalizeInavElement();
                    break;
                case Fund:
                    xel.NormalizeFundElement();
                    break;
            }
        }
        #endregion

        #region NormalizeStockElement
        internal static void NormalizeStockElement(this XElement xel)
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

            XElement xelStock = xel.Element(Stock);
            if (null == xelStock)
            {
                xelStock = new XElement(Stock,
                    new XAttribute(Cfi, ""),
                    new XAttribute(Compartment, ""),
                    new XAttribute(TradingMode, ""),
                    new XAttribute(Currency, ""),
                    new XAttribute(Shares, ""));
                xel.Add(xelStock);
            }
            else
            {
                if (null == xelStock.Attribute(Cfi))
                    xelStock.Add(new XAttribute(Cfi, ""));
                if (null == xelStock.Attribute(Compartment))
                    xelStock.Add(new XAttribute(Compartment, ""));
                if (null == xelStock.Attribute(TradingMode))
                    xelStock.Add(new XAttribute(TradingMode, ""));
                if (null == xelStock.Attribute(Currency))
                    xelStock.Add(new XAttribute(Currency, ""));
                if (null == xelStock.Attribute(Shares))
                    xelStock.Add(new XAttribute(Shares, ""));
            }
            XElement xelIcb = xelStock.Element(Icb);
            if (null == xelIcb)
            {
                xelIcb = new XElement(Icb,
                    new XAttribute(Icb1, ""),
                    new XAttribute(Icb2, ""),
                    new XAttribute(Icb3, ""),
                    new XAttribute(Icb4, ""));
                xelStock.Add(xelIcb);
            }
            else
            {
                if (null == xelIcb.Attribute(Icb1))
                    xelIcb.Add(new XAttribute(Icb1, ""));
                if (null == xelIcb.Attribute(Icb2))
                    xelIcb.Add(new XAttribute(Icb2, ""));
                if (null == xelIcb.Attribute(Icb3))
                    xelIcb.Add(new XAttribute(Icb3, ""));
                if (null == xelIcb.Attribute(Icb4))
                    xelIcb.Add(new XAttribute(Icb4, ""));
            }
            xel.FixInstrumentElementCurrency(xelStock);
        }
        #endregion

        #region NormalizeIndexElement
        internal static void NormalizeIndexElement(this XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" isin="NL0000000107" symbol="AEX" name="AEX-INDEX" type="index" mic="XAMS"
            //     file="euronext/ams/indices/AEX.xml"
            //     description="The best-known index of Euronext Amsterdam, the AEX (Price) index ... calender year."
            //     >
            //     <index kind="price" family="AEX" calcFreq="15s" baseDate="1983-01-03" baseLevel="45.378" weighting="float market cap" capFactor="0.15" currency="EUR"/>
            // </instrument>

            XElement xelIndex = xel.Element(Index);
            if (null == xelIndex)
            {
                xelIndex = new XElement(Index,
                    new XAttribute(Kind, ""),
                    new XAttribute(Family, ""),
                    new XAttribute(CalcFreq, ""),
                    new XAttribute(BaseDate, ""),
                    new XAttribute(BaseLevel, ""),
                    new XAttribute(Weighting, ""),
                    new XAttribute(CapFactor, ""),
                    new XAttribute(Currency, ""));
                xel.Add(xelIndex);
            }
            else
            {
                if (null == xelIndex.Attribute(Kind))
                    xelIndex.Add(new XAttribute(Kind, ""));
                if (null == xelIndex.Attribute(Family))
                    xelIndex.Add(new XAttribute(Family, ""));
                if (null == xelIndex.Attribute(CalcFreq))
                    xelIndex.Add(new XAttribute(CalcFreq, ""));
                if (null == xelIndex.Attribute(BaseDate))
                    xelIndex.Add(new XAttribute(BaseDate, ""));
                if (null == xelIndex.Attribute(BaseLevel))
                    xelIndex.Add(new XAttribute(BaseLevel, ""));
                if (null == xelIndex.Attribute(Weighting))
                    xelIndex.Add(new XAttribute(Weighting, ""));
                if (null == xelIndex.Attribute(CapFactor))
                    xelIndex.Add(new XAttribute(CapFactor, ""));
                if (null == xelIndex.Attribute(Currency))
                    xelIndex.Add(new XAttribute(Currency, ""));
            }
            xel.FixInstrumentElementCurrency(xelIndex);
        }
        #endregion

        #region NormalizeEtfElement
        internal static void NormalizeEtfElement(this XElement xel)
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

            XElement xelEtf = xel.Element(Etf);
            if (null == xelEtf)
            {
                xelEtf = new XElement(Etf,
                    new XAttribute(Cfi, ""),
                    new XAttribute(TradingMode, ""),
                    new XAttribute(Ter, ""),
                    new XAttribute(LaunchDate, ""),
                    new XAttribute(Issuer, ""),
                    new XAttribute(Fraction, ""),
                    new XAttribute(DividendFrequency, ""),
                    new XAttribute(IndexFamily, ""),
                    new XAttribute(ExpositionType, ""),
                    new XAttribute(Currency, ""));
                xel.Add(xelEtf);
            }
            else
            {
                if (null == xelEtf.Attribute(Cfi))
                    xelEtf.Add(new XAttribute(Cfi, ""));
                if (null == xelEtf.Attribute(TradingMode))
                    xelEtf.Add(new XAttribute(TradingMode, ""));
                if (null == xelEtf.Attribute(Ter))
                    xelEtf.Add(new XAttribute(Ter, ""));
                if (null == xelEtf.Attribute(LaunchDate))
                    xelEtf.Add(new XAttribute(LaunchDate, ""));
                if (null == xelEtf.Attribute(Issuer))
                    xelEtf.Add(new XAttribute(Issuer, ""));
                if (null == xelEtf.Attribute(Fraction))
                    xelEtf.Add(new XAttribute(Fraction, ""));
                if (null == xelEtf.Attribute(DividendFrequency))
                    xelEtf.Add(new XAttribute(DividendFrequency, ""));
                if (null == xelEtf.Attribute(IndexFamily))
                    xelEtf.Add(new XAttribute(IndexFamily, ""));
                if (null == xelEtf.Attribute(ExpositionType))
                    xelEtf.Add(new XAttribute(ExpositionType, ""));
                if (null == xelEtf.Attribute(Currency))
                    xelEtf.Add(new XAttribute(Currency, ""));
            }
            xel.FixInstrumentElementCurrency(xelEtf);
            XAttribute xat = xel.Attribute("mer");
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelEtf.Attribute(Ter);
                if (xat != null && 0 == xat.Value.Length)
                    xat.Value = value;
            }
            XElement xelInav = xelEtf.Element(Inav);
            if (null == xelInav)
            {
                xelInav = new XElement(Inav,
                    new XAttribute(Vendor, Euronext),
                    new XAttribute(Mep, ""),
                    new XAttribute(Mic, ""),
                    new XAttribute(Isin, ""),
                    new XAttribute(Symbol, ""),
                    new XAttribute(Name, ""));
                xelEtf.Add(xelInav);
            }
            else
            {
                if (null == xelInav.Attribute(Vendor))
                    xelInav.Add(new XAttribute(Vendor, Euronext));
                if (null == xelInav.Attribute(Mep))
                    xelInav.Add(new XAttribute(Mep, ""));
                if (null == xelInav.Attribute(Mic))
                    xelInav.Add(new XAttribute(Mic, ""));
                if (null == xelInav.Attribute(Isin))
                    xelInav.Add(new XAttribute(Isin, ""));
                if (null == xelInav.Attribute(Symbol))
                    xelInav.Add(new XAttribute(Symbol, ""));
                if (null == xelInav.Attribute(Name))
                    xelInav.Add(new XAttribute(Name, ""));
            }
            XElement xelUnderlying = xelEtf.Element(Underlying);
            if (null == xelUnderlying)
            {
                xelUnderlying = new XElement(Underlying,
                    new XAttribute(Vendor, Euronext),
                    new XAttribute(Mep, ""),
                    new XAttribute(Mic, ""),
                    new XAttribute(Isin, ""),
                    new XAttribute(Symbol, ""),
                    new XAttribute(Name, ""));
                xelEtf.Add(xelUnderlying);
            }
            else
            {
                if (null == xelUnderlying.Attribute(Vendor))
                    xelUnderlying.Add(new XAttribute(Vendor, Euronext));
                if (null == xelUnderlying.Attribute(Mep))
                    xelUnderlying.Add(new XAttribute(Mep, ""));
                if (null == xelUnderlying.Attribute(Mic))
                    xelUnderlying.Add(new XAttribute(Mic, ""));
                if (null == xelUnderlying.Attribute(Isin))
                    xelUnderlying.Add(new XAttribute(Isin, ""));
                if (null == xelUnderlying.Attribute(Symbol))
                    xelUnderlying.Add(new XAttribute(Symbol, ""));
                if (null == xelUnderlying.Attribute(Name))
                    xelUnderlying.Add(new XAttribute(Name, ""));
            }
        }
        #endregion

        #region InavElementFromEtf
        internal static XElement InavElementFromEtf(this XElement xel)
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

            XElement xelEtf = xel.Element(Etf);
            if (null == xelEtf)
                return null;
            XElement xelInav = xelEtf.Element(Inav);
            if (null == xelInav)
                return null;

            string inavVendor = xelInav.AttributeValue(Vendor);
            string inavMep = xelInav.AttributeValue(Mep);
            string inavMic = xelInav.AttributeValue(Mic);
            string inavIsin = xelInav.AttributeValue(Isin);
            string inavSymbol = xelInav.AttributeValue(Symbol);
            string inavName = xelInav.AttributeValue(Name);
            string inavCurrency = xelInav.AttributeValue(Currency);

            string targetVendor = xel.AttributeValue(Vendor);
            string targetMep = xel.AttributeValue(Mep);
            string targetMic = xel.AttributeValue(Mic);
            string targetIsin = xel.AttributeValue(Isin);
            string targetSymbol = xel.AttributeValue(Symbol);
            string targetName = xel.AttributeValue(Name);
            string targetDescription = xel.AttributeValue(Description);

            if (string.IsNullOrEmpty(inavMic) || string.IsNullOrEmpty(inavIsin))
                return null;
            string inavFile = "inav/" + (string.IsNullOrEmpty(inavSymbol) ? inavIsin : inavSymbol);
            string inavDescription = "";
            if (!string.IsNullOrEmpty(targetDescription))
                inavDescription = "iNav " + targetDescription;

            var xelNew = new XElement(Instrument,
                new XAttribute(File, inavFile),
                new XAttribute(Mep, inavMep),
                new XAttribute(Mic, inavMic),
                new XAttribute(Isin, inavIsin),
                // ReSharper disable AssignNullToNotNullAttribute
                new XAttribute(Symbol, inavSymbol),
                // ReSharper restore AssignNullToNotNullAttribute
                new XAttribute(Name, inavName),
                new XAttribute(Type, Inav),
                new XAttribute(Description, inavDescription),
                new XAttribute(Vendor, inavVendor),
                new XElement(Inav,
                    new XAttribute(Currency, inavCurrency),
                    new XElement(Target,
                        new XAttribute(Vendor, targetVendor),
                        new XAttribute(Mep, targetMep),
                        new XAttribute(Mic, targetMic),
                        new XAttribute(Isin, targetIsin),
                        new XAttribute(Symbol, targetSymbol),
                        new XAttribute(Name, targetName)
                )));
            return xelNew;
        }
        #endregion

        #region NormalizeInavElement
        internal static void NormalizeInavElement(this XElement xel)
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

            XElement xelInav = xel.Element(Inav);
            if (null == xelInav)
            {
                xelInav = new XElement(Inav, new XAttribute(Currency, ""));
                xel.Add(xelInav);
            }
            else
            {
                if (null == xelInav.Attribute(Currency))
                    xelInav.Add(new XAttribute(Currency, ""));
            }
            xel.FixInstrumentElementCurrency(xelInav);
            XElement xelTarget = xelInav.Element(Target);
            if (null == xelTarget)
            {
                xelTarget = new XElement(Target,
                    new XAttribute(Vendor, Euronext),
                    new XAttribute(Mep, ""),
                    new XAttribute(Mic, ""),
                    new XAttribute(Isin, ""),
                    new XAttribute(Symbol, ""),
                    new XAttribute(Name, ""));
                xelInav.Add(xelTarget);
            }
            else
            {
                if (null == xelTarget.Attribute(Vendor))
                    xelTarget.Add(new XAttribute(Vendor, Euronext));
                if (null == xelTarget.Attribute(Mep))
                    xelTarget.Add(new XAttribute(Mep, ""));
                if (null == xelTarget.Attribute(Mic))
                    xelTarget.Add(new XAttribute(Mic, ""));
                if (null == xelTarget.Attribute(Isin))
                    xelTarget.Add(new XAttribute(Isin, ""));
                if (null == xelTarget.Attribute(Symbol))
                    xelTarget.Add(new XAttribute(Symbol, ""));
                if (null == xelTarget.Attribute(Name))
                    xelTarget.Add(new XAttribute(Name, ""));
            }
            xel.FixInstrumentElementCurrency(xelInav);
        }
        #endregion

        #region NormalizeEtvElement
        internal static void NormalizeEtvElement(this XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="PAR" mic="XPAR" isin="GB00B15KXP72" symbol="COFFP" name="ETFS COFFEE" type="etv"
            //     file="etf/COFFP.xml"
            //     description=""
            //     >
            //     <etv cfi="DTZSPR" tradingMode="continuous" allInFees="0,49%" expenseRatio="" dividendFrequency="yearly" currency="EUR" issuer="ETFS COMMODITY SECURITIES LTD" shares="944,000">
            // </instrument>

            xel.AttributeValue(Type, Etv);
            string fileOld = xel.AttributeValue(File);
            if (fileOld.Contains("etf"))
            {
                Trace.TraceInformation("ETV: has ETF file [{0}] in [{1}]", fileOld, xel.ToString(SaveOptions.None));
            }
            /*if (fileOld.Contains("/etf/commodities/"))
            {
                string fileNew = fileOld.Replace("/etf/commodities/", "/etv/");
                Trace.TraceInformation("ETV: replacing file [{0}] with {{1}] in element [{3}]", fileOld, fileNew, xel.ToString(SaveOptions.None));
                AttributeValue(xel, file, fileNew);
            }*/

            XElement xelEtv = xel.Element(Etv);
            if (null == xelEtv)
            {
                xelEtv = new XElement(Etv,
                    new XAttribute(Cfi, ""),
                    new XAttribute(TradingMode, ""),
                    new XAttribute(AllInFees, ""),
                    new XAttribute(ExpenseRatio, ""),
                    new XAttribute(DividendFrequency, ""),
                    new XAttribute(Currency, ""),
                    new XAttribute(Issuer, ""),
                    new XAttribute(Shares, ""));
                xel.Add(xelEtv);
            }
            else
            {
                if (null == xelEtv.Attribute(Cfi))
                    xelEtv.Add(new XAttribute(Cfi, ""));
                if (null == xelEtv.Attribute(TradingMode))
                    xelEtv.Add(new XAttribute(TradingMode, ""));
                if (null == xelEtv.Attribute(AllInFees))
                    xelEtv.Add(new XAttribute(AllInFees, ""));
                if (null == xelEtv.Attribute(ExpenseRatio))
                    xelEtv.Add(new XAttribute(ExpenseRatio, ""));
                if (null == xelEtv.Attribute(DividendFrequency))
                    xelEtv.Add(new XAttribute(DividendFrequency, ""));
                if (null == xelEtv.Attribute(Currency))
                    xelEtv.Add(new XAttribute(Currency, ""));
                if (null == xelEtv.Attribute(Issuer))
                    xelEtv.Add(new XAttribute(Issuer, ""));
                if (null == xelEtv.Attribute(Shares))
                    xelEtv.Add(new XAttribute(Shares, ""));
            }
            xel.FixInstrumentElementCurrency(xelEtv);
        }
        #endregion

        #region NormalizeFundElement
        internal static void NormalizeFundElement(this XElement xel)
        {
            // <instrument vendor="Euronext"
            //     mep="AMS" mic="XAMS" isin="NL0006259996" symbol="AWAF" name="ACH WERELD AANDFD3" type="fund"
            //     file="fund/AWAF.xml"
            //     description=""
            //     >
            //     <fund cfi="EUOISB" tradingmode="fixing" currency="EUR" issuer="ACHMEA BELEGGINGSFONDSEN" shares="860,248">
            // </instrument>

            xel.AttributeValue(Type, Fund);

            XElement xelFund = xel.Element(Fund);
            if (null == xelFund)
            {
                xelFund = new XElement(Fund,
                    new XAttribute(Cfi, ""),
                    new XAttribute(TradingMode, ""),
                    new XAttribute(Currency, ""),
                    new XAttribute(Issuer, ""),
                    new XAttribute(Shares, ""));
                xel.Add(xelFund);
            }
            else
            {
                if (null == xelFund.Attribute(Cfi))
                    xelFund.Add(new XAttribute(Cfi, ""));
                if (null == xelFund.Attribute(TradingMode))
                    xelFund.Add(new XAttribute(TradingMode, ""));
                if (null == xelFund.Attribute(Currency))
                    xelFund.Add(new XAttribute(Currency, ""));
                if (null == xelFund.Attribute(Issuer))
                    xelFund.Add(new XAttribute(Issuer, ""));
                if (null == xelFund.Attribute(Shares))
                    xelFund.Add(new XAttribute(Shares, ""));
            }
            xel.FixInstrumentElementCurrency(xelFund);
        }
        #endregion

        #region FixInstrumentElementCurrency
        private static void FixInstrumentElementCurrency(this XElement xel, XElement xelInner)
        {
            XAttribute xat = xel.Attribute(Currency);
            if (null != xat)
            {
                string value = xat.Value;
                xat.Remove();
                xat = xelInner.Attribute(Currency);
                if (xat != null && 0 == xat.Value.Length)
                    xat.Value = value;
            }
        }
        #endregion

        #region BackupXmlFile
        internal static void BackupXmlFile(string filePath, DateTime dateTime)
        {
            string suffix = dateTime.ToString("yyyyMMdd_HHmmss");
            string filePathBackup = string.Concat(filePath, ".", suffix, ".xml");
            System.IO.File.Copy(filePath, filePathBackup);
        }
        #endregion
    }
}
