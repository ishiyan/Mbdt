using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EuronextInstrumentIndexConverter
{
    #region Fixer
    public static class Fixer
    {
        public static string Fix(this string source)
        {
            return null == source ? null : source.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
    #endregion

    #region ConvertedInstrument
    public class ConvertedInstrument
    {
        #region Properties
        #region Root
        public string Vendor { get; set; }
        public string Mep { get; set; }
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string File { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int MepInteger
        {
            get
            {
                if ("AMS".Equals(Mep))
                    return 2;
                else if ("PAR".Equals(Mep))
                    return 1;
                else if ("BRU".Equals(Mep))
                    return 3;
                else if ("LIS".Equals(Mep))
                    return 5;
                else
                    return 6; // other
            }
        }
        public string FinderHeadline
        {
            get
            {
                return string.Format("*** {0}_{1}_{2}_{3}_{4}_{5} ***", Vendor, Mep, Isin, Symbol, Name, Type);
            }
        }
        public string EuronextIsinSearch
        {
            get
            {
                if ("index" == Type || "inav" == Type)
                    return string.Format("http://www.euronext.com/quicksearch/resultquicksearchindices.jsp?lan=EN&matchpattern={0}&cha=1921", Isin);
                else if ("stock" == Type)
                    //return string.Format("http://www.euronext.com/quicksearch/resultquicksearch.jsp?lan=EN&matchpattern={0}&cha=1921", Isin);
                    return string.Format("http://www.euronext.com/trader/companyprofile/companyprofilev2-18661-EN-{0}.html?selectedMep={1}", Isin, MepInteger);
                else if ("etf" == Type)
                    return string.Format("http://www.euronext.com/quicksearch/resultquicksearchtrackers.jsp?lan=EN&matchpattern={0}&cha=1921", Isin);
                else if ("etv" == Type)
                    return string.Format("http://www.euronext.com/quicksearch/resultquicksearchETC-6400-EN.html?matchpattern={0}", Isin);
                else if ("fund" == Type)
                    return string.Format("http://www.euronext.com/quicksearch/resultquicksearchfunds-6750-EN.html?matchpattern={0}", Isin);
                else
                    return string.Format("http://www.euronext.com/quicksearch/resultquicksearch.jsp?lan=EN&matchpattern={0}&cha=1921", Isin);
            }
        }
        #endregion

        #region Index
        public string IndexKind { get { return Index.Kind; } set { Index.Kind = value; } }
        public string IndexCalcFreq { get { return Index.CalcFreq; } set { Index.CalcFreq = value; } }
        public string IndexBaseDate { get { return Index.BaseDate; } set { Index.BaseDate = value; } }
        public string IndexBaseLevel { get { return Index.BaseLevel; } set { Index.BaseLevel = value; } }
        public string IndexWeighting { get { return Index.Weighting; } set { Index.Weighting = value; } }
        public string IndexCapFactor { get { return Index.CapFactor; } set { Index.CapFactor = value; } }
        public string IndexFamily { get { return Index.Family; } set { Index.Family = value; } }
        public string IndexBaseCap { get { return Index.BaseCap; } set { Index.BaseCap = value; } }
        public string IndexBaseCapCurrency { get { return Index.BaseCapCurrency; } set { Index.BaseCapCurrency = value; } }
        public string IndexIcb1 { get { return Index.Icb.Icb1; } set { Index.Icb.Icb1 = value; } }
        public string IndexIcb2 { get { return Index.Icb.Icb2; } set { Index.Icb.Icb2 = value; } }
        public string IndexIcb3 { get { return Index.Icb.Icb3; } set { Index.Icb.Icb3 = value; } }
        public string IndexIcb4 { get { return Index.Icb.Icb4; } set { Index.Icb.Icb4 = value; } }
        // Not used: ConstituentList, DerivativeList
        #endregion

        #region Stock
        public string StockCfi { get { return Stock.Cfi; } set { Stock.Cfi = value; } }
        public string StockTradingMode { get { return Stock.TradingMode; } set { Stock.TradingMode = value; } }
        public string StockCompartment { get { return Stock.Compartment; } set { Stock.Compartment = value; } }
        public string StockCurrency { get { return Stock.Currency; } set { Stock.Currency = value; } }
        public string StockIcb1 { get { return Stock.Icb.Icb1; } set { Stock.Icb.Icb1 = value; } }
        public string StockIcb2 { get { return Stock.Icb.Icb2; } set { Stock.Icb.Icb2 = value; } }
        public string StockIcb3 { get { return Stock.Icb.Icb3; } set { Stock.Icb.Icb3 = value; } }
        public string StockIcb4 { get { return Stock.Icb.Icb4; } set { Stock.Icb.Icb4 = value; } }
        // Not used: IndexList, DerivativeList
        #endregion

        #region Inav
        public string InavCurrency { get { return Inav.Currency; } set { Inav.Currency = value; } }
        public string InavIcb1 { get { return Inav.Icb.Icb1; } set { Inav.Icb.Icb1 = value; } }
        public string InavIcb2 { get { return Inav.Icb.Icb2; } set { Inav.Icb.Icb2 = value; } }
        public string InavIcb3 { get { return Inav.Icb.Icb3; } set { Inav.Icb.Icb3 = value; } }
        public string InavIcb4 { get { return Inav.Icb.Icb4; } set { Inav.Icb.Icb4 = value; } }
        public string InavTarget0Vendor { get { return Inav.TargetList[0].Vendor; } set { Inav.TargetList[0].Vendor = value; } }
        public string InavTarget0Mep { get { return Inav.TargetList[0].Mep; } set { Inav.TargetList[0].Mep = value; } }
        public string InavTarget0Isin { get { return Inav.TargetList[0].Isin; } set { Inav.TargetList[0].Isin = value; } }
        public string InavTarget0Symbol { get { return Inav.TargetList[0].Symbol; } set { Inav.TargetList[0].Symbol = value; } }
        public string InavTarget0Name { get { return Inav.TargetList[0].Name; } set { Inav.TargetList[0].Name = value; } }
        public string InavTarget1Vendor { get { return Inav.TargetList[1].Vendor; } set { Inav.TargetList[1].Vendor = value; } }
        public string InavTarget1Mep { get { return Inav.TargetList[1].Mep; } set { Inav.TargetList[1].Mep = value; } }
        public string InavTarget1Isin { get { return Inav.TargetList[1].Isin; } set { Inav.TargetList[1].Isin = value; } }
        public string InavTarget1Symbol { get { return Inav.TargetList[1].Symbol; } set { Inav.TargetList[1].Symbol = value; } }
        public string InavTarget1Name { get { return Inav.TargetList[1].Name; } set { Inav.TargetList[1].Name = value; } }
        // We use only two first entries of TargetList
        #endregion

        #region Etf
        public string EtfCfi { get { return Etf.Cfi; } set { Etf.Cfi = value; } }
        public string EtfMer { get { return Etf.Mer; } set { Etf.Mer = value; } }
        public string EtfLaunchDate { get { return Etf.LaunchDate; } set { Etf.LaunchDate = value; } }
        public string EtfCurrency { get { return Etf.Currency; } set { Etf.Currency = value; } }
        public string EtfIssuer { get { return Etf.Issuer; } set { Etf.Issuer = value; } }
        public string EtfFraction { get { return Etf.Fraction; } set { Etf.Fraction = value; } }
        public string EtfDividendFrequency { get { return Etf.DividendFrequency; } set { Etf.DividendFrequency = value; } }
        public string EtfIndexFamily { get { return Etf.IndexFamily; } set { Etf.IndexFamily = value; } }
        public string EtfSeg1 { get { return Etf.Seg1; } set { Etf.Seg1 = value; } }
        public string EtfSeg2 { get { return Etf.Seg2; } set { Etf.Seg2 = value; } }
        public string EtfSeg3 { get { return Etf.Seg3; } set { Etf.Seg3 = value; } }
        public string EtfIcb1 { get { return Etf.Icb.Icb1; } set { Etf.Icb.Icb1 = value; } }
        public string EtfIcb2 { get { return Etf.Icb.Icb2; } set { Etf.Icb.Icb2 = value; } }
        public string EtfIcb3 { get { return Etf.Icb.Icb3; } set { Etf.Icb.Icb3 = value; } }
        public string EtfIcb4 { get { return Etf.Icb.Icb4; } set { Etf.Icb.Icb4 = value; } }
        public string EtfInavVendor { get { return Etf.Inav.Vendor; } set { Etf.Inav.Vendor = value; } }
        public string EtfInavMep { get { return Etf.Inav.Mep; } set { Etf.Inav.Mep = value; } }
        public string EtfInavIsin { get { return Etf.Inav.Isin; } set { Etf.Inav.Isin = value; } }
        public string EtfInavSymbol { get { return Etf.Inav.Symbol; } set { Etf.Inav.Symbol = value; } }
        public string EtfInavName { get { return Etf.Inav.Name; } set { Etf.Inav.Name = value; } }
        public string EtfUnderlyingVendor { get { return Etf.Inav.Vendor; } set { Etf.Underlying.Vendor = value; } }
        public string EtfUnderlyingMep { get { return Etf.Underlying.Mep; } set { Etf.Underlying.Mep = value; } }
        public string EtfUnderlyingIsin { get { return Etf.Underlying.Isin; } set { Etf.Underlying.Isin = value; } }
        public string EtfUnderlyingSymbol { get { return Etf.Underlying.Symbol; } set { Etf.Underlying.Symbol = value; } }
        public string EtfUnderlyingName { get { return Etf.Underlying.Name; } set { Etf.Underlying.Name = value; } }
        #endregion

        #region Etv
        public string EtvCfi { get { return Etv.Cfi; } set { Etv.Cfi = value; } }
        public string EtvMer { get { return Etv.Mer; } set { Etv.Mer = value; } }
        public string EtvLaunchDate { get { return Etv.LaunchDate; } set { Etv.LaunchDate = value; } }
        public string EtvCurrency { get { return Etv.Currency; } set { Etv.Currency = value; } }
        public string EtvIssuer { get { return Etv.Issuer; } set { Etv.Issuer = value; } }
        public string EtvFraction { get { return Etv.Fraction; } set { Etv.Fraction = value; } }
        public string EtvDividendFrequency { get { return Etv.DividendFrequency; } set { Etv.DividendFrequency = value; } }
        public string EtvIndexFamily { get { return Etv.IndexFamily; } set { Etv.IndexFamily = value; } }
        public string EtvSeg1 { get { return Etv.Seg1; } set { Etv.Seg1 = value; } }
        public string EtvSeg2 { get { return Etv.Seg2; } set { Etv.Seg2 = value; } }
        public string EtvSeg3 { get { return Etv.Seg3; } set { Etv.Seg3 = value; } }
        public string EtvIcb1 { get { return Etv.Icb.Icb1; } set { Etv.Icb.Icb1 = value; } }
        public string EtvIcb2 { get { return Etv.Icb.Icb2; } set { Etv.Icb.Icb2 = value; } }
        public string EtvIcb3 { get { return Etv.Icb.Icb3; } set { Etv.Icb.Icb3 = value; } }
        public string EtvIcb4 { get { return Etv.Icb.Icb4; } set { Etv.Icb.Icb4 = value; } }
        public string EtvInavVendor { get { return Etv.Inav.Vendor; } set { Etv.Inav.Vendor = value; } }
        public string EtvInavMep { get { return Etv.Inav.Mep; } set { Etv.Inav.Mep = value; } }
        public string EtvInavIsin { get { return Etv.Inav.Isin; } set { Etv.Inav.Isin = value; } }
        public string EtvInavSymbol { get { return Etv.Inav.Symbol; } set { Etv.Inav.Symbol = value; } }
        public string EtvInavName { get { return Etv.Inav.Name; } set { Etv.Inav.Name = value; } }
        public string EtvUnderlyingVendor { get { return Etv.Inav.Vendor; } set { Etv.Underlying.Vendor = value; } }
        public string EtvUnderlyingMep { get { return Etv.Underlying.Mep; } set { Etv.Underlying.Mep = value; } }
        public string EtvUnderlyingIsin { get { return Etv.Underlying.Isin; } set { Etv.Underlying.Isin = value; } }
        public string EtvUnderlyingSymbol { get { return Etv.Underlying.Symbol; } set { Etv.Underlying.Symbol = value; } }
        public string EtvUnderlyingName { get { return Etv.Underlying.Name; } set { Etv.Underlying.Name = value; } }
        #endregion

        #region Fund
        public string FundCfi { get { return Fund.Cfi; } set { Fund.Cfi = value; } }
        public string FundTradingMode { get { return Fund.TradingMode; } set { Fund.TradingMode = value; } }
        public string FundCurrency { get { return Fund.Currency; } set { Fund.Currency = value; } }
        public string FundIcb1 { get { return Fund.Icb.Icb1; } set { Fund.Icb.Icb1 = value; } }
        public string FundIcb2 { get { return Fund.Icb.Icb2; } set { Fund.Icb.Icb2 = value; } }
        public string FundIcb3 { get { return Fund.Icb.Icb3; } set { Fund.Icb.Icb3 = value; } }
        public string FundIcb4 { get { return Fund.Icb.Icb4; } set { Fund.Icb.Icb4 = value; } }
        // Not used: DerivativeList
        #endregion
        #endregion

        #region Elements
        public Index Index = new Index();
        public Stock Stock = new Stock();
        public Inav Inav = new Inav();
        public Etf Etf = new Etf();
        public Etv Etv = new Etv();
        public Fund Fund = new Fund();
        #endregion

        #region Construction
        public ConvertedInstrument()
        {
        }

        public ConvertedInstrument(OriginalInstrument source)
        {
            string value;
            if (source.Dictionary.TryGetValue("vendor", out value))
                Vendor = value;
            else
                Vendor = "Euronext";
            if (source.Dictionary.TryGetValue("mep", out value))
                Mep = value;
            if (source.Dictionary.TryGetValue("isin", out value))
                Isin = value;
            if (source.Dictionary.TryGetValue("symbol", out value))
                Symbol = value;
            if (source.Dictionary.TryGetValue("name", out value))
                Name = value;
            if (source.Dictionary.TryGetValue("state", out value))
                State = value;
            if (source.Dictionary.TryGetValue("type", out value))
                Type = value;
            if (source.Dictionary.TryGetValue("file", out value))
                File = value;
            if (source.Dictionary.TryGetValue("description", out value))
                Description = value;
            if (source.Dictionary.TryGetValue("notes", out value))
                Notes = value;
            if (source.Dictionary.TryGetValue("currency", out value))
            {
                Stock.Currency = value;
                Inav.Currency = value;
                Etf.Currency = value;
                Etv.Currency = value;
                Fund.Currency = value;
            }
            if (source.Dictionary.TryGetValue("underlyingSymbol", out value))
            {
                Etf.Underlying.Symbol = value;
                Etv.Underlying.Symbol = value;
                if (string.IsNullOrEmpty(Etf.Underlying.Vendor))
                    Etf.Underlying.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Underlying.Vendor))
                    Etv.Underlying.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("underlyingName", out value))
            {
                Etf.Underlying.Name = value;
                Etv.Underlying.Name = value;
                if (string.IsNullOrEmpty(Etf.Underlying.Vendor))
                    Etf.Underlying.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Underlying.Vendor))
                    Etv.Underlying.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("inavSymbol", out value))
            {
                Etf.Inav.Symbol = value;
                Etv.Inav.Symbol = value;
                if (string.IsNullOrEmpty(Etf.Inav.Vendor))
                    Etf.Inav.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Inav.Vendor))
                    Etv.Inav.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("inavName", out value))
            {
                Etf.Inav.Name = value;
                Etv.Inav.Name = value;
                if (string.IsNullOrEmpty(Etf.Inav.Vendor))
                    Etf.Inav.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Inav.Vendor))
                    Etv.Inav.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("inavMep", out value))
            {
                Etf.Inav.Mep = value;
                Etv.Inav.Mep = value;
                if (string.IsNullOrEmpty(Etf.Inav.Vendor))
                    Etf.Inav.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Inav.Vendor))
                    Etv.Inav.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("inavIsin", out value))
            {
                Etf.Inav.Isin = value;
                Etv.Inav.Isin = value;
                if (string.IsNullOrEmpty(Etf.Inav.Vendor))
                    Etf.Inav.Vendor = "Euronext";
                if (string.IsNullOrEmpty(Etv.Inav.Vendor))
                    Etv.Inav.Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("etfSymbol", out value))
            {
                Inav.TargetList[0].Symbol = value;
                Inav.TargetList[0].Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("etfName", out value))
            {
                Inav.TargetList[0].Name = value;
                Inav.TargetList[0].Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("etfMep", out value))
            {
                Inav.TargetList[0].Mep = value;
                Inav.TargetList[0].Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("etfIsin", out value))
            {
                Inav.TargetList[0].Isin = value;
                Inav.TargetList[0].Vendor = "Euronext";
            }
            if (source.Dictionary.TryGetValue("cfi", out value))
            {
                Stock.Cfi = value;
                Etf.Cfi = value;
                Etv.Cfi = value;
                Fund.Cfi = value;
            }
            if (source.Dictionary.TryGetValue("compartment", out value))
            {
                Stock.Compartment = value;
            }
            if (source.Dictionary.TryGetValue("icb1", out value))
            {
                Index.Icb.Icb1 = value;
                Stock.Icb.Icb1 = value;
                Inav.Icb.Icb1 = value;
                Etf.Icb.Icb1 = value;
                Etv.Icb.Icb1 = value;
                Fund.Icb.Icb1 = value;
            }
            if (source.Dictionary.TryGetValue("icb2", out value))
            {
                Index.Icb.Icb2 = value;
                Stock.Icb.Icb2 = value;
                Inav.Icb.Icb2 = value;
                Etf.Icb.Icb2 = value;
                Etv.Icb.Icb2 = value;
                Fund.Icb.Icb2 = value;
            }
            if (source.Dictionary.TryGetValue("icb3", out value))
            {
                Index.Icb.Icb3 = value;
                Stock.Icb.Icb3 = value;
                Inav.Icb.Icb3 = value;
                Etf.Icb.Icb3 = value;
                Etv.Icb.Icb3 = value;
                Fund.Icb.Icb3 = value;
            }
            if (source.Dictionary.TryGetValue("icb4", out value))
            {
                Index.Icb.Icb4 = value;
                Stock.Icb.Icb4 = value;
                Inav.Icb.Icb4 = value;
                Etf.Icb.Icb4 = value;
                Etv.Icb.Icb4 = value;
                Fund.Icb.Icb4 = value;
            }
            if (source.Dictionary.TryGetValue("tradingmode", out value))
            {
                Stock.TradingMode = value;
                Fund.TradingMode = value;
            }
            // Index
            if (source.Dictionary.TryGetValue("index.kind", out value))
                IndexKind = value;
            if (source.Dictionary.TryGetValue("index.calcFreq", out value))
                IndexCalcFreq = value;
            if (source.Dictionary.TryGetValue("index.baseDate", out value))
                IndexBaseDate = value;
            if (source.Dictionary.TryGetValue("index.baseLevel", out value))
                IndexBaseLevel = value;
            if (source.Dictionary.TryGetValue("index.weighting", out value))
                IndexWeighting = value;
            if (source.Dictionary.TryGetValue("index.capFactor", out value))
                IndexCapFactor = value;
            if (source.Dictionary.TryGetValue("index.family", out value))
                IndexFamily = value;
            if (source.Dictionary.TryGetValue("index.baseCap", out value))
                IndexBaseCap = value;
            if (source.Dictionary.TryGetValue("index.baseCapCurrency", out value))
                IndexBaseCapCurrency = value;
            if (source.Dictionary.TryGetValue("index.icb1", out value))
                IndexIcb1 = value;
            if (source.Dictionary.TryGetValue("index.icb2", out value))
                IndexIcb2 = value;
            if (source.Dictionary.TryGetValue("index.icb3", out value))
                IndexIcb3 = value;
            if (source.Dictionary.TryGetValue("index.icb4", out value))
                IndexIcb4 = value;
            // Stock
            if (source.Dictionary.TryGetValue("stock.cfi", out value))
                StockCfi = value;
            if (source.Dictionary.TryGetValue("stock.tradingMode", out value))
                StockTradingMode = value;
            if (source.Dictionary.TryGetValue("stock.compartment", out value))
                StockCompartment = value;
            if (source.Dictionary.TryGetValue("stock.currency", out value))
                StockCurrency = value;
            if (source.Dictionary.TryGetValue("stock.icb1", out value))
                StockIcb1 = value;
            if (source.Dictionary.TryGetValue("stock.icb2", out value))
                StockIcb2 = value;
            if (source.Dictionary.TryGetValue("stock.icb3", out value))
                StockIcb3 = value;
            if (source.Dictionary.TryGetValue("stock.icb4", out value))
                StockIcb4 = value;
            // Etf
            if (source.Dictionary.TryGetValue("etf.cfi", out value))
                EtfCfi = value;
            if (source.Dictionary.TryGetValue("etf.mer", out value))
                EtfMer = value;
            if (source.Dictionary.TryGetValue("etf.launchDate", out value))
                EtfLaunchDate = value;
            if (source.Dictionary.TryGetValue("etf.currency", out value))
                EtfCurrency = value;
            if (source.Dictionary.TryGetValue("etf.issuer", out value))
                EtfIssuer = value;
            if (source.Dictionary.TryGetValue("etf.fraction", out value))
                EtfFraction = value;
            if (source.Dictionary.TryGetValue("etf.dividendFrequency", out value))
                EtfDividendFrequency = value;
            if (source.Dictionary.TryGetValue("etf.indexFamily", out value))
                EtfIndexFamily = value;
            if (source.Dictionary.TryGetValue("etf.seg1", out value))
                EtfSeg1 = value;
            if (source.Dictionary.TryGetValue("etf.seg2", out value))
                EtfSeg2 = value;
            if (source.Dictionary.TryGetValue("etf.seg3", out value))
                EtfSeg3 = value;
            if (source.Dictionary.TryGetValue("etf.icb1", out value))
                EtfIcb1 = value;
            if (source.Dictionary.TryGetValue("etf.icb2", out value))
                EtfIcb2 = value;
            if (source.Dictionary.TryGetValue("etf.icb3", out value))
                EtfIcb3 = value;
            if (source.Dictionary.TryGetValue("etf.icb4", out value))
                EtfIcb4 = value;
            if (source.Dictionary.TryGetValue("etf.inav.vendor", out value))
                EtfInavVendor = value;
            if (source.Dictionary.TryGetValue("etf.inav.mep", out value))
                EtfInavMep = value;
            if (source.Dictionary.TryGetValue("etf.inav.isin", out value))
                EtfInavIsin = value;
            if (source.Dictionary.TryGetValue("etf.inav.symbol", out value))
                EtfInavSymbol = value;
            if (source.Dictionary.TryGetValue("etf.inav.name", out value))
                EtfInavName = value;
            if (source.Dictionary.TryGetValue("etf.underlying.vendor", out value))
                EtfUnderlyingVendor = value;
            if (source.Dictionary.TryGetValue("etf.underlying.mep", out value))
                EtfUnderlyingMep = value;
            if (source.Dictionary.TryGetValue("etf.underlying.isin", out value))
                EtfUnderlyingIsin = value;
            if (source.Dictionary.TryGetValue("etf.underlying.symbol", out value))
                EtfUnderlyingSymbol = value;
            if (source.Dictionary.TryGetValue("etf.underlying.name", out value))
                EtfUnderlyingName = value;
            // Etv
            if (source.Dictionary.TryGetValue("etv.cfi", out value))
                EtvCfi = value;
            if (source.Dictionary.TryGetValue("etv.mer", out value))
                EtvMer = value;
            if (source.Dictionary.TryGetValue("etv.launchDate", out value))
                EtvLaunchDate = value;
            if (source.Dictionary.TryGetValue("etv.currency", out value))
                EtvCurrency = value;
            if (source.Dictionary.TryGetValue("etv.issuer", out value))
                EtvIssuer = value;
            if (source.Dictionary.TryGetValue("etv.fraction", out value))
                EtvFraction = value;
            if (source.Dictionary.TryGetValue("etv.dividendFrequency", out value))
                EtvDividendFrequency = value;
            if (source.Dictionary.TryGetValue("etv.indexFamily", out value))
                EtvIndexFamily = value;
            if (source.Dictionary.TryGetValue("etv.seg1", out value))
                EtvSeg1 = value;
            if (source.Dictionary.TryGetValue("etv.seg2", out value))
                EtvSeg2 = value;
            if (source.Dictionary.TryGetValue("etv.seg3", out value))
                EtvSeg3 = value;
            if (source.Dictionary.TryGetValue("etv.icb1", out value))
                EtvIcb1 = value;
            if (source.Dictionary.TryGetValue("etv.icb2", out value))
                EtvIcb2 = value;
            if (source.Dictionary.TryGetValue("etv.icb3", out value))
                EtvIcb3 = value;
            if (source.Dictionary.TryGetValue("etv.icb4", out value))
                EtvIcb4 = value;
            if (source.Dictionary.TryGetValue("etv.inav.vendor", out value))
                EtvInavVendor = value;
            if (source.Dictionary.TryGetValue("etv.inav.mep", out value))
                EtvInavMep = value;
            if (source.Dictionary.TryGetValue("etv.inav.isin", out value))
                EtvInavIsin = value;
            if (source.Dictionary.TryGetValue("etv.inav.symbol", out value))
                EtvInavSymbol = value;
            if (source.Dictionary.TryGetValue("etv.inav.name", out value))
                EtvInavName = value;
            if (source.Dictionary.TryGetValue("etv.underlying.vendor", out value))
                EtvUnderlyingVendor = value;
            if (source.Dictionary.TryGetValue("etv.underlying.mep", out value))
                EtvUnderlyingMep = value;
            if (source.Dictionary.TryGetValue("etv.underlying.isin", out value))
                EtvUnderlyingIsin = value;
            if (source.Dictionary.TryGetValue("etv.underlying.symbol", out value))
                EtvUnderlyingSymbol = value;
            if (source.Dictionary.TryGetValue("etv.underlying.name", out value))
                EtvUnderlyingName = value;
            // Inav
            if (source.Dictionary.TryGetValue("inav.currency", out value))
                InavCurrency = value;
            if (source.Dictionary.TryGetValue("inav.icb1", out value))
                InavIcb1 = value;
            if (source.Dictionary.TryGetValue("inav.icb2", out value))
                InavIcb2 = value;
            if (source.Dictionary.TryGetValue("inav.icb3", out value))
                InavIcb3 = value;
            if (source.Dictionary.TryGetValue("inav.icb4", out value))
                InavIcb4 = value;
            if (source.Dictionary.TryGetValue("inav.target0.vendor", out value))
                InavTarget0Vendor = value;
            if (source.Dictionary.TryGetValue("inav.target0.mep", out value))
                InavTarget0Mep = value;
            if (source.Dictionary.TryGetValue("inav.target0.isin", out value))
                InavTarget0Isin = value;
            if (source.Dictionary.TryGetValue("inav.target0.symbol", out value))
                InavTarget0Symbol = value;
            if (source.Dictionary.TryGetValue("inav.target0.name", out value))
                InavTarget0Name = value;
            if (source.Dictionary.TryGetValue("inav.target1.vendor", out value))
                InavTarget1Vendor = value;
            if (source.Dictionary.TryGetValue("inav.target1.mep", out value))
                InavTarget1Mep = value;
            if (source.Dictionary.TryGetValue("inav.target1.isin", out value))
                InavTarget1Isin = value;
            if (source.Dictionary.TryGetValue("inav.target1.symbol", out value))
                InavTarget1Symbol = value;
            if (source.Dictionary.TryGetValue("inav.target1.name", out value))
                InavTarget1Name = value;
            // Fund
            if (source.Dictionary.TryGetValue("fund.currency", out value))
                FundCurrency = value;
            if (source.Dictionary.TryGetValue("fund.cfi", out value))
                FundCfi = value;
            if (source.Dictionary.TryGetValue("fund.tradingMode", out value))
                FundTradingMode = value;
            if (source.Dictionary.TryGetValue("fund.icb1", out value))
                FundIcb1 = value;
            if (source.Dictionary.TryGetValue("fund.icb2", out value))
                FundIcb2 = value;
            if (source.Dictionary.TryGetValue("fund.icb3", out value))
                FundIcb3 = value;
            if (source.Dictionary.TryGetValue("fund.icb4", out value))
                FundIcb4 = value;
        }
        #endregion

        #region Save
        public void Save(TextWriter file)
        {
            file.WriteLine();
            file.WriteLine("<instrument vendor=\"{0}\"", Vendor.Fix());
            file.Write("\tmep=\"{0}\" isin=\"{1}\" symbol=\"{2}\" name=\"{3}\" type=\"{4}\"", Mep.Fix(), Isin.Fix(), Symbol.Fix(), Name.Fix(), Type.Fix());
            if (!string.IsNullOrEmpty(State))
                file.Write(" state=\"{0}\"", State.Fix());
            file.WriteLine();
            file.WriteLine("\tfile=\"{0}\"", File.Fix());
            if (!string.IsNullOrEmpty(Description))
                file.WriteLine("\tdescription=\"{0}\"", Description.Fix());
            if (!string.IsNullOrEmpty(Notes))
                file.WriteLine("\tnotes=\"{0}\"", Notes.Fix());
            file.WriteLine("\t>");
            if ("index" == Type)
                Index.Save(file);
            else if ("stock" == Type)
                Stock.Save(file);
            else if ("inav" == Type)
                Inav.Save(file);
            else if ("etf" == Type)
                Etf.Save(file);
            else if ("etv" == Type)
                Etv.Save(file);
            else if ("fund" == Type)
                Fund.Save(file);
            file.WriteLine("</instrument>");
        }
        #endregion

        #region CreateFiles
        public void CreateFiles()
        {
            if (string.IsNullOrEmpty(File))
                return;
            string path = string.Concat("created/endofday/" + File);
            FileInfo fi = new FileInfo(path);
            fi.Directory.Create();
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                file.WriteLine("<instruments>");
                file.Write("<instrument vendor=\"{0}\" mep=\"{1}\" isin=\"{2}\" symbol=\"{3}\" name=\"{4}\" type=\"{5}\"",
                    Vendor.Fix(), Mep.Fix(), Isin.Fix(), Symbol.Fix(), Name.Fix(), Type.Fix());
                if ("stock" == Type)
                {
                    if (!string.IsNullOrEmpty(Stock.Currency))
                        file.Write(" currency=\"{0}\"", Stock.Currency.Fix());
                }
                else if ("inav" == Type)
                {
                    if (!string.IsNullOrEmpty(Inav.Currency))
                        file.Write(" currency=\"{0}\"", Inav.Currency.Fix());
                }
                else if ("etf" == Type)
                {
                    if (!string.IsNullOrEmpty(Etf.Currency))
                        file.Write(" currency=\"{0}\"", Etf.Currency.Fix());
                }
                else if ("etv" == Type)
                {
                    if (!string.IsNullOrEmpty(Etv.Currency))
                        file.Write(" currency=\"{0}\"", Etv.Currency.Fix());
                }
                else if ("fund" == Type)
                {
                    if (!string.IsNullOrEmpty(Fund.Currency))
                        file.Write(" currency=\"{0}\"", Fund.Currency.Fix());
                }
                file.WriteLine(">");
                file.WriteLine("<endofday>");
                file.WriteLine("</endofday>");
                file.WriteLine("</instrument>");
                file.WriteLine("</instruments>");
            }
            path = string.Concat("created/intraday/" + File);
            fi = new FileInfo(path);
            fi.Directory.Create();
            using (StreamWriter file = new StreamWriter(string.Concat("created/intraday/" + File)))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                file.WriteLine("<instruments>");
                file.Write("<instrument vendor=\"{0}\" mep=\"{1}\" isin=\"{2}\" symbol=\"{3}\" name=\"{4}\" type=\"{5}\"",
                    Vendor.Fix(), Mep.Fix(), Isin.Fix(), Symbol.Fix(), Name.Fix(), Type.Fix());
                if ("stock" == Type)
                {
                    if (!string.IsNullOrEmpty(Stock.Currency))
                        file.Write(" currency=\"{0}\"", Stock.Currency.Fix());
                }
                else if ("inav" == Type)
                {
                    if (!string.IsNullOrEmpty(Inav.Currency))
                        file.Write(" currency=\"{0}\"", Inav.Currency.Fix());
                }
                else if ("etf" == Type)
                {
                    if (!string.IsNullOrEmpty(Etf.Currency))
                        file.Write(" currency=\"{0}\"", Etf.Currency.Fix());
                }
                else if ("etv" == Type)
                {
                    if (!string.IsNullOrEmpty(Etv.Currency))
                        file.Write(" currency=\"{0}\"", Etv.Currency.Fix());
                }
                else if ("fund" == Type)
                {
                    if (!string.IsNullOrEmpty(Fund.Currency))
                        file.Write(" currency=\"{0}\"", Fund.Currency.Fix());
                }
                file.WriteLine(">");
                file.WriteLine("<intraday>");
                file.WriteLine("</intraday>");
                file.WriteLine("</instrument>");
                file.WriteLine("</instruments>");
            }
        }
        #endregion
    }
    #endregion

    #region Icb
    public class Icb
    {
        public string Icb1 { get; set; }
        public string Icb2 { get; set; }
        public string Icb3 { get; set; }
        public string Icb4 { get; set; }

        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrEmpty(Icb1) &&
                    string.IsNullOrEmpty(Icb2) &&
                    string.IsNullOrEmpty(Icb3) &&
                    string.IsNullOrEmpty(Icb4);
            }
        }

        public void Save(TextWriter file, string prefix)
        {
            file.Write(prefix);
            if (!string.IsNullOrEmpty(Icb1))
                file.Write(" icb1=\"{0}\"", Icb1.Fix());
            if (!string.IsNullOrEmpty(Icb2))
                file.Write(" icb2=\"{0}\"", Icb2.Fix());
            if (!string.IsNullOrEmpty(Icb3))
                file.Write(" icb3=\"{0}\"", Icb3.Fix());
            if (!string.IsNullOrEmpty(Icb4))
                file.Write(" icb4=\"{0}\"", Icb4.Fix());
            file.WriteLine("/>");
        }
    }
    #endregion

    #region Derivative
    public class Derivative
    {
        public string Vendor { get; set; }
        public string Mep { get; set; }
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }

        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrEmpty(Vendor) &&
                    string.IsNullOrEmpty(Mep) &&
                    string.IsNullOrEmpty(Isin) &&
                    string.IsNullOrEmpty(Symbol) &&
                    string.IsNullOrEmpty(Name);
            }
        }

        public void Save(TextWriter file, string prefix)
        {
            file.Write(prefix);
            if (!string.IsNullOrEmpty(Vendor))
                file.Write(" vendor=\"{0}\"", Vendor.Fix());
            if (!string.IsNullOrEmpty(Mep))
                file.Write(" mep=\"{0}\"", Mep.Fix());
            if (!string.IsNullOrEmpty(Isin))
                file.Write(" isin=\"{0}\"", Isin.Fix());
            if (!string.IsNullOrEmpty(Symbol))
                file.Write(" symbol=\"{0}\"", Symbol.Fix());
            if (!string.IsNullOrEmpty(Name))
                file.Write(" name=\"{0}\"", Name.Fix());
            file.WriteLine("/>");
        }
    }
    #endregion

    #region Constituent
    public class Constituent
    {
        public string Vendor { get; set; }
        public string Mep { get; set; }
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Weight { get; set; }

        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrEmpty(Vendor) &&
                    string.IsNullOrEmpty(Mep) &&
                    string.IsNullOrEmpty(Isin) &&
                    string.IsNullOrEmpty(Symbol) &&
                    string.IsNullOrEmpty(Name) &&
                    string.IsNullOrEmpty(Weight);
            }
        }

        public void Save(TextWriter file, string prefix)
        {
            file.Write(prefix);
            if (!string.IsNullOrEmpty(Weight))
                file.Write(" weight=\"{0}\"", Weight.Fix());
            if (!string.IsNullOrEmpty(Vendor))
                file.Write(" vendor=\"{0}\"", Vendor.Fix());
            if (!string.IsNullOrEmpty(Mep))
                file.Write(" mep=\"{0}\"", Mep.Fix());
            if (!string.IsNullOrEmpty(Isin))
                file.Write(" isin=\"{0}\"", Isin.Fix());
            if (!string.IsNullOrEmpty(Symbol))
                file.Write(" symbol=\"{0}\"", Symbol.Fix());
            if (!string.IsNullOrEmpty(Name))
                file.Write(" name=\"{0}\"", Name.Fix());
            file.WriteLine("/>");
        }
    }
    #endregion

    #region Index
    public class Index
    {
        public string Kind { get; set; }
        public string CalcFreq { get; set; }
        public string BaseDate { get; set; }
        public string BaseLevel { get; set; }
        public string Weighting { get; set; }
        public string CapFactor { get; set; }
        public string Family { get; set; }
        public string BaseCap { get; set; }
        public string BaseCapCurrency { get; set; }
        public Icb Icb = new Icb();
        public List<Constituent> ConstituentList = new List<Constituent>();
        public List<Derivative> DerivativeList = new List<Derivative>();

        public Index()
        {
            DerivativeList.Add(new Derivative());
            DerivativeList.Add(new Derivative());
        }

        public void Save(TextWriter file)
        {
            bool constituentEmpty = true;
            foreach (var t in ConstituentList)
                if (!t.IsEmpty)
                {
                    constituentEmpty = false;
                    break;
                }
            bool derivativeEmpty = true;
            foreach (var t in DerivativeList)
                if (!t.IsEmpty)
                {
                    derivativeEmpty = false;
                    break;
                }
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Kind) &&
                string.IsNullOrEmpty(CalcFreq) &&
                string.IsNullOrEmpty(BaseDate) &&
                string.IsNullOrEmpty(BaseLevel) &&
                string.IsNullOrEmpty(Weighting) &&
                string.IsNullOrEmpty(CapFactor) &&
                string.IsNullOrEmpty(Family) &&
                string.IsNullOrEmpty(BaseCap) &&
                string.IsNullOrEmpty(BaseCapCurrency);
            if (attributesEmpty && icbEmpty && constituentEmpty && derivativeEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<index>");
            else
            {
                file.Write("\t<index");
                if (!string.IsNullOrEmpty(Kind))
                    file.Write(" kind=\"{0}\"", Kind.Fix());
                if (!string.IsNullOrEmpty(Family))
                    file.Write(" family=\"{0}\"", Family.Fix());
                if (!string.IsNullOrEmpty(CalcFreq))
                    file.Write(" calcFreq=\"{0}\"", CalcFreq.Fix());
                if (!string.IsNullOrEmpty(BaseDate))
                    file.Write(" baseDate=\"{0}\"", BaseDate.Fix());
                if (!string.IsNullOrEmpty(BaseLevel))
                    file.Write(" baseLevel=\"{0}\"", BaseLevel.Fix());
                if (!string.IsNullOrEmpty(BaseCap))
                    file.Write(" baseCap=\"{0}\"", BaseCap.Fix());
                if (!string.IsNullOrEmpty(BaseCapCurrency))
                    file.Write(" baseCapCurrency=\"{0}\"", BaseCapCurrency.Fix());
                if (!string.IsNullOrEmpty(Weighting))
                    file.Write(" weighting=\"{0}\"", Weighting.Fix());
                if (!string.IsNullOrEmpty(CapFactor))
                    file.Write(" capFactor=\"{0}\"", CapFactor.Fix());
                if (icbEmpty && constituentEmpty && derivativeEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!constituentEmpty)
                ConstituentList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<constituent"); });
            if (!derivativeEmpty)
                DerivativeList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<derivative"); });
            file.WriteLine("\t</index>");
        }
    }
    #endregion

    #region Stock
    public class Stock
    {
        public string Cfi { get; set; }
        public string TradingMode { get; set; }
        public string Compartment { get; set; }
        public string Currency { get; set; }
        public Icb Icb = new Icb();
        public List<Constituent> IndexList = new List<Constituent>();
        public List<Derivative> DerivativeList = new List<Derivative>();

        public void Save(TextWriter file)
        {
            bool indexEmpty = true;
            foreach (var t in IndexList)
                if (!t.IsEmpty)
                {
                    indexEmpty = false;
                    break;
                }
            bool derivativeEmpty = true;
            foreach (var t in DerivativeList)
                if (!t.IsEmpty)
                {
                    derivativeEmpty = false;
                    break;
                }
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Cfi) &&
                string.IsNullOrEmpty(TradingMode) &&
                string.IsNullOrEmpty(Compartment) &&
                string.IsNullOrEmpty(Currency);
            if (attributesEmpty && icbEmpty && indexEmpty && derivativeEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<stock>");
            else
            {
                file.Write("\t<stock");
                if (!string.IsNullOrEmpty(Cfi))
                    file.Write(" cfi=\"{0}\"", Cfi.Fix());
                if (!string.IsNullOrEmpty(Compartment))
                    file.Write(" compartment=\"{0}\"", Compartment.Fix());
                if (!string.IsNullOrEmpty(TradingMode))
                    file.Write(" tradingMode=\"{0}\"", TradingMode.Fix());
                if (!string.IsNullOrEmpty(Currency))
                    file.Write(" currency=\"{0}\"", Currency.Fix());
                if (icbEmpty && indexEmpty && derivativeEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!indexEmpty)
                IndexList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<index"); });
            if (!derivativeEmpty)
                DerivativeList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<derivative"); });
            file.WriteLine("\t</stock>");
        }
    }
    #endregion

    #region Inav
    public class Inav
    {
        public string Currency { get; set; }
        public Icb Icb = new Icb();
        public List<Derivative> TargetList = new List<Derivative>();
        public Inav()
        {
            TargetList.Add(new Derivative());
            TargetList.Add(new Derivative());
        }

        public void Save(TextWriter file)
        {
            bool targetEmpty = true;
            foreach (var t in TargetList)
                if (!t.IsEmpty)
                {
                    targetEmpty = false;
                    break;
                }
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Currency);
            if (attributesEmpty && icbEmpty && targetEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<inav>");
            else
            {
                file.Write("\t<inav");
                if (!string.IsNullOrEmpty(Currency))
                    file.Write(" currency=\"{0}\"", Currency.Fix());
                if (icbEmpty && targetEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!targetEmpty)
                TargetList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<target"); });
            file.WriteLine("\t</inav>");
        }
    }
    #endregion

    #region Etf
    public class Etf
    {
        public string Cfi { get; set; }
        public string Mer { get; set; }
        public string LaunchDate { get; set; }
        public string Currency { get; set; }
        public string Issuer { get; set; }
        public string Fraction { get; set; }
        public string DividendFrequency { get; set; }
        public string IndexFamily { get; set; }
        public string Seg1 { get; set; }
        public string Seg2 { get; set; }
        public string Seg3 { get; set; }
        public Icb Icb = new Icb();
        public Derivative Inav = new Derivative();
        public Derivative Underlying = new Derivative();

        public void Save(TextWriter file)
        {
            bool inavEmpty = Inav.IsEmpty;
            bool underlyingEmpty = Underlying.IsEmpty;
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Cfi) &&
                string.IsNullOrEmpty(Mer) &&
                string.IsNullOrEmpty(LaunchDate) &&
                string.IsNullOrEmpty(Currency) &&
                string.IsNullOrEmpty(Issuer) &&
                string.IsNullOrEmpty(Fraction) &&
                string.IsNullOrEmpty(DividendFrequency) &&
                string.IsNullOrEmpty(IndexFamily) &&
                string.IsNullOrEmpty(Seg1) &&
                string.IsNullOrEmpty(Seg2) &&
                string.IsNullOrEmpty(Seg3);
            if (attributesEmpty && icbEmpty && inavEmpty && underlyingEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<etf>");
            else
            {
                file.Write("\t<etf");
                if (!string.IsNullOrEmpty(Cfi))
                    file.Write(" cfi=\"{0}\"", Cfi.Fix());
                if (!string.IsNullOrEmpty(Mer))
                    file.Write(" mer=\"{0}\"", Mer.Fix());
                if (!string.IsNullOrEmpty(LaunchDate))
                    file.Write(" launchDate=\"{0}\"", LaunchDate.Fix());
                if (!string.IsNullOrEmpty(Currency))
                    file.Write(" currency=\"{0}\"", Currency.Fix());
                if (!string.IsNullOrEmpty(Issuer))
                    file.Write(" issuer=\"{0}\"", Issuer.Fix());
                if (!string.IsNullOrEmpty(Fraction))
                    file.Write(" fraction=\"{0}\"", Fraction.Fix());
                if (!string.IsNullOrEmpty(DividendFrequency))
                    file.Write(" dividendFrequency=\"{0}\"", DividendFrequency.Fix());
                if (!string.IsNullOrEmpty(IndexFamily))
                    file.Write(" indexFamily=\"{0}\"", IndexFamily.Fix());
                if (!string.IsNullOrEmpty(Seg1))
                    file.Write(" seg1=\"{0}\"", Seg1.Fix());
                if (!string.IsNullOrEmpty(Seg2))
                    file.Write(" seg2=\"{0}\"", Seg2.Fix());
                if (!string.IsNullOrEmpty(Seg3))
                    file.Write(" seg3=\"{0}\"", Seg3.Fix());
                if (icbEmpty && inavEmpty && underlyingEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!inavEmpty)
                Inav.Save(file, "\t\t<inav");
            if (!underlyingEmpty)
                Underlying.Save(file, "\t\t<underlying");
            file.WriteLine("\t</etf>");
        }
    }
    #endregion

    #region Etv
    public class Etv
    {
        public string Cfi { get; set; }
        public string Mer { get; set; }
        public string LaunchDate { get; set; }
        public string Currency { get; set; }
        public string Issuer { get; set; }
        public string Fraction { get; set; }
        public string DividendFrequency { get; set; }
        public string IndexFamily { get; set; }
        public string Seg1 { get; set; }
        public string Seg2 { get; set; }
        public string Seg3 { get; set; }
        public Icb Icb = new Icb();
        public Derivative Inav = new Derivative();
        public Derivative Underlying = new Derivative();

        public void Save(TextWriter file)
        {
            bool inavEmpty = Inav.IsEmpty;
            bool underlyingEmpty = Underlying.IsEmpty;
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Cfi) &&
                string.IsNullOrEmpty(Mer) &&
                string.IsNullOrEmpty(LaunchDate) &&
                string.IsNullOrEmpty(Currency) &&
                string.IsNullOrEmpty(Issuer) &&
                string.IsNullOrEmpty(Fraction) &&
                string.IsNullOrEmpty(DividendFrequency) &&
                string.IsNullOrEmpty(IndexFamily) &&
                string.IsNullOrEmpty(Seg1) &&
                string.IsNullOrEmpty(Seg2) &&
                string.IsNullOrEmpty(Seg3);
            if (attributesEmpty && icbEmpty && inavEmpty && underlyingEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<etv>");
            else
            {
                file.Write("\t<etv");
                if (!string.IsNullOrEmpty(Cfi))
                    file.Write(" cfi=\"{0}\"", Cfi.Fix());
                if (!string.IsNullOrEmpty(Mer))
                    file.Write(" mer=\"{0}\"", Mer.Fix());
                if (!string.IsNullOrEmpty(LaunchDate))
                    file.Write(" launchDate=\"{0}\"", LaunchDate.Fix());
                if (!string.IsNullOrEmpty(Currency))
                    file.Write(" currency=\"{0}\"", Currency.Fix());
                if (!string.IsNullOrEmpty(Issuer))
                    file.Write(" issuer=\"{0}\"", Issuer.Fix());
                if (!string.IsNullOrEmpty(Fraction))
                    file.Write(" fraction=\"{0}\"", Fraction.Fix());
                if (!string.IsNullOrEmpty(DividendFrequency))
                    file.Write(" dividendFrequency=\"{0}\"", DividendFrequency.Fix());
                if (!string.IsNullOrEmpty(IndexFamily))
                    file.Write(" indexFamily=\"{0}\"", IndexFamily.Fix());
                if (!string.IsNullOrEmpty(Seg1))
                    file.Write(" seg1=\"{0}\"", Seg1.Fix());
                if (!string.IsNullOrEmpty(Seg2))
                    file.Write(" seg2=\"{0}\"", Seg2.Fix());
                if (!string.IsNullOrEmpty(Seg3))
                    file.Write(" seg3=\"{0}\"", Seg3.Fix());
                if (icbEmpty && inavEmpty && underlyingEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!inavEmpty)
                Inav.Save(file, "\t\t<inav");
            if (!underlyingEmpty)
                Underlying.Save(file, "\t\t<underlying");
            file.WriteLine("\t</etv>");
        }
    }
    #endregion

    #region Fund
    public class Fund
    {
        public string Cfi { get; set; }
        public string TradingMode { get; set; }
        public string Currency { get; set; }
        public Icb Icb = new Icb();
        public List<Derivative> DerivativeList = new List<Derivative>();

        public void Save(TextWriter file)
        {
            bool derivativeEmpty = true;
            foreach (var t in DerivativeList)
                if (!t.IsEmpty)
                {
                    derivativeEmpty = false;
                    break;
                }
            bool icbEmpty = Icb.IsEmpty;
            bool attributesEmpty =
                string.IsNullOrEmpty(Cfi) &&
                string.IsNullOrEmpty(TradingMode) &&
                string.IsNullOrEmpty(Currency);
            if (attributesEmpty && icbEmpty && derivativeEmpty)
                return;
            if (attributesEmpty)
                file.WriteLine("\t<fund>");
            else
            {
                file.Write("\t<fund");
                if (!string.IsNullOrEmpty(Cfi))
                    file.Write(" cfi=\"{0}\"", Cfi.Fix());
                if (!string.IsNullOrEmpty(TradingMode))
                    file.Write(" tradingMode=\"{0}\"", TradingMode.Fix());
                if (!string.IsNullOrEmpty(Currency))
                    file.Write(" currency=\"{0}\"", Currency.Fix());
                if (icbEmpty && derivativeEmpty)
                {
                    file.WriteLine("/>");
                    return;
                }
                file.WriteLine(">");
            }
            if (!icbEmpty)
                Icb.Save(file, "\t\t<icb");
            if (!derivativeEmpty)
                DerivativeList.ForEach(t => { if (!t.IsEmpty) t.Save(file, "\t\t<derivative"); });
            file.WriteLine("\t</fund>");
        }
    }
    #endregion

}
