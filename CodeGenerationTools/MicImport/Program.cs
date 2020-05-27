using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
// ReSharper disable UseStringInterpolation
// ReSharper disable LoopCanBeConvertedToQuery

namespace MicImport
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private class Record
        {
            private readonly string country;
            public string Country => country;
            private readonly string countryFull;
            public string CountryFull => countryFull;

            private readonly string countryCode;
            public string CountryCode => countryCode;

            private readonly string mic;
            public string Mic => mic;

            private readonly string description;
            public string Description => description;

            //private readonly string acronym;
            //public string Acronym => acronym;

            //private readonly string city;
            //public string City => city;

            private readonly string website;
            public string Website => website;

            //private readonly string date;
            //public string Date => date;

            //private readonly string status;
            //public string Status => status;

            private readonly bool isEuronext;
            public bool IsEuronext => isEuronext;

            public Record(string line)
            {
                // COUNTRY;CC;MIC;INSTITUTION DESCRIPTION;ACCR;CITY;WEBSITE;DATE;STATUS
                string[] cells = line.Split(';');

                #region Country
                country = cells[0].ToUpper();
                country = country.Replace(", REPUBLIC OF", "");
                country = country.Replace(" (REPUBLIC OF)", "").Trim();
                if ("CAPE VERDE" == country)
                    country = "Cape Verde";
                else if ("BOSNIA AND HERZEGOVINA" == country)
                    country = "Bosnia and Herzegovina";
                else if ("CAYMAN ISLANDS" == country)
                    country = "Cayman Islands";
                else if ("COSTA RICA" == country)
                    country = "Costa Rica";
                else if ("CZECH REPUBLIC" == country)
                    country = "Czech Republic";
                else if ("DOMINICAN REPUBLIC" == country)
                    country = "Dominican Republic";
                else if ("GUERNSEY, C.I." == country)
                    country = "Channel Islands";
                else if ("NEW ZEALAND" == country)
                    country = "New Zealand";
                else if ("PAPUA NEW GUINEA" == country)
                    country = "Papua New Guinea";
                else if ("SAINT KITTS AND NEVIS" == country)
                    country = "Saint Kitts and Nevis";
                else if ("SOUTH AFRICA" == country)
                    country = "South Africa";
                else if ("SRI LANKA" == country)
                    country = "Sri Lanka";
                else if ("SYRIAN ARAB REPUBLIC" == country)
                    country = "Syria";
                else if ("THE NETHERLANDS" == country)
                    country = "The Netherlands";
                else if ("TRINIDAD AND TOBAGO" == country)
                    country = "Trinidad and Tobago";
                else if ("UNITED ARAB EMIRATES" == country)
                    country = "Arab Emirates";
                else if ("UNITED KINGDOM" == country)
                    country = "United Kingdom";
                else if ("UNITED STATES OF AMERICA" == country)
                    country = "United States";
                else if ("VIET NAM" == country)
                    country = "Viet Nam";
                else if ("ZZ" == country)
                    country = "No Country";
                else if ("PALESTINIAN TERRITORY, OCCUPIED" == country)
                    country = "Palestinian Territory";
                else if ("SAUDI ARABIA" == country)
                    country = "Saudi Arabia";
                else if ("EL SALVADOR" == country)
                    country = "Salvador";
                else if ("HONG KONG" == country)
                    country = "Hong Kong";
                else if ("HONG-KONG" == country)
                    country = "Hong Kong";
                else if ("IVORY COAST" == country)
                    country = "Ivory Coast";
                else if ("LIBYANARABJAMAHIRIYA" == country)
                    country = "Libyan Arab Jamahiriya";
                else if ("LIBYAN ARAB JAMAHIRIYA" == country)
                    country = "Libyan Arab Jamahiriya";
                else if ("CZECHREPUBLIC" == country)
                    country = "Czech Republic";
                else if ("UNITEDARABEMIRATES" == country)
                    country = "Arab Emirates";
                else if ("SEYCHELLES" == country)
                    country = "Seychelles";
                else
                {
                    country = country.ToLowerInvariant();
                    country = string.Concat(country.Substring(0, 1).ToUpperInvariant(), country.Substring(1));
                }
                countryFull = country;
                country = country.Replace("The ", "").Replace(" and ", "").Replace(" ", "");
                #endregion
                #region CountryCode
                countryCode = cells[1].ToUpperInvariant().Trim();
                #endregion
                #region Mic
                mic = cells[2].ToLowerInvariant().Trim();
                mic = string.Concat(mic.Substring(0, 1).ToUpperInvariant(), mic.Substring(1));
                if (mic.StartsWith("360t"))
                    mic = "X360T";
                if (mic == "C2ox")
                    mic = "C2Ox";
                if (mic == "N2ex")
                    mic = "N2Ex";
                #endregion
                #region Decription
                description = cells[3].Trim().ToUpper();
                description = description.Replace("’", "").Replace("’", "").Replace("\"", "").Replace(".", "").Replace("'", "").Replace("“", "").Replace("”", "");
                isEuronext = description.Contains("EURONEXT");
                if (description.StartsWith("NYSE EURONEXT - MERCADO DE FUTUROS E OP"))
                    description = "NYSE EURONEXT - MERCADO DE FUTUROS E OPÇÕES";
                if (description.StartsWith("EURONEXT - MERCADO DE FUTUROS E OP"))
                    description = "EURONEXT - MERCADO DE FUTUROS E OPÇÕES";
                if (description.StartsWith("BOLSA DE CEREAIS E MERCADORIAS DE MARING"))
                    description = "BOLSA DE CEREAIS E MERCADORIAS DE MARINGÁ";
                if (description.StartsWith("BOLSA DE VALORES MINAS-ESP"))
                    description = "BOLSA DE VALORES MINAS-ESPÍRITO SANTO-BRASÍLIA";
                if (description.StartsWith("GEMMA (GILT EDGED MARKET MAKERS"))
                    description = "GEMMA (GILT EDGED MARKET MAKERS ASSOCIATION)";
                if (description.StartsWith("JOINT-STOCK COMPANY "))
                    description = "JOINT-STOCK COMPANY STOCK EXCHANGE INNEX";
                if (description.StartsWith("OFF-EXCHANGE TRANSACTIONS - LISTED INSTRUMENTS"))
                    description = "MIC TO USE FOR OFF-EXCHANGE TRANSACTIONS IN LISTED INSTRUMENTS";
                #endregion
                #region Website
                website = cells[6].ToLower().Trim();
                if (website.Length > 3)
                {
                    if (website.StartsWith("www."))
                        website = string.Concat(@"http://", website);
                    else if (!website.StartsWith("http://") && !website.StartsWith("https://"))
                        website = string.Concat(@"http://", website);
                }
                else
                    website = null;
                #endregion
            }
        }

        private class TimeZoneCountryCodes
        {
            public string TimeSpan { get; set; }
            public string TimeSpanMinutes { get; set; }
            public List<string> CountryCodes { get; set; }
        }

        private static readonly List<TimeZoneCountryCodes> timeZoneCountryCodes = new List<TimeZoneCountryCodes>(32);
        private static readonly List<Record> records = new List<Record>(1024);
        private static readonly Dictionary<string, List<Record>> recordsPerCountry = new Dictionary<string, List<Record>>(1024);
        private static readonly Dictionary<string, List<Record>> recordsPerCountryCode = new Dictionary<string, List<Record>>(1024);
        //private static readonly Dictionary<string, string> countryCodeToCountry = new Dictionary<string, string>(256);
        private static readonly string includes = ListIncludes();
        private static readonly string includesH = ListIncludesH();

        private static string ListIncludes()
        {
            if (Properties.Settings.Default.IncludeOnly.Count <= 0)
                return null;
            string result = "/// <para>Only the following countries are included:</para><para>";
            for (int i = 0; i < Properties.Settings.Default.IncludeOnly.Count; ++i)
            {
                result = string.Concat(result, Properties.Settings.Default.IncludeOnly[i]);
                if (i < Properties.Settings.Default.IncludeOnly.Count - 1)
                    result = string.Concat(result, ", ");
                else
                    result = string.Concat(result, ".");
            }
            return string.Concat(result, "</para>");
        }

        private static string ListIncludesH()
        {
            if (Properties.Settings.Default.IncludeOnly.Count <= 0)
                return null;
            string result = "//! Only the following countries are included: ";
            for (int i = 0; i < Properties.Settings.Default.IncludeOnly.Count; ++i)
            {
                result = string.Concat(result, Properties.Settings.Default.IncludeOnly[i]);
                if (i < Properties.Settings.Default.IncludeOnly.Count - 1)
                    result = string.Concat(result, ", ");
                else
                    result = string.Concat(result, ".");
            }
            return string.Concat(result, ".");
        }

        private static void ReadInput(string fileName)
        {
            using (var streamReader = new StreamReader(fileName,Encoding.UTF8))
            {
                streamReader.ReadLine(); // skip the first line
                string line;
                while (null != (line = streamReader.ReadLine()))
                {
                    var record = new Record(line);
                    if (Properties.Settings.Default.IncludeOnly.Count > 0 &&
                        !Properties.Settings.Default.IncludeOnly.Contains(record.Country))
                        continue;
                    records.Add(record);

                    if (!recordsPerCountry.ContainsKey(record.Country))
                        recordsPerCountry.Add(record.Country, new List<Record>(256));
                    recordsPerCountry[record.Country].Add(record);

                    if (record.CountryCode.Length > 1)
                    {
                        if (!recordsPerCountryCode.ContainsKey(record.CountryCode))
                            recordsPerCountryCode.Add(record.CountryCode, new List<Record>(256));
                        recordsPerCountryCode[record.CountryCode].Add(record);
                    }
                }
                streamReader.Close();
            }
        }

        private static void WriteFile(IEnumerable<string> list, string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                foreach (var line in list)
                    streamWriter.WriteLine(line);
                streamWriter.Close();
            }
        }

        private static string ExtractVersion(string fileName, string date)
        {
            // The CSV file name has a standard pattern ISO10383_MIC_v1_81.csv
            var name = new FileInfo(fileName).Name;
            name = name.Substring(0, name.Length - ".csv".Length);
            string[] splitted = name.Split('_');
            string version = string.Concat("version ", splitted[2].Trim().Substring(1), ".", splitted[3].Trim());
            return date != null ? string.Concat(version, " (", date, ")") : version;
        }

        private static IEnumerable<string> GenerateExchangeMic(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"using System.ComponentModel;");
            list.Add(@"");
            list.Add(@"namespace Mbst.Trading");
            list.Add(@"{");
            list.Add("    /// <summary>");
            list.Add(@"    /// Exchange representations according to ISO 10383 Market Identifier Code (MIC).");
            list.Add(string.Concat(@"    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @".</para>"));
            list.Add(@"    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includes));
            list.Add(@"    /// </summary>");
            list.Add(@"    [System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Naming"", ""CA1709:IdentifiersShouldBeCasedCorrectly"", Scope = ""namespace"", Target = ""Mbst.Trading.ExchangeMic"", Justification = ""Compiance with the ISO 10383 Market Identifier Code."")]");
            list.Add(@"    public enum ExchangeMic");
            list.Add(@"    {");
            bool first = true;
            int count = 0, total = records.Count;
            foreach (var record in records)
            {
                if (record.Website != null)
                {
                    list.Add(string.Format("        /// <summary>{0}<para>{1}</para></summary>", record.Description.Replace("&", "&amp;"), record.Website));
                    if (first)
                    {
                        list.Add(@"        [System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Naming"", ""CA1709:IdentifiersShouldBeCasedCorrectly"", Scope = ""member"", Target = ""Mbst.Trading.ExchangeMic.#*"", Justification = ""Compiance with the ISO 10383 Market Identifier Code."")]");
                        first = false;
                    }
                    list.Add(string.Format("        [Description(@\"{0} [{1}]\")]", record.Description, record.CountryCode));
                }
                else
                {
                    list.Add(string.Format("        /// <summary>{0}</summary>", record.Description.Replace("&", "&amp;")));
                    if (first)
                    {
                        list.Add(@"        [System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Naming"", ""CA1709:IdentifiersShouldBeCasedCorrectly"", Scope = ""member"", Target = ""Mbst.Trading.ExchangeMic.#*"", Justification = ""Compiance with the ISO 10383 Market Identifier Code."")]");
                        first = false;
                    }
                    list.Add(string.Format("        [Description(@\"{0}\")]", record.Description));
                }
                list.Add(string.Format(++count == total ? "        {0}" : "        {0},", record.Mic));
            }
            list.Add(@"    }");
            list.Add(@"}");
            return list;
        }

        private static List<string> GenerateExchangeHeaderH()
        {
            var list = new List<string>(4096);
            list.Add(@"#pragma once");
            list.Add(@"");
            list.Add(@"// This file is generated automatically.");
            list.Add(@"");
            list.Add(@"namespace mbsl { namespace trading {");
            return list;
        }

        private static List<string> GenerateExchangeFooterH(List<string> list)
        {
            list.Add(@"}}");
            return list;
        }

        private static List<string> GenerateExchangeMicH(List<string> list, string versionInfo)
        {
            list.Add(@"");
            list.Add(@"    //! Exchange representations according to ISO 10383 Market Identifier Code (MIC).");
            list.Add(string.Concat(@"    //! Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"    //! http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includesH));
            list.Add(@"    enum class ExchangeMic");
            list.Add(@"    {");
            int count = 0, total = records.Count;
            foreach (var record in records)
            {
                if (record.Website != null)
                {
                    list.Add(string.Format("        //! {0} [{1}], {2}", record.Description, record.CountryCode, record.Website));
                }
                else
                {
                    list.Add(string.Format("        //! {0} [{1}]", record.Description, record.CountryCode));
                }
                list.Add(string.Format(++count == total ? "        {0}" : "        {0},", record.Mic));
            }
            list.Add(@"    };");
            return list;
        }

        private static IEnumerable<string> GenerateEuronextMic(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"using System.ComponentModel;");
            list.Add(@"");
            list.Add(@"namespace Mbst.Trading");
            list.Add(@"{");
            list.Add("    /// <summary>");
            list.Add(@"    /// NYSE Euronext (http://www.euronext.com) exchanges MIC helper.");
            list.Add(string.Concat(@"    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @".</para>"));
            list.Add(@"    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includes));
            list.Add(@"    /// </summary>");
            list.Add(@"    [System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Naming"", ""CA1709:IdentifiersShouldBeCasedCorrectly"", Scope = ""namespace"", Target = ""Mbst.Trading.ExchangeMic"", Justification = ""Compiance with the ISO 10383 Market Identifier Code."")]");
            list.Add(@"    public enum EuronextMic");
            list.Add(@"    {");
            bool first = true;
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        /// <summary>{0}</summary>", record.Description.Replace("&", "&amp;")));
                    if (first)
                    {
                        list.Add(@"        [System.Diagnostics.CodeAnalysis.SuppressMessage(""Microsoft.Naming"", ""CA1709:IdentifiersShouldBeCasedCorrectly"", Scope = ""member"", Target = ""Mbst.Trading.EuronextMic.#*"", Justification = ""Compiance with the ISO 10383 Market Identifier Code."")]");
                        first = false;
                    }
                    list.Add(string.Format("        [Description(@\"{0}\")]", record.Description));
                    list.Add(string.Format("        {0},", record.Mic));
                }
            }
            list.Add(@"        /// <summary>EURONEXT OTHER OR UNKNOWN</summary>");
            list.Add(@"        [Description(@""UNKNOWN"")]");
            list.Add(@"        Xxxx");
            list.Add(@"    }");
            list.Add(@"}");
            return list;
        }

        private static List<string> GenerateEuronextMicH(List<string> list, string versionInfo)
        {
            list.Add(@"");
            list.Add(@"    //! NYSE Euronext (http://www.euronext.com) exchanges MIC helper.");
            list.Add(string.Concat(@"    //! Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"    //! http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includesH));
            list.Add(@"    enum class EuronextMic");
            list.Add(@"    {");
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        //! {0} [{1}]", record.Description, record.CountryCode));
                    list.Add(string.Format("        {0},", record.Mic));
                }
            }
            list.Add(@"        //! EURONEXT OTHER OR UNKNOWN");
            list.Add(@"        Xxxx");
            list.Add(@"    };");
            return list;
        }

        private static IEnumerable<string> GenerateExchangeCountry(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"namespace Mbst.Trading");
            list.Add(@"{");
            list.Add("    /// <summary>");
            list.Add(@"    /// Exchange countries related to ISO 10383 Market Identifier Codes.");
            list.Add(string.Concat(@"    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @".</para>"));
            list.Add(@"    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includes));
            list.Add(@"    /// </summary>");
            list.Add(@"    public enum ExchangeCountry");
            list.Add(@"    {");
            int total = recordsPerCountry.Keys.Count;
            foreach (var country in recordsPerCountry.Keys)
            {
                if (country == "NoCountry")
                {
                    list.Add(string.Format("        /// <summary>{0}.</summary>", country));
                    list.Add(string.Format("        {0},", country));
                }
            }
            int count = 0;
            --total;
            foreach (var country in recordsPerCountry.Keys)
            {
                if (country != "NoCountry")
                {
                    string c = country;
                    if (c == "Faroeislands")
                        c = "FaroeIslands";
                    else if (c == "Republicofseychelles")
                        c = "RepublicOfSeychelles";
                    list.Add(string.Format("        /// <summary>{0}.</summary>", (recordsPerCountry[country])[0].CountryFull));
                    list.Add(string.Format(++count == total ? "        {0}" : "        {0},", c));
                }
            }
            list.Add(@"    }");
            list.Add(@"}");
            return list;
        }

        private static List<string> GenerateExchangeCountryH(List<string> list, string versionInfo)
        {
            list.Add(@"");
            list.Add(@"    //! Exchange countries related to ISO 10383 Market Identifier Codes.");
            list.Add(string.Concat(@"    //! Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"    //! http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includesH));
            list.Add(@"    enum class ExchangeCountry");
            list.Add(@"    {");
            int total = recordsPerCountry.Keys.Count;
            foreach (var country in recordsPerCountry.Keys)
            {
                if (country == "NoCountry")
                {
                    list.Add(string.Format("        //! {0}", country));
                    list.Add(string.Format("        {0},", country));
                }
            }
            int count = 0;
            --total;
            foreach (var country in recordsPerCountry.Keys)
            {
                if (country != "NoCountry")
                {
                    string c = country;
                    if (c == "Faroeislands")
                        c = "FaroeIslands";
                    else if (c == "Republicofseychelles")
                        c = "RepublicOfSeychelles";
                    list.Add(string.Format("        //! {0}.", (recordsPerCountry[country])[0].CountryFull));
                    list.Add(string.Format(++count == total ? "        {0}" : "        {0},", c));
                }
            }
            list.Add(@"    };");
            return list;
        }

        private static IEnumerable<string> GenerateExchange(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"using System;");
            list.Add(@"using System.Runtime.Serialization;");
            list.Add(@"");
            list.Add(@"namespace Mbst.Trading");
            list.Add(@"{");
            list.Add("    /// <summary>");
            list.Add(@"    /// ISO 10383 Market Identifier Code (MIC) utilities.");
            list.Add(string.Concat(@"    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @".</para>"));
            list.Add(@"    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includes));
            list.Add(@"    /// </summary>");
            list.Add(@"    [DataContract]");
            list.Add(@"    public sealed class Exchange : IComparable<Exchange>, IEquatable<Exchange>");
            list.Add(@"    {");
            list.Add(@"        #region Members and accessors");
            list.Add(@"        #region Mic");
            list.Add(@"        [DataMember]");
            list.Add(@"        private readonly ExchangeMic mic = ExchangeMic.Xams;");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// ISO 10383 Market Identifier Code (MIC).");
            list.Add(@"        /// </summary>");
            list.Add(@"        public ExchangeMic Mic => mic;");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region TimeZone");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The time zone.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public TimeSpan TimeZone");
            list.Add(@"        {");
            list.Add(@"            get");
            list.Add(@"            {");
            list.Add(@"                switch (mic)");
            list.Add(@"                {");
            foreach (var v in timeZoneCountryCodes)
            {
                foreach (var code in v.CountryCodes)
                {
                    if (recordsPerCountryCode.ContainsKey(code))
                    {
                        list.Add(string.Format("                    // {0}, {1}", code, (recordsPerCountryCode[code])[0].CountryFull));
                        foreach (var r in (recordsPerCountryCode[code]))
                        {
                            if (!(code == "PT" && r.IsEuronext))
                                list.Add(string.Format("                    case ExchangeMic.{0}:", r.Mic));
                        }
                    }
                    else
                    {
                        if (Properties.Settings.Default.IncludeOnly.Count <= 0)
                            list.Add(string.Format("                    // {0} -- NOT FOUND!!!", code));
                    }
                }
                list.Add(string.Format("                        return new TimeSpan({0});", v.TimeSpan));
            }
            list.Add(string.Format("                    // PT, {0}", (recordsPerCountryCode["PT"])[0].CountryFull));
            foreach (var r in recordsPerCountryCode["PT"])
            {
                if (r.IsEuronext)
                    list.Add(string.Format("                    case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                        return new TimeSpan(1, 0, 0);");
            list.Add(@"                }");
            list.Add(@"                return new TimeSpan(0L);");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EuronextMep");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The Euronext Market Entry Place (MEP) symbol of this exchange.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public string EuronextMep => MicToEuronextMep(mic);");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EuronextMepNumber");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The Euronext Market Entry Place (MEP) number for this exchange.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public int EuronextMepNumber => MicToEuronextMepNumber(mic);");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region IsEuronext");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Is this exchange belongs to the Euronext family.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public bool IsEuronext => IsEuronextMic(mic);");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region Country");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The country.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public ExchangeCountry Country");
            list.Add(@"        {");
            list.Add(@"            get");
            list.Add(@"            {");
            list.Add(@"                switch (mic)");
            list.Add(@"                {");
            foreach (var kvp in recordsPerCountry)
            {
                if (kvp.Value[0].Country == "NoCountry")
                    continue;
                foreach (var r in kvp.Value)
                    list.Add(string.Format("                    case ExchangeMic.{0}:", r.Mic));
                string c = kvp.Value[0].Country;
                if (c == "Faroeislands")
                    c = "FaroeIslands";
                else if (c == "Republicofseychelles")
                    c = "RepublicOfSeychelles";
                list.Add(string.Format("                        return ExchangeCountry.{0};", c));
            }
            list.Add(@"                }");
            list.Add(@"                return ExchangeCountry.NoCountry;");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region Construction");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Constructs a new instance of this class.");
            list.Add(@"        /// </summary>");
            list.Add(@"        public Exchange()");
            list.Add(@"        {");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Constructs a new instance of this class.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""mic"">The MIC.</param>");
            list.Add(@"        public Exchange(ExchangeMic mic)");
            list.Add(@"        {");
            list.Add(@"            this.mic = mic;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Constructs a new instance of this class.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""euronextMic"">The Euronext MIC.</param>");
            list.Add(@"        public Exchange(EuronextMic euronextMic)");
            list.Add(@"            : this(EuronextToMic(euronextMic))");
            list.Add(@"        {");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EuronextToMic");
            list.Add(@"        private static ExchangeMic EuronextToMic(EuronextMic euronextMic)");
            list.Add(@"        {");
            list.Add(@"            switch (euronextMic)");
            list.Add(@"            {");
            foreach (var r in records)
            {
                if (r.IsEuronext)
                    list.Add(string.Format("                case EuronextMic.{0}: return ExchangeMic.{0};", r.Mic));
            }
            list.Add(@"                default: return ExchangeMic.Xxxx;");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region IsEuronext");
            list.Add(@"        private static bool IsEuronextMic(ExchangeMic mic)");
            list.Add(@"        {");
            list.Add(@"            switch (mic)");
            list.Add(@"            {");
            foreach (var r in records)
            {
                if (r.IsEuronext)
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return true;");
            list.Add(@"                default:");
            list.Add(@"                    return false;");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region MicToEuronextMep");
            list.Add(@"        private static string MicToEuronextMep(ExchangeMic mic)");
            list.Add(@"        {");
            list.Add(@"            switch (mic)");
            list.Add(@"            {");
            list.Add(@"                // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "NL")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return ""AMS"";");
            list.Add(@"");
            list.Add(@"                // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "BE")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return ""BRU"";");
            list.Add(@"");
            list.Add(@"                // Should contain: Xlis, Enxl, Mfox, Wqxl.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "PT")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return ""LIS"";");
            list.Add(@"");
            list.Add(@"                // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "FR")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return ""PAR"";");
            list.Add(@"");
            list.Add(@"                // Should contain: Xldn.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "GB")
                    list.Add(string.Format("                //case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                //    return ""LDN"";");
            list.Add(@"");
            list.Add(@"                default:");
            list.Add(@"                    return ""OTHER"";");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region MicToEuronextMepNumber");
            list.Add(@"        private static int MicToEuronextMepNumber(ExchangeMic mic)");
            list.Add(@"        {");
            list.Add(@"            switch (mic)");
            list.Add(@"            {");
            list.Add(@"                // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "NL")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return 2; // AMS");
            list.Add(@"");
            list.Add(@"                // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "BE")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return 3; // BRU");
            list.Add(@"");
            list.Add(@"                // Should contain: Xlis, Enxl, Mfox, Wqxl.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "PT")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return 5; // LIS");
            list.Add(@"");
            list.Add(@"                // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "FR")
                    list.Add(string.Format("                case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                    return 1; // PAR");
            list.Add(@"");
            list.Add(@"                // Should contain: Xldn.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "GB")
                    list.Add(string.Format("                //case ExchangeMic.{0}:", r.Mic));
            }
            list.Add(@"                //    return ?; // LDN");
            list.Add(@"");
            list.Add(@"                default:");
            list.Add(@"                    return 6; // OTHER");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region IComparable");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// IComparable&lt;Exchange&gt; implementation.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""other"">The other instance to compare.</param>");
            list.Add(@"        /// <returns>The result of comparison.</returns>");
            list.Add(@"        public int CompareTo(Exchange other)");
            list.Add(@"        {");
            list.Add(@"            object obj = other;");
            list.Add(@"            if (null == obj)");
            list.Add(@"                return -1;");
            list.Add(@"            return (int)other.mic - (int)mic;");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region IEquatable");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// IEquatable&lt;Exchange&gt; implementation. Determines whether the specified instances are considered equal.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""other"">The other instance to compare.</param>");
            list.Add(@"        /// <returns>True if instances are equal, false otherwise.</returns>");
            list.Add(@"        public bool Equals(Exchange other)");
            list.Add(@"        {");
            list.Add(@"            object obj = other;");
            list.Add(@"            if (null == obj)");
            list.Add(@"                return false;");
            list.Add(@"            return other.mic == mic;");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region Overrides");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Serves as a hash function for a particular type.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <returns>The hash code.</returns>");
            list.Add(@"        public override int GetHashCode()");
            list.Add(@"        {");
            list.Add(@"            return (int)mic;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Determines whether the specified instances are considered equal.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""obj"">The object to compare with this object.</param>");
            list.Add(@"        /// <returns>True if objects are equal, false if not.</returns>");
            list.Add(@"        public override bool Equals(object obj)");
            list.Add(@"        {");
            list.Add(@"            var other = obj as Exchange;");
            list.Add(@"            return null != other && mic == other.mic;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// Returns the string that represents this object.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <returns>Returns the string that represents this object.</returns>");
            list.Add(@"        public override string ToString()");
            list.Add(@"        {");
            list.Add(@"            return mic.ToString();");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The <c>==</c> operator.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""object1"">The first object.</param>");
            list.Add(@"        /// <param name=""object2"">The second object.</param>");
            list.Add(@"        /// <returns>Boolean specifying the equality relationship.</returns>");
            list.Add(@"        public static bool operator ==(Exchange object1, Exchange object2)");
            list.Add(@"        {");
            list.Add(@"            object obj1 = object1;");
            list.Add(@"            object obj2 = object2;");
            list.Add(@"            if (null != obj1)");
            list.Add(@"                return null != obj2 && object1.mic == object2.mic;");
            list.Add(@"            return null == obj2;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The <c>!=</c> operator.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""object1"">The first object.</param>");
            list.Add(@"        /// <param name=""object2"">The second object.</param>");
            list.Add(@"        /// <returns>Boolean specifying the inequality relationship.</returns>");
            list.Add(@"        public static bool operator !=(Exchange object1, Exchange object2)");
            list.Add(@"        {");
            list.Add(@"            object obj1 = object1;");
            list.Add(@"            object obj2 = object2;");
            list.Add(@"            if (null != obj1)");
            list.Add(@"                return null == obj2 || object1.mic != object2.mic;");
            list.Add(@"            return null != obj2;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The <c>&lt;</c> operator.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""object1"">The first object.</param>");
            list.Add(@"        /// <param name=""object2"">The second object.</param>");
            list.Add(@"        /// <returns>Boolean specifying the less than relationship.</returns>");
            list.Add(@"        public static bool operator <(Exchange object1, Exchange object2)");
            list.Add(@"        {");
            list.Add(@"            object obj1 = object1;");
            list.Add(@"            object obj2 = object2;");
            list.Add(@"            if (null != obj1)");
            list.Add(@"                return null != obj2 && object1.mic < object2.mic;");
            list.Add(@"            return false;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// The <c>&gt;</c> operator.");
            list.Add(@"        /// </summary>");
            list.Add(@"        /// <param name=""object1"">The first object.</param>");
            list.Add(@"        /// <param name=""object2"">The second object.</param>");
            list.Add(@"        /// <returns>Boolean specifying the greater than relationship.</returns>");
            list.Add(@"        public static bool operator >(Exchange object1, Exchange object2)");
            list.Add(@"        {");
            list.Add(@"            object obj1 = object1;");
            list.Add(@"            object obj2 = object2;");
            list.Add(@"            if (null != obj1)");
            list.Add(@"                return null == obj2 || object1.mic > object2.mic;");
            list.Add(@"            return false;");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"    }");
            list.Add(@"}");
            return list;
        }

        private static List<string> GenerateExchangeH(List<string> list, string versionInfo)
        {
            list.Add(@"");
            list.Add(@"    //! ISO 10383 Market Identifier Code (MIC) utilities.");
            list.Add(string.Concat(@"    //! Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"    //! http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includesH));
            list.Add(@"    class Exchange");
            list.Add(@"    {");
            list.Add(@"    public:");
            list.Add(@"        //! Returns the ISO 10383 Market Identifier Code (MIC) of this exchange.");
            list.Add(@"        ExchangeMic mic() const noexcept { return mic_; }");
            list.Add(@"");
            list.Add(@"        //! Sets the ISO 10383 Market Identifier Code (MIC) of this exchange.");
            list.Add(@"        //! \param mic The Market Identifier Code to set.");
            list.Add(@"        void mic(ExchangeMic mic) noexcept { mic_ = mic; }");
            list.Add(@"");
            list.Add(@"        //! Sets the Euronext Market Identifier Code (MIC) of this exchange.");
            list.Add(@"        //! \param euronextMic The Euronext Market Identifier Code (MIC).");
            list.Add(@"        void mic(EuronextMic euronextMic) noexcept { mic_ = euronextToMic(euronextMic); }");
            list.Add(@"");
            list.Add(@"        //! Returns the ISO 10383 Market Identifier Code (MIC) from a given Euronext exchange MIC.");
            list.Add(@"        //! \param euronextMic The  Euronext exchange Market Identifier Code.");
            list.Add(@"        static ExchangeMic euronextToMic(EuronextMic euronextMic) noexcept;");
            list.Add(@"");
            list.Add(@"        //! The time zone offset in minutes.");
            list.Add(@"        int timeZoneMinutes() const noexcept;");
            list.Add(@"");
            list.Add(@"        //! The country code of this exchange.");
            list.Add(@"        ExchangeCountry country() const noexcept;");
            list.Add(@"");
            list.Add(@"        //! Is this exchange belongs to the Euronext family.");
            list.Add(@"        bool isEuronext() const noexcept;");
            list.Add(@"");
            list.Add(@"        //! The Euronext Market Entry Place (MEP) symbol of this exchange.");
            list.Add(@"        const char* euronextMep() const noexcept;");
            list.Add(@"");
            list.Add(@"        //! The Euronext Market Entry Place (MEP) number for this exchange.");
            list.Add(@"        int euronextMepNumber() const noexcept;");
            list.Add(@"");
            list.Add(@"        //! Constructs a new instance of the class.");
            list.Add(@"        //! \param mic The ISO 10383 Market Identifier Code (MIC).");
            list.Add(@"        explicit Exchange(ExchangeMic mic) : mic_{ mic } {}");
            list.Add(@"");
            list.Add(@"        //! Constructs a new instance of the class.");
            list.Add(@"        //! \param euronextMic The Euronext Market Identifier Code (MIC).");
            list.Add(@"        explicit Exchange(EuronextMic euronextMic) : mic_{ euronextToMic(euronextMic) } {}");
            list.Add(@"");
            list.Add(@"        //! Constructs a new instance of the class.");
            list.Add(@"        Exchange() : mic_{ static_cast<ExchangeMic>(0) } {}");
            list.Add(@"");
            list.Add(@"        Exchange(const Exchange& other) = default;");
            list.Add(@"        Exchange(Exchange&& other) = default;");
            list.Add(@"        Exchange& operator=(const Exchange& other) = default;");
            list.Add(@"        Exchange& operator=(Exchange&& other) = default;");
            list.Add(@"        ~Exchange() {}");
            list.Add(@"");
            list.Add(@"    private:");
            list.Add(@"        ExchangeMic mic_;");
            list.Add(@"    };");
            return list;
        }

        private static List<string> GenerateExchangeCpp(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"#include ""trading/exchange/exchange.h""");
            list.Add(@"");
            list.Add(@"// This file is generated automatically.");
            list.Add(@"");
            list.Add(@"namespace mbsl { namespace trading");
            list.Add(@"{");
            list.Add(string.Concat(@"    // Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"    // http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includesH));
            list.Add(@"");
            list.Add(@"    int Exchange::timeZoneMinutes() const noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (mic_)");
            list.Add(@"        {");
            foreach (var v in timeZoneCountryCodes)
            {
                foreach (var code in v.CountryCodes)
                {
                    if (recordsPerCountryCode.ContainsKey(code))
                    {
                        list.Add(string.Format("        // {0}, {1}", code, (recordsPerCountryCode[code])[0].CountryFull));
                        foreach (var r in (recordsPerCountryCode[code]))
                        {
                            if (!(code == "PT" && r.IsEuronext))
                                list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
                        }
                    }
                    else
                    {
                        if (Properties.Settings.Default.IncludeOnly.Count <= 0)
                            list.Add(string.Format("        // {0} -- NOT FOUND!!!", code));
                    }
                }
                list.Add(string.Format("            return {0};", v.TimeSpanMinutes));
            }
            list.Add(string.Format("        // PT, {0}", (recordsPerCountryCode["PT"])[0].CountryFull));
            foreach (var r in recordsPerCountryCode["PT"])
            {
                if (r.IsEuronext)
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return 60;");
            list.Add(@"        default:");
            list.Add(@"            return 0;");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    ExchangeCountry Exchange::country() const noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (mic_)");
            list.Add(@"        {");
            foreach (var kvp in recordsPerCountry)
            {
                if (kvp.Value[0].Country == "NoCountry")
                    continue;
                foreach (var r in kvp.Value)
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
                string c = kvp.Value[0].Country;
                if (c == "Faroeislands")
                    c = "FaroeIslands";
                else if (c == "Republicofseychelles")
                    c = "RepublicOfSeychelles";
                list.Add(string.Format("            return ExchangeCountry::{0};", c));
            }
            list.Add(@"        default:");
            list.Add(@"            return ExchangeCountry::NoCountry;");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    ExchangeMic Exchange::euronextToMic(EuronextMic euronextMic) noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (euronextMic)");
            list.Add(@"        {");
            foreach (var r in records)
            {
                if (r.IsEuronext)
                    list.Add(string.Format("        case EuronextMic::{0}: return ExchangeMic::{0};", r.Mic));
            }
            list.Add(@"        default: return ExchangeMic::Xxxx;");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    bool Exchange::isEuronext() const noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (mic_)");
            list.Add(@"        {");
            foreach (var r in records)
            {
                if (r.IsEuronext)
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return true;");
            list.Add(@"        default:");
            list.Add(@"            return false;");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    static const char* ams{""AMS""};");
            list.Add(@"    static const char* bru{""BRU""};");
            list.Add(@"    //static const char* ldn{""LDN""};");
            list.Add(@"    static const char* lis{""LIS""};");
            list.Add(@"    static const char* par{""PAR""};");
            list.Add(@"    static const char* other{""OTHER""};");
            list.Add(@"");
            list.Add(@"    const char* Exchange::euronextMep() const noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (mic_)");
            list.Add(@"        {");
            list.Add(@"        // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "NL")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return ams;");
            list.Add(@"");
            list.Add(@"        // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "BE")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return bru;");
            list.Add(@"");
            list.Add(@"        // Should contain: Xlis, Enxl, Mfox, Wqxl.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "PT")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return lis;");
            list.Add(@"");
            list.Add(@"        // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "FR")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return par;");
            list.Add(@"");
            list.Add(@"        // Should contain: Xldn.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "GB")
                    list.Add(string.Format("        //case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"        //    return ldn;");
            list.Add(@"");
            list.Add(@"        default:");
            list.Add(@"            return other;");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    int Exchange::euronextMepNumber() const noexcept");
            list.Add(@"    {");
            list.Add(@"        switch (mic_)");
            list.Add(@"        {");
            list.Add(@"        // Should contain: Xams, Alxa, Tnla, Xeuc, Xeue, Xeui, Xhft.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "NL")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return 2; // AMS");
            list.Add(@"");
            list.Add(@"        // Should contain: Xbru, Alxb, Enxb, Mlxb, Tnlb, Vpxb, Xbrd.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "BE")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return 3; // BRU");
            list.Add(@"");
            list.Add(@"        // Should contain: Xlis, Enxl, Mfox, Wqxl.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "PT")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return 5; // LIS");
            list.Add(@"");
            list.Add(@"        // Should contain: Xpar, Alxp, Xmli, Xmat, Xmon.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "FR")
                    list.Add(string.Format("        case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"            return 1; // PAR");
            list.Add(@"");
            list.Add(@"        // Should contain: Xldn.");
            foreach (var r in records)
            {
                if (r.IsEuronext && r.CountryCode == "GB")
                    list.Add(string.Format("        //case ExchangeMic::{0}:", r.Mic));
            }
            list.Add(@"        //    return ?; // LDN");
            list.Add(@"");
            list.Add(@"        default:");
            list.Add(@"            return 6; // OTHER");
            list.Add(@"        }");
            list.Add(@"    }");
            list.Add(@"}}");
            return list;
        }

        private static IEnumerable<string> GenerateExchangeTest(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"using System;");
            list.Add(@"using System.IO;");
            list.Add(@"using System.Runtime.Serialization;");
            list.Add(@"using System.Xml;");
            list.Add(@"using Microsoft.VisualStudio.TestTools.UnitTesting;");
            list.Add(@"");
            list.Add(@"using Mbst.Trading;");
            list.Add(@"");
            list.Add(@"namespace Tests.Trading.Instrument");
            list.Add(@"{");
            list.Add("    /// <summary>");
            list.Add(@"    /// Exchange unit tests.");
            list.Add(string.Concat(@"    /// <para>Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @".</para>"));
            list.Add(@"    /// <para><c>http://www.iso15022.org/MIC/homepageMIC.htm</c></para>");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"    ", includes));
            list.Add(@"    /// </summary>");
            list.Add(@"    [TestClass]");
            list.Add(@"    public class ExchangeTest");
            list.Add(@"    {");
            list.Add(@"        #region ExchangeConstructorTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for Exchange Constructor.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void ExchangeConstructorTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(EuronextMic.Xams);");
            list.Add(@"            Assert.IsTrue(target.Mic == ExchangeMic.Xams);");
            list.Add(@"            target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target.Mic == ExchangeMic.Xpar);");
            list.Add(@"            target = new Exchange();");
            list.Add(@"            Assert.IsTrue(target.Mic == ExchangeMic.Xams);");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region CompareToTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for CompareTo.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void CompareToTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(0 == target.CompareTo(other));");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsFalse(0 == target.CompareTo(other));");
            list.Add(@"            Assert.IsFalse(0 == target.CompareTo(null));");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EqualsTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for Equals.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void EqualsTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            object obj = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target.Equals(obj));");
            list.Add(@"            Assert.IsTrue(target.Equals((Exchange)obj));");
            list.Add(@"            obj = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsFalse(target.Equals(obj));");
            list.Add(@"            obj = null;");
            list.Add(@"            // ReSharper disable once ExpressionIsAlwaysNull");
            list.Add(@"            Assert.IsFalse(target.Equals(obj));");
            list.Add(@"            // ReSharper disable once ExpressionIsAlwaysNull");
            list.Add(@"            Assert.IsFalse(target.Equals((Exchange)obj));");
            list.Add(@"            Assert.IsTrue(target.Equals(target));");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region GetHashCodeTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for GetHashCode.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void GetHashCodeTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target.GetHashCode() == other.GetHashCode());");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsFalse(target.GetHashCode() == other.GetHashCode());");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region ToStringTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for ToStringCode.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void ToStringTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target.ToString() == ExchangeMic.Xpar.ToString());");
            list.Add(@"            target = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsTrue(target.ToString() == ExchangeMic.Xams.ToString());");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region OpEqualityTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for OpEquality.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void OpEqualityTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target == other);");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsFalse(target == other);");
            list.Add(@"            other = null;");
            list.Add(@"            Assert.IsFalse(target == other);");
            list.Add(@"            Assert.IsFalse(other == target);");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region OpGreaterThanTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for OpGreaterThan.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void OpGreaterThanTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.AreEqual(target > other, (int)target.Mic > (int)other.Mic);");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.AreEqual(target > other, (int)target.Mic > (int)other.Mic);");
            list.Add(@"            other = null;");
            list.Add(@"            Assert.IsTrue(target > other);");
            list.Add(@"            Assert.IsFalse(other > target);");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region OpInequalityTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for OpInequality.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void OpInequalityTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsFalse(target != other);");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.IsTrue(target != other);");
            list.Add(@"            other = null;");
            list.Add(@"            Assert.IsTrue(target != other);");
            list.Add(@"            Assert.IsTrue(other != target);");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region OpLessThanTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for OpLessThan.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void OpGreaterLessTest()");
            list.Add(@"        {");
            list.Add(@"            var target = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            var other = new Exchange(ExchangeMic.Xpar);");
            list.Add(@"            Assert.AreEqual(target < other, (int)target.Mic < (int)other.Mic);");
            list.Add(@"            other = new Exchange(ExchangeMic.Xams);");
            list.Add(@"            Assert.AreEqual(target < other, (int)target.Mic < (int)other.Mic);");
            list.Add(@"            other = null;");
            list.Add(@"            Assert.IsFalse(target < other);");
            list.Add(@"            Assert.IsFalse(other < target);");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region CountryTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for Country.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void CountryTest()");
            list.Add(@"        {");
            string c = records[0].Country;
            if (c == "Faroeislands")
                c = "FaroeIslands";
            else if (c == "Republicofseychelles")
                c = "RepublicOfSeychelles";
            list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", records[0].Mic));
            list.Add(string.Format("            Assert.AreEqual(ExchangeCountry.{0}, target.Country, \"{1}\");", c, records[0].Mic));
            for (int i = 1; i < records.Count; ++i)
            {
                c = records[i].Country;
                if (c == "Faroeislands")
                    c = "FaroeIslands";
                else if (c == "Republicofseychelles")
                    c = "RepublicOfSeychelles";
                list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", records[i].Mic));
                list.Add(string.Format("            Assert.AreEqual(ExchangeCountry.{0}, target.Country, \"{1}\");", c, records[i].Mic));
            }
            list.Add("            target = new Exchange(ExchangeMic.Xxxx);");
            list.Add("            Assert.AreEqual(ExchangeCountry.NoCountry, target.Country, \"Xxxx\");");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region TimeZoneTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for TimeZone.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void TimeZoneTest()");
            list.Add(@"        {");
            bool first = true;
            foreach (var v in timeZoneCountryCodes)
            {
                foreach (var code in v.CountryCodes)
                {
                    if (recordsPerCountryCode.ContainsKey(code))
                    {
                        foreach (var record in (recordsPerCountryCode[code]))
                        {
                            if (!(code == "PT" && record.IsEuronext))
                            {
                                if (first)
                                {
                                    list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", record.Mic));
                                    first = false;
                                }
                                else
                                {
                                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                                }
                                list.Add(string.Format("            Assert.AreEqual(new TimeSpan({0}), target.TimeZone, \"{1}\");", v.TimeSpan, record.Mic));
                            }
                        }
                    }
                }
            }
            foreach (var record in recordsPerCountryCode["PT"])
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    list.Add(string.Format("            Assert.AreEqual(new TimeSpan(1, 0, 0), target.TimeZone, \"{0}\");", record.Mic));
                }
            }
            list.Add("            target = new Exchange(ExchangeMic.Xxxx);");
            list.Add("            Assert.AreEqual(new TimeSpan(0, 0, 0), target.TimeZone, \"Xxxx\");");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region IsEuronextTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for IsEuronext.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void IsEuronextTest()");
            list.Add(@"        {");
            first = true;
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    if (first)
                    {
                        list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", record.Mic));
                        first = false;
                    }
                    else
                    {
                        list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    }
                    list.Add(string.Format("            Assert.IsTrue(target.IsEuronext,  \"{0}\");", record.Mic));
                }
            }
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    list.Add(string.Format("            Assert.IsFalse(target.IsEuronext,  \"{0}\");", record.Mic));
                }
            }
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EuronextMepTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for EuronextMep.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void EuronextMepTest()");
            list.Add(@"        {");
            list.Add(@"            const string ams = ""AMS"";");
            list.Add(@"            const string bru = ""BRU"";");
            list.Add(@"            const string par = ""PAR"";");
            list.Add(@"            const string lis = ""LIS"";");
            list.Add(@"            const string oth = ""OTHER"";");
            list.Add(@"");
            first = true;
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    if (first)
                    {
                        list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", record.Mic));
                        first = false;
                    }
                    else
                    {
                        list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    }
                    switch (record.CountryCode.ToUpperInvariant())
                    {
                        case "NL":
                            list.Add(string.Format("            Assert.AreEqual(ams, target.EuronextMep, \"{0}\");", record.Mic)); break;
                        case "BE":
                            list.Add(string.Format("            Assert.AreEqual(bru, target.EuronextMep, \"{0}\");", record.Mic)); break;
                        case "FR":
                            list.Add(string.Format("            Assert.AreEqual(par, target.EuronextMep, \"{0}\");", record.Mic)); break;
                        case "PT":
                            list.Add(string.Format("            Assert.AreEqual(lis, target.EuronextMep, \"{0}\");", record.Mic)); break;
                        default:
                            list.Add(string.Format("            Assert.AreEqual(oth, target.EuronextMep, \"{0}\");", record.Mic)); break;
                    }
                }
            }
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    list.Add(string.Format("            Assert.AreEqual(oth, target.EuronextMep, \"{0}\");", record.Mic));
                }
            }
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region EuronextMepNumberTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for EuronextMepNumber.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void EuronextMepNumberTest()");
            list.Add(@"        {");
            list.Add(@"            const int ams = 2;");
            list.Add(@"            const int bru = 3;");
            list.Add(@"            const int par = 1;");
            list.Add(@"            const int lis = 5;");
            list.Add(@"            const int oth = 6;");
            list.Add(@"");
            first = true;
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    if (first)
                    {
                        list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", record.Mic));
                        first = false;
                    }
                    else
                    {
                        list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    }
                    switch (record.CountryCode.ToUpperInvariant())
                    {
                        case "NL":
                            list.Add(string.Format("            Assert.AreEqual(ams, target.EuronextMepNumber, \"{0}\");", record.Mic)); break;
                        case "BE":
                            list.Add(string.Format("            Assert.AreEqual(bru, target.EuronextMepNumber, \"{0}\");", record.Mic)); break;
                        case "FR":
                            list.Add(string.Format("            Assert.AreEqual(par, target.EuronextMepNumber, \"{0}\");", record.Mic)); break;
                        case "PT":
                            list.Add(string.Format("            Assert.AreEqual(lis, target.EuronextMepNumber, \"{0}\");", record.Mic)); break;
                        default:
                            list.Add(string.Format("            Assert.AreEqual(oth, target.EuronextMepNumber, \"{0}\");", record.Mic)); break;
                    }
                }
            }
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                    list.Add(string.Format("            Assert.AreEqual(oth, target.EuronextMepNumber, \"{0}\");", record.Mic));
                }
            }
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region MicTest");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for Mic.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void MicTest()");
            list.Add(@"        {");
            first = true;
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    if (first)
                    {
                        list.Add(string.Format("            var target = new Exchange(EuronextMic.{0});", record.Mic));
                        first = false;
                    }
                    else
                    {
                        list.Add(string.Format("            target = new Exchange(EuronextMic.{0});", record.Mic));
                    }
                    list.Add(string.Format("            Assert.AreEqual(ExchangeMic.{0}, target.Mic, \"{0}\");", record.Mic));
                }
            }
            list.Add("            target = new Exchange(EuronextMic.Xxxx);");
            list.Add("            Assert.AreEqual(ExchangeMic.Xxxx, target.Mic, \"Xxxx\");");
            list.Add(@"        }");
            /*
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for Mic.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void MicTest2()");
            list.Add(@"        {");
            first = true;
            foreach (var record in records)
            {
                if (first)
                {
                    list.Add(string.Format("            var target = new Exchange(ExchangeMic.{0});", record.Mic));
                    first = false;
                }
                else
                {
                    list.Add(string.Format("            target = new Exchange(ExchangeMic.{0});", record.Mic));
                }
                list.Add(string.Format("            Assert.IsTrue(target.Mic == EuronextToMic(EuronextMic.{0}));", record.Mic));
            }
            list.Add(@"        }");
            */
            list.Add(@"        #endregion");
            list.Add(@"");
            list.Add(@"        #region SerializationTest");
            list.Add(@"        private static void SerializeTo(Exchange instance, string fileName)");
            list.Add(@"        {");
            list.Add(@"            var dcs = new DataContractSerializer(typeof(Exchange), null, 65536, false, true, null);");
            list.Add(@"            using (var fs = new FileStream(fileName, FileMode.Create))");
            list.Add(@"            {");
            list.Add(@"                dcs.WriteObject(fs, instance);");
            list.Add(@"                fs.Close();");
            list.Add(@"            }");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        private static Exchange DeserializeFrom(string fileName)");
            list.Add(@"        {");
            list.Add(@"            var fs = new FileStream(fileName, FileMode.Open);");
            list.Add(@"            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());");
            list.Add(@"            var ser = new DataContractSerializer(typeof(Exchange), null, 65536, false, true, null);");
            list.Add(@"            var instance = (Exchange)ser.ReadObject(reader, true);");
            list.Add(@"            reader.Close();");
            list.Add(@"            fs.Close();");
            list.Add(@"            return instance;");
            list.Add(@"        }");
            list.Add(@"");
            list.Add(@"        /// <summary>");
            list.Add(@"        /// A test for the serialization.");
            list.Add(@"        /// </summary>");
            list.Add(@"        [TestMethod]");
            list.Add(@"        public void SerializationTest()");
            list.Add(@"        {");
            list.Add(@"            var source = new Exchange(ExchangeMic.Xpar);");
            list.Add( "            const string fileName = \"ExchangeTest_1.xml\";");
            list.Add(@"            SerializeTo(source, fileName);");
            list.Add(@"            Exchange target = DeserializeFrom(fileName);");
            list.Add(@"            Assert.IsTrue(target.Mic == ExchangeMic.Xpar);");
            list.Add(@"            Assert.IsTrue(target.IsEuronext);");
            list.Add(@"            //FileInfo fi = new FileInfo(fileName);");
            list.Add(@"            //fi.Delete();");
            list.Add(@"        }");
            list.Add(@"        #endregion");
            list.Add(@"    }");
            list.Add(@"}");
            return list;
        }

        private static IEnumerable<string> GenerateExchangeTestCpp(string versionInfo)
        {
            var list = new List<string>(4096);
            list.Add(@"#include ""mbsl/trading/exchange/exchange.h""");
            list.Add(@"#include ""mbsl/testharness.h""");
            list.Add(@"");
            list.Add(string.Concat(@"// Generated automatically from the list of ISO 10383 Exchange/Market Identifier Codes, ", versionInfo, @"."));
            list.Add(@"// http://www.iso15022.org/MIC/homepageMIC.htm");
            if (Properties.Settings.Default.IncludeOnly.Count > 0)
                list.Add(string.Concat(@"", includesH));
            list.Add(@"");
            list.Add(@"namespace {");
            list.Add(@"");
            list.Add(@"TESTGROUP(""trading::Exchange"")");
            list.Add(@"{");
            list.Add(@"    using Exchange = mbsl::trading::Exchange;");
            list.Add(@"    using ExchangeCountry = mbsl::trading::ExchangeCountry;");
            list.Add(@"    using ExchangeMic = mbsl::trading::ExchangeMic;");
            list.Add(@"    using EuronextMic = mbsl::trading::EuronextMic;");
            list.Add(@"");
            list.Add(@"    TESTCASE(""mic() returns value passed to the constructor"")");
            list.Add(@"    {");
            list.Add(@"        EuronextMic euronext{ EuronextMic::Xpar };");
            list.Add(@"        ExchangeMic expected{ ExchangeMic::Xpar };");
            list.Add(@"        ExchangeMic empty{ static_cast<ExchangeMic>(0) };");
            list.Add(@"");
            list.Add(@"        Exchange exchange0;");
            list.Add(@"        ASSERT_EQUAL(empty, exchange0.mic()) << ""empty"";");
            list.Add(@"");
            list.Add(@"        Exchange exchange1{ expected };");
            list.Add(@"        ASSERT_EQUAL(expected, exchange1.mic()) << ""exchange mic"";");
            list.Add(@"");
            list.Add(@"        Exchange exchange2{ euronext };");
            list.Add(@"        ASSERT_EQUAL(expected, exchange2.mic()) << ""euronext mic"";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""mic() returns value passed via copy constructor"")");
            list.Add(@"    {");
            list.Add(@"        const ExchangeMic expected{ ExchangeMic::Xpar };");
            list.Add(@"        Exchange exchange1{ expected };");
            list.Add(@"");
            list.Add(@"        Exchange exchange2{ exchange1 };");
            list.Add(@"        ASSERT_EQUAL(expected, exchange2.mic()) << ""copy constructor"";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""mic() returns value passed via assignment"")");
            list.Add(@"    {");
            list.Add(@"        const ExchangeMic expected{ ExchangeMic::Xpar };");
            list.Add(@"        const ExchangeMic other{ ExchangeMic::Xams };");
            list.Add(@"");
            list.Add(@"        Exchange exchange2{ other };");
            list.Add(@"        ASSERT_EQUAL(other, exchange2.mic()) << ""before assignment"";");
            list.Add(@"");
            list.Add(@"        Exchange exchange1{ expected };");
            list.Add(@"        exchange2 = exchange1;");
            list.Add(@"        ASSERT_EQUAL(expected, exchange2.mic()) << ""after assignment"";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""mic() returns correct value after mic(ExchangeMic) call"")");
            list.Add(@"    {");
            list.Add(@"        const ExchangeMic expected{ ExchangeMic::Xpar };");
            list.Add(@"        const ExchangeMic initial{ ExchangeMic::Xams };");
            list.Add(@"");
            list.Add(@"        Exchange exchange{ initial };");
            list.Add(@"        exchange.mic(expected);");
            list.Add(@"        ASSERT_EQUAL(expected, exchange.mic()) << ""exchange mic"";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""mic() returns correct value after mic(EuronextMic) call"")");
            list.Add(@"    {");
            list.Add(@"        const ExchangeMic expected{ ExchangeMic::Xpar };");
            list.Add(@"        const EuronextMic euronext{ EuronextMic::Xpar };");
            list.Add(@"        const ExchangeMic initial{ ExchangeMic::Xams };");
            list.Add(@"");
            list.Add(@"        Exchange exchange{ initial };");
            list.Add(@"        exchange.mic(euronext);");
            list.Add(@"        ASSERT_EQUAL(expected, exchange.mic()) << ""euronext mic""; ");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""country() returns correct country of exchange"")");
            list.Add(@"    {");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            string c = records[0].Country;
            if (c == "Faroeislands")
                c = "FaroeIslands";
            else if (c == "Republicofseychelles")
                c = "RepublicOfSeychelles";
            list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", records[0].Mic));
            list.Add(string.Format("        ASSERT_EQUAL(ExchangeCountry::{0}, exchange.country()) << \"{1}\";", c, records[0].Mic));
            for (int i = 1; i < records.Count; ++i)
            {
                c = records[i].Country;
                if (c == "Faroeislands")
                    c = "FaroeIslands";
                else if (c == "Republicofseychelles")
                    c = "RepublicOfSeychelles";
                list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", records[i].Mic));
                list.Add(string.Format("        ASSERT_EQUAL(ExchangeCountry::{0}, exchange.country()) << \"{1}\";", c, records[i].Mic));
            }
            list.Add("        exchange.mic(ExchangeMic::Xxxx);");
            list.Add("        ASSERT_EQUAL(ExchangeCountry::NoCountry, exchange.country()) << \"Xxxx\";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""timeZoneMinutes() returns correct time zone value in minutes"")");
            list.Add(@"    {");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            bool first = true;
            foreach (var v in timeZoneCountryCodes)
            {
                foreach (var code in v.CountryCodes)
                {
                    if (recordsPerCountryCode.ContainsKey(code))
                    {
                        foreach (var record in (recordsPerCountryCode[code]))
                        {
                            if (!(code == "PT" && record.IsEuronext))
                            {
                                if (first)
                                {
                                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                                    first = false;
                                }
                                else
                                {
                                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                                }
                                list.Add(string.Format("        ASSERT_EQUAL({0}, exchange.timeZoneMinutes()) << \"{1}\";", v.TimeSpanMinutes, record.Mic));
                            }
                        }
                    }
                }
            }
            foreach (var record in recordsPerCountryCode["PT"])
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    list.Add(string.Format("        ASSERT_EQUAL(60, exchange.timeZoneMinutes()) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"        exchange.mic(ExchangeMic::Xxxx);");
            list.Add(@"        ASSERT_EQUAL(0, exchange.timeZoneMinutes()) << ""Xxxx"";");
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""isEuronext() returns true for Euronext exchanges"")");
            list.Add(@"    {");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    list.Add(string.Format("        ASSERT_IS_TRUE(exchange.isEuronext()) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""isEuronext() returns false for non-Euronext exchanges"")");
            list.Add(@"    {");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    list.Add(string.Format("        ASSERT_IS_FALSE(exchange.isEuronext()) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""euronextMep() returns correct value"")");
            list.Add(@"    {");
            list.Add(@"        const char* ams{""AMS""};");
            list.Add(@"        const char* bru{""BRU""};");
            list.Add(@"        const char* par{""PAR""};");
            list.Add(@"        const char* lis{""LIS""};");
            list.Add(@"        const char* oth{""OTHER""};");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    switch (record.CountryCode.ToUpperInvariant())
                    {
                        case "NL":
                            list.Add(string.Format("        ASSERT_EQUAL(ams, exchange.euronextMep()) << \"{0}\";", record.Mic));
                            break;
                        case "BE":
                            list.Add(string.Format("        ASSERT_EQUAL(bru, exchange.euronextMep()) << \"{0}\";", record.Mic));
                            break;
                        case "FR":
                            list.Add(string.Format("        ASSERT_EQUAL(par, exchange.euronextMep()) << \"{0}\";", record.Mic));
                            break;
                        case "PT":
                            list.Add(string.Format("        ASSERT_EQUAL(lis, exchange.euronextMep()) << \"{0}\";", record.Mic));
                            break;
                        default:
                            list.Add(string.Format("        ASSERT_EQUAL(oth, exchange.euronextMep()) << \"{0}\";", record.Mic));
                            break;
                    }
                }
            }
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    list.Add(string.Format("        ASSERT_EQUAL(oth, exchange.euronextMep()) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""euronextMepNumber() returns correct value"")");
            list.Add(@"    {");
            list.Add(@"        const int ams{2};");
            list.Add(@"        const int bru{3};");
            list.Add(@"        const int par{1};");
            list.Add(@"        const int lis{5};");
            list.Add(@"        const int oth{6};");
            list.Add(@"        Exchange exchange;");
            list.Add(@"");
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    switch (record.CountryCode.ToUpperInvariant())
                    {
                        case "NL":
                            list.Add(string.Format("        ASSERT_EQUAL(ams, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                            break;
                        case "BE":
                            list.Add(string.Format("        ASSERT_EQUAL(bru, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                            break;
                        case "FR":
                            list.Add(string.Format("        ASSERT_EQUAL(par, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                            break;
                        case "PT":
                            list.Add(string.Format("        ASSERT_EQUAL(lis, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                            break;
                        default:
                            list.Add(string.Format("        ASSERT_EQUAL(oth, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                            break;
                    }
                }
            }
            foreach (var record in records)
            {
                if (!record.IsEuronext)
                {
                    list.Add(string.Format("        exchange.mic(ExchangeMic::{0});", record.Mic));
                    list.Add(string.Format("        ASSERT_EQUAL(oth, exchange.euronextMepNumber()) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"    }");
            list.Add(@"");
            list.Add(@"    TESTCASE(""euronextToMic() returns correct value"")");
            list.Add(@"    {");
            foreach (var record in records)
            {
                if (record.IsEuronext)
                {
                    list.Add(string.Format("        ASSERT_EQUAL(ExchangeMic::{0}, Exchange::euronextToMic(EuronextMic::{0})) << \"{0}\";", record.Mic));
                }
            }
            list.Add(@"        ASSERT_EQUAL(ExchangeMic::Xxxx, Exchange::euronextToMic(EuronextMic::Xxxx)) << ""Xxxx""; ");
            list.Add(@"    }");
            list.Add(@"}");
            list.Add(@"");
            list.Add(@"}");
            return list;
        }

        static void Main(string[] args)
        {
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes {TimeSpan = "1, 0, 0", TimeSpanMinutes = "60", CountryCodes = new List<string>{"AL", "DZ", "AT", "BE", "CZ", "DK", "FR", "DE", "IT", "SK", "ES", "SE", "CH", "NL", "RS", "NG", "PL", "NO", "NA", "ME", "MT", "MK", "LU", "HU", "BA", "HR", "TN", "CM"}});
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "2, 0, 0", TimeSpanMinutes = "120", CountryCodes = new List<string> { "BY", "CY", "EE", "GR", "FI", "IL", "LB", "LV", "LT", "MD", "ZA", "TR", "UA", "RO", "RW", "PS", "JO", "EG", "BG", "SZ", "MZ" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "3, 0, 0", TimeSpanMinutes = "180", CountryCodes = new List<string> { "RU", "QA", "KW", "KE", "IQ", "BH", "UG", "TZ", "SD", "MG", "SY" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "3, 30, 0", TimeSpanMinutes = "210", CountryCodes = new List<string> { "IR" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "4, 0, 0", TimeSpanMinutes = "240", CountryCodes = new List<string> { "AE", "KN", "MU", "AM", "AZ", "OM" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "5, 0, 0", TimeSpanMinutes = "300", CountryCodes = new List<string> { "PK", "UZ" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "5, 30, 0", TimeSpanMinutes = "330", CountryCodes = new List<string> { "LK", "IN" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "5, 45, 0", TimeSpanMinutes = "345", CountryCodes = new List<string> { "NP" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "6, 0, 0", TimeSpanMinutes = "360", CountryCodes = new List<string> { "KG", "KZ" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "7, 0, 0", TimeSpanMinutes = "420", CountryCodes = new List<string> { "SG", "ID", "TH" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "8, 0, 0", TimeSpanMinutes = "480", CountryCodes = new List<string> { "PH", "MN", "MY", "HK", "CN", "BD", "TW" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "9, 0, 0", TimeSpanMinutes = "540", CountryCodes = new List<string> { "KR", "JP" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "10, 0, 0", TimeSpanMinutes = "600", CountryCodes = new List<string> { "AU" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "12, 0, 0", TimeSpanMinutes = "720", CountryCodes = new List<string> { "FJ", "NZ" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-1, 0, 0", TimeSpanMinutes = "-60", CountryCodes = new List<string> { "CV" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-3, 0, 0", TimeSpanMinutes = "-180", CountryCodes = new List<string> { "PY", "BR", "UY", "AR" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-4, 0, 0", TimeSpanMinutes = "-240", CountryCodes = new List<string> { "PG", "CI", "DO", "CL", "BO", "BM" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-4, -30, 0", TimeSpanMinutes = "-270", CountryCodes = new List<string> { "VE", "TT" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-5, 0, 0", TimeSpanMinutes = "-300", CountryCodes = new List<string> { "BB", "KY", "PA", "PE", "JM", "BS", "EC", "CO", "CA", "US" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-6, 0, 0", TimeSpanMinutes = "-360", CountryCodes = new List<string> { "HN", "MX", "NI", "GT", "SV", "CR" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "-11, 0, 0", TimeSpanMinutes = "-660", CountryCodes = new List<string> { "VU" } });
            timeZoneCountryCodes.Add(new TimeZoneCountryCodes { TimeSpan = "0, 0, 0", TimeSpanMinutes = "0", CountryCodes = new List<string> { "GB", "IE", "IS", "GH", "MA", "GG", "PT" } });

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: MicImport csvFileName [date]");
                Console.WriteLine("The [csvFileName] is formatted as ISO10383_MIC_vFEB20161_1.csv");
                Console.WriteLine("The [date] is formatted as 2016-03-21");
            }
            string version = ExtractVersion(args[0], args.Length > 1 ? args[1] : null);
            if (!Directory.Exists(version))
                Directory.CreateDirectory(version);
            string dir = string.Concat(version, "\\");
            ReadInput(args[0]);

            IEnumerable<string> list = GenerateExchangeMic(version);
            WriteFile(list, dir + "ExchangeMic.cs");
            list = GenerateEuronextMic(version);
            WriteFile(list, dir + "EuronextMic.cs");
            list = GenerateExchangeCountry(version);
            WriteFile(list, dir + "ExchangeCountry.cs");
            list = GenerateExchange(version);
            WriteFile(list, dir + "Exchange.cs");
            list = GenerateExchangeTest(version);
            WriteFile(list, dir + "ExchangeTest.cs");

            list = GenerateExchangeHeaderH();
            list = GenerateExchangeMicH((List<string>)list, version);
            list = GenerateEuronextMicH((List<string>)list, version);
            list = GenerateExchangeCountryH((List<string>)list, version);
            list = GenerateExchangeH((List<string>)list, version);
            list = GenerateExchangeFooterH((List<string>)list);
            WriteFile(list, dir + "exchange.h");//
            list = GenerateExchangeCpp(version);
            WriteFile(list, dir + "exchange.cpp");
            list = GenerateExchangeTestCpp(version);
            WriteFile(list, dir + "exchangeTests.cpp");
        }
    }
}
