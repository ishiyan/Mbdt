using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mbdt.EuronextJsonExport
{
    /// <summary>
    /// Euronext JSON export utility.
    /// </summary>
    internal static class EuronextJsonExport
    {
        private static readonly List<string> MicList = new List<string>();
        private static readonly List<string> TradingModesList = new List<string>();
        private static readonly List<string> IndexKindList = new List<string>();
        private static readonly List<string> IndexFamilyList = new List<string>();
        private static readonly List<string> IndexCalculationFrequencyList = new List<string>();
        private static readonly List<string> IndexWeightingList = new List<string>();
        private static readonly List<string> IndexCappingFactorList = new List<string>();
        private static readonly List<string> FundIssuerList = new List<string>();
        private static readonly List<string> EtvIssuerList = new List<string>();
        private static readonly List<string> EtvDividendFrequencyList = new List<string>();
        private static readonly List<string> EtvExpenseRatioList = new List<string>();
        private static readonly List<string> EtvAllInFeesList = new List<string>();
        private static readonly List<string> EtfIssuerList = new List<string>();
        private static readonly List<string> EtfDividendFrequencyList = new List<string>();
        private static readonly List<string> EtfExpositionTypeList = new List<string>();
        private static readonly List<string> EtfFractionList = new List<string>();
        private static readonly List<string> EtfIndexFamilyList = new List<string>();
        private static readonly List<string> EtfTotalExpenseRatioList = new List<string>();

        internal static void JsonExportTask(string xmlPath)
        {
            XDocument xdoc = XDocument.Load(xmlPath
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xeList = xdoc.XPathSelectElements("/instruments/instrument").ToList();
            XElement xelLast = xeList.Last();

            var jsonList = new List<string> { "[" };
            foreach (var xel in xeList)
            {
                var foundInSearch = xel.AttributeValue("foundInSearch", false) ?? "true";
                var type = xel.AttributeValue("type");
                if (foundInSearch != "true" || type == "FOO")
                    continue;
                var sb = new StringBuilder();
                sb.Append("{");
                AddFields(xel, sb);
                sb.Append(xel != xelLast ? "," : "");
                jsonList.Add(sb.ToString());
            }
            jsonList.Add("]");

            var jsonPath = string.Concat(xmlPath, ".exported.json");
            File.WriteAllLines(jsonPath, jsonList);

            foreach (var el in TradingModesList)
                Trace.TraceInformation($"tradingMode: {el}");
            foreach (var el in IndexKindList)
                Trace.TraceInformation($"indexKind: {el}");
            foreach (var el in IndexFamilyList)
                Trace.TraceInformation($"indexFamily: {el}");
            foreach (var el in IndexCalculationFrequencyList)
                Trace.TraceInformation($"indexCalculationFrequency: {el}");
            foreach (var el in IndexWeightingList)
                Trace.TraceInformation($"indexWeighting: {el}");
            foreach (var el in IndexCappingFactorList)
                Trace.TraceInformation($"indexCappingFactor: {el}");
            foreach (var el in FundIssuerList)
                Trace.TraceInformation($"fundIssuer: {el}");
            foreach (var el in EtvIssuerList)
                Trace.TraceInformation($"etvIssuer: {el}");
            foreach (var el in EtvDividendFrequencyList)
                Trace.TraceInformation($"etvDividendFrequency: {el}");
            foreach (var el in EtvExpenseRatioList)
                Trace.TraceInformation($"etvExpenseRatio: {el}");
            foreach (var el in EtvAllInFeesList)
                Trace.TraceInformation($"etvAllInFees: {el}");
            foreach (var el in EtfIssuerList)
                Trace.TraceInformation($"etfIssuer: {el}");
            foreach (var el in EtfDividendFrequencyList)
                Trace.TraceInformation($"etfDividendFrequency: {el}");
            foreach (var el in EtfExpositionTypeList)
                Trace.TraceInformation($"etfExpositionType: {el}");
            foreach (var el in EtfFractionList)
                Trace.TraceInformation($"etfFraction: {el}");
            foreach (var el in EtfIndexFamilyList)
                Trace.TraceInformation($"etfIndexFamily: {el}");
            foreach (var el in EtfTotalExpenseRatioList)
                Trace.TraceInformation($"etfTotalExpenseRatio: {el}");
            foreach (var el in MicList)
                Trace.TraceInformation($"mic: {el}");
        }

        private static void AddFields(XElement xel, StringBuilder sb)
        {
            // var file = xel.AttributeValue("file");
            var symbol = xel.AttributeValue("symbol");
            var name = xel.AttributeValue("name");
            var mic = xel.AttributeValue("mic");
            var isin = xel.AttributeValue("isin");
            var type = xel.AttributeValue("type");
            var description = xel.AttributeValue("description", false);

            sb.Append($"\"symbol\":\"{symbol}\"");
            sb.Append($",\"name\":\"{name}\"");
            sb.Append($",\"type\":\"{type}\"");
            sb.Append($",\"mic\":\"{mic}\"");
            sb.Append($",\"isin\":\"{isin}\"");

            switch (type)
            {
                case "stock":
                    AddStockFields(xel, sb);
                    break;
                case "index":
                    AddIndexFields(xel, sb);
                    break;
                case "fund":
                    AddFundFields(xel, sb);
                    break;
                case "etv":
                    AddEtvFields(xel, sb);
                    break;
                case "etf":
                    AddEtfFields(xel, sb);
                    break;
                case "inav":
                    AddInavFields(xel, sb);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(description))
                sb.Append($",\"description\":\"{description}\"");
            sb.Append("}");

            if (!MicList.Contains(mic))
                MicList.Add(mic);

        }

        private static void AddStockFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("stock");
            var currency = xel.AttributeValue("currency");
            var tradingMode = xel.AttributeValue("tradingMode", false);
            var cfi = xel.AttributeValue("cfi", false);
            var icb = xel.IcbAttributeValue();
            var shares = xel.AttributeValue("shares", false);

            sb.Append(",\"stock\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            sb.AppendIfExists("tradingMode", MapTradingMode(tradingMode));
            sb.AppendIfExists("cfi", MapCfi(cfi));
            sb.AppendIfExists("icb", MapIcb(icb));
            sb.AppendIfExists("shares", MapShares(shares));
            sb.Append("}");


            if (!TradingModesList.Contains(tradingMode))
                TradingModesList.Add(tradingMode);
        }

        private static void AddIndexFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("index");
            var currency = xel.AttributeValue("currency");
            var kind = xel.AttributeValue("kind", false);
            var family = xel.AttributeValue("family", false);
            var calculationFrequency = xel.AttributeValue("calcFreq", false);
            var baseDate = xel.AttributeValue("baseDate", false);
            var baseLevel = xel.AttributeValue("baseLevel", false);
            var weighting = xel.AttributeValue("weighting", false);
            var cappingFactor = xel.AttributeValue("capFactor", false);
            var icb = xel.IcbAttributeValue();

            sb.Append(",\"index\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            sb.AppendIfExists("kind", MapKind(kind));
            sb.AppendIfExists("family", MapFamily(family));
            sb.AppendIfExists("calculationFrequency", MapCalculationFrequency(calculationFrequency));
            sb.AppendIfExists("baseDate", MapBaseDate(baseDate));
            sb.AppendIfExists("baseLevel", MapBaseLevel(baseLevel));
            sb.AppendIfExists("weighting", MapWeighting(weighting));
            sb.AppendIfExists("cappingFactor", MapCappingFactor(cappingFactor));
            sb.AppendIfExists("icb", MapIcb(icb));
            sb.Append("}");

            if (!string.IsNullOrWhiteSpace(kind) && !IndexKindList.Contains(kind))
                IndexKindList.Add(kind);
            if (!string.IsNullOrWhiteSpace(family) && !IndexFamilyList.Contains(family))
                IndexFamilyList.Add(family);
            if (!string.IsNullOrWhiteSpace(calculationFrequency) && !IndexCalculationFrequencyList.Contains(calculationFrequency))
                IndexCalculationFrequencyList.Add(calculationFrequency);
            if (!string.IsNullOrWhiteSpace(weighting) && !IndexWeightingList.Contains(weighting))
                IndexWeightingList.Add(weighting);
            if (!string.IsNullOrWhiteSpace(cappingFactor) && !IndexCappingFactorList.Contains(cappingFactor))
                IndexCappingFactorList.Add(cappingFactor);
        }

        private static void AddFundFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("fund");
            if (xel == null)
                return;
            var currency = xel.AttributeValue("currency");
            var tradingMode = xel.AttributeValue("tradingMode", false);
            var cfi = xel.AttributeValue("cfi", false);
            var issuer = xel.AttributeValue("issuer", false);
            var shares = xel.AttributeValue("shares", false);

            sb.Append(",\"fund\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            sb.AppendIfExists("tradingMode", MapTradingMode(tradingMode));
            sb.AppendIfExists("cfi", MapCfi(cfi));
            sb.AppendIfExists("issuer", MapIssuer(issuer));
            sb.AppendIfExists("sharesOutstanding", MapShares(shares));
            sb.Append("}");

            if (!string.IsNullOrWhiteSpace(tradingMode) && !TradingModesList.Contains(tradingMode))
                TradingModesList.Add(tradingMode);
            if (!string.IsNullOrWhiteSpace(issuer) && !FundIssuerList.Contains(issuer))
                FundIssuerList.Add(issuer);
        }

        private static void AddEtvFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("etv");
            if (xel == null)
                return;
            var currency = xel.AttributeValue("currency");
            var tradingMode = xel.AttributeValue("tradingMode", false);
            var allInFees = xel.AttributeValue("allInFees", false);
            var expenseRatio = xel.AttributeValue("expenseRatio", false);
            var dividendFrequency = xel.AttributeValue("dividendFrequency", false);
            var issuer = xel.AttributeValue("issuer", false);
            var shares = xel.AttributeValue("shares", false);

            sb.Append(",\"fund\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            sb.AppendIfExists("tradingMode", MapTradingMode(tradingMode));
            sb.AppendIfExists("allInFees", MapAllInFees(allInFees));
            sb.AppendIfExists("expenseRatio", MapExpenseRatio(expenseRatio));
            sb.AppendIfExists("dividendFrequency", MapDividendFrequency(dividendFrequency));
            sb.AppendIfExists("issuer", MapIssuer(issuer));
            sb.AppendIfExists("sharesOutstanding", MapShares(shares));
            sb.Append("}");

            if (!string.IsNullOrWhiteSpace(tradingMode) && !TradingModesList.Contains(tradingMode))
                TradingModesList.Add(tradingMode);
            if (!string.IsNullOrWhiteSpace(issuer) && !EtvIssuerList.Contains(issuer))
                EtvIssuerList.Add(issuer);
            if (!string.IsNullOrWhiteSpace(allInFees) && !EtvAllInFeesList.Contains(allInFees))
                EtvAllInFeesList.Add(allInFees);
            if (!string.IsNullOrWhiteSpace(expenseRatio) && !EtvExpenseRatioList.Contains(expenseRatio))
                EtvExpenseRatioList.Add(expenseRatio);
            if (!string.IsNullOrWhiteSpace(dividendFrequency) && !EtvDividendFrequencyList.Contains(dividendFrequency))
                EtvDividendFrequencyList.Add(dividendFrequency);
        }

        private static void AddEtfFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("etf");
            if (xel == null)
                return;
            var currency = xel.AttributeValue("currency");
            var tradingMode = xel.AttributeValue("tradingMode", false);
            var cfi = xel.AttributeValue("cfi", false);
            var dividendFrequency = xel.AttributeValue("dividendFrequency", false);
            var expositionType = xel.AttributeValue("expositionType", false);
            var fraction = xel.AttributeValue("fraction", false);
            var totalExpenseRatio = xel.AttributeValue("ter", false);//
            var indexFamily = xel.AttributeValue("indexFamily", false);
            var launchDate = xel.AttributeValue("launchDate", false);
            var issuer = xel.AttributeValue("issuer", false);
            var xelInav = xel.Element("inav");
            var inavMic = xelInav.AttributeValue("mic", false);
            var inavIsin = xelInav.AttributeValue("isin", false);
            var inavSymbol = xelInav.AttributeValue("symbol", false);
            var inavName = xelInav.AttributeValue("name", false);
            var xelUnder = xel.Element("underlying");
            var underMic = xelUnder.AttributeValue("mic", false);
            var underIsin = xelUnder.AttributeValue("isin", false);
            var underSymbol = xelUnder.AttributeValue("symbol", false);
            var underName = xelUnder.AttributeValue("name", false);

            sb.Append(",\"etf\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            sb.AppendIfExists("tradingMode", MapTradingMode(tradingMode));
            sb.AppendIfExists("cfi", MapCfi(cfi));
            sb.AppendIfExists("dividendFrequency", MapDividendFrequency(dividendFrequency));
            sb.AppendIfExists("expositionType", MapExpositionType(expositionType));
            sb.AppendIfExists("fraction", MapFraction(fraction));
            sb.AppendIfExists("totalExpenseRatio", MapTotalExpenseRatio(totalExpenseRatio));
            sb.AppendIfExists("indexFamily", MapIndexFamily(indexFamily));
            sb.AppendIfExists("launchDate", MapLaunchDate(launchDate));
            sb.AppendIfExists("issuer", MapIssuer(issuer));

            if (!string.IsNullOrWhiteSpace(inavMic) ||
                !string.IsNullOrWhiteSpace(inavIsin) ||
                !string.IsNullOrWhiteSpace(inavSymbol) ||
                !string.IsNullOrWhiteSpace(inavName))
            {
                bool comma = false;
                sb.Append(",\"inav\":{");
                sb.AppendIfExists("mic", inavMic, false);
                if (!string.IsNullOrWhiteSpace(inavMic))
                    comma = true;
                sb.AppendIfExists("isin", inavIsin, comma);
                if (!string.IsNullOrWhiteSpace(inavIsin))
                    comma = true;
                sb.AppendIfExists("symbol", inavSymbol, comma);
                if (!string.IsNullOrWhiteSpace(inavSymbol))
                    comma = true;
                sb.AppendIfExists("name", inavName, comma);
                sb.Append("}");
            }

            if (!string.IsNullOrWhiteSpace(underMic) ||
                !string.IsNullOrWhiteSpace(underIsin) ||
                !string.IsNullOrWhiteSpace(underSymbol) ||
                !string.IsNullOrWhiteSpace(underName))
            {
                bool comma = false;
                sb.Append(",\"underlying\":{");
                sb.AppendIfExists("mic", underMic, false);
                if (!string.IsNullOrWhiteSpace(underMic))
                    comma = true;
                sb.AppendIfExists("isin", underIsin, comma);
                if (!string.IsNullOrWhiteSpace(underIsin))
                    comma = true;
                sb.AppendIfExists("symbol", underSymbol, comma);
                if (!string.IsNullOrWhiteSpace(underSymbol))
                    comma = true;
                sb.AppendIfExists("name", underName, comma);
                sb.Append("}");
            }
            sb.Append("}");

            if (!string.IsNullOrWhiteSpace(tradingMode) && !TradingModesList.Contains(tradingMode))
                TradingModesList.Add(tradingMode);
            if (!string.IsNullOrWhiteSpace(issuer) && !EtfIssuerList.Contains(issuer))
                EtfIssuerList.Add(issuer);
            if (!string.IsNullOrWhiteSpace(dividendFrequency) && !EtfDividendFrequencyList.Contains(dividendFrequency))
                EtfDividendFrequencyList.Add(dividendFrequency);
            if (!string.IsNullOrWhiteSpace(expositionType) && !EtfExpositionTypeList.Contains(expositionType))
                EtfExpositionTypeList.Add(expositionType);
            if (!string.IsNullOrWhiteSpace(fraction) && !EtfFractionList.Contains(fraction))
                EtfFractionList.Add(fraction);
            if (!string.IsNullOrWhiteSpace(indexFamily) && !EtfIndexFamilyList.Contains(indexFamily))
                EtfIndexFamilyList.Add(indexFamily);
            if (!string.IsNullOrWhiteSpace(totalExpenseRatio) && !EtfTotalExpenseRatioList.Contains(totalExpenseRatio))
                EtfTotalExpenseRatioList.Add(totalExpenseRatio);
        }

        private static void AddInavFields(XElement xel, StringBuilder sb)
        {
            xel = xel.Element("inav");
            if (xel == null)
                return;
            var currency = xel.AttributeValue("currency");
            var xelTgt = xel.Element("target");
            var tgtMic = xelTgt.AttributeValue("mic", false);
            var tgtIsin = xelTgt.AttributeValue("isin", false);
            var tgtSymbol = xelTgt.AttributeValue("symbol", false);
            var tgtName = xelTgt.AttributeValue("name", false);

            sb.Append(",\"inav\":{");
            sb.AppendFirst("currency", MapCurrency(currency));
            if (!string.IsNullOrWhiteSpace(tgtMic) ||
                !string.IsNullOrWhiteSpace(tgtIsin) ||
                !string.IsNullOrWhiteSpace(tgtSymbol) ||
                !string.IsNullOrWhiteSpace(tgtName))
            {
                bool comma = false;
                sb.Append(",\"target\":{");
                sb.AppendIfExists("mic", tgtMic, false);
                if (!string.IsNullOrWhiteSpace(tgtMic))
                    comma = true;
                sb.AppendIfExists("isin", tgtIsin, comma);
                if (!string.IsNullOrWhiteSpace(tgtIsin))
                    comma = true;
                sb.AppendIfExists("symbol", tgtSymbol, comma);
                if (!string.IsNullOrWhiteSpace(tgtSymbol))
                    comma = true;
                sb.AppendIfExists("name", tgtName, comma);
                sb.Append("}");
            }
            sb.Append("}");
        }

        private static string MapCurrency(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "EUR": return "eur";
                case "USD": return "usd";
                case "GBP": return "gbp";
                case "GBX": return "gbx";
                case "JPY": return "jpy";
                case "CNY": return "cny";
                case "CHF": return "chf";
                case "DKK": return "dkk";
                case "NOK": return "nok";
                case "SEK": return "sek";
                case "PLN": return "pln";
                case "BGN": return "bgn";
                case "HUF": return "huf";
                case "ISK": return "isk";
                case "TRY": return "try";
                case "CZK": return "czk";
                case "RON": return "ron";
                case "LTL": return "ltl";
                case "LVL": return "lvl";
                case "RUB": return "rub";
            }
            if (string.IsNullOrWhiteSpace(value))
                return null;
            throw new ArgumentException($"unknown currency {value}");
        }

        private static string MapTradingMode(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "continuous": return "continuous";
                case "double call auction": return "doubleCallAuction";
                case "fixing": return "fixing";
                case "unknown": return "unknown";
            }

            if (string.IsNullOrWhiteSpace(value))
                return null;
            throw new ArgumentException($"unknown trading mode {value}");
        }

        private static string MapShares(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.Replace(",", "");
        }

        private static string MapKind(string value)
        {
            return value;
        }

        private static string MapFamily(string value)
        {
            return value;
        }

        private static string MapCalculationFrequency(string value)
        {
            return value;
        }

        private static string MapBaseDate(string value)
        {
            return value;
        }

        private static string MapBaseLevel(string value)
        {
            return value;
        }

        private static string MapWeighting(string value)
        {
            return value;
        }

        private static string MapCappingFactor(string value)
        {
            return value;
        }

        private static string MapAllInFees(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.Replace(",", ".");
        }

        private static string MapFraction(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.Replace(",", ".");
        }

        private static string MapExpositionType(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "physical": return "physical";
                case "synthetic": return "synthetic";
                case "active": return "active";
            }

            if (string.IsNullOrWhiteSpace(value))
                return null;
            throw new ArgumentException($"unknown exposition type {value}");
        }

        private static string MapTotalExpenseRatio(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return value.Replace(",", ".");
        }

        private static string MapLaunchDate(string value)
        {
            return value;
        }

        private static string MapIssuer(string value)
        {
            return value;
        }

        private static string MapDividendFrequency(string value)
        {
            return value;
        }

        private static string MapExpenseRatio(string value)
        {
            return value;
        }

        private static string MapIndexFamily(string value)
        {
            return value;
        }

        private static string MapCfi(string value)
        {
            return value;
        }

        private static string MapIcb(string value)
        {
            return value;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static string AttributeValue(this XElement xel, string attributeName, bool mustExist = true)
        {
            XAttribute attribute = xel.Attribute(attributeName);
            if (attribute == null && mustExist)
                throw new ArgumentException($"attribute {attributeName} does not exist in {xel}");
            return attribute?.Value;
        }

        private static string IcbAttributeValue(this XElement xel)
        {
            xel = xel.Element("icb");
            if (xel == null)
                return null;
            var icb = xel.AttributeValue("icb4", false);
            if (string.IsNullOrWhiteSpace(icb))
                icb = xel.AttributeValue("icb3", false);
            if (string.IsNullOrWhiteSpace(icb))
                icb = xel.AttributeValue("icb2", false);
            if (string.IsNullOrWhiteSpace(icb))
                icb = xel.AttributeValue("icb1", false);
            return icb;
        }

        private static void AppendFirst(this StringBuilder sb, string name, string value)
        {
            sb.Append($"\"{name}\":\"{value}\"");
        }

        private static void AppendIfExists(this StringBuilder sb, string name, string value, bool comma = true)
        {
            var str = comma ? "," : "";
            if (!string.IsNullOrWhiteSpace(value))
                sb.Append($"{str}\"{name}\":\"{value}\"");
        }
    }
}
