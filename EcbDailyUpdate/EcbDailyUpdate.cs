using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Net;
using Mbh5;
using System.Globalization;

namespace EcbDailyUpdate
{
    class EcbDailyUpdate
    {
        // private readonly Dictionary<string, string> currencyNameDictionary = new Dictionary<string, string>();
        private readonly Dictionary<string, ScalarData> currencyDictionary = new Dictionary<string, ScalarData>();
        private readonly string root = Properties.Settings.Default.RepositoryRoot;
        private readonly string h5File = Properties.Settings.Default.RepositoryFile;
        private readonly string symbolFormat = Properties.Settings.Default.SymbolFormat;
        private IXmlLineInfo xmlLineInfo;

        public EcbDailyUpdate()
        {
            // currencyNameDictionary.Add("AUD", "Australian dollar");
            // currencyNameDictionary.Add("BGN", "Bulgarian lev");
            // currencyNameDictionary.Add("CAD", "Canadian dollar");
            // currencyNameDictionary.Add("CHF", "Swiss franc");
            // currencyNameDictionary.Add("CNY", "Chinese yuan renminbi");
            // currencyNameDictionary.Add("CZK", "Czech koruna");
            // currencyNameDictionary.Add("DKK", "Danish krone");
            // currencyNameDictionary.Add("EEK", "Estonian kroon");
            // currencyNameDictionary.Add("GBP", "Pound sterling");
            // currencyNameDictionary.Add("HKD", "Hong Kong dollar");
            // currencyNameDictionary.Add("HRK", "Croatian kuna");
            // currencyNameDictionary.Add("HUF", "Hungarian forint");
            // currencyNameDictionary.Add("ISK", "Icelandic krona, the last rate was published on 3 Dec 2008");
            // currencyNameDictionary.Add("IDR", "Indonesian rupiah");
            // currencyNameDictionary.Add("JPY", "Japanese yen");
            // currencyNameDictionary.Add("KRW", "South Korean won");
            // currencyNameDictionary.Add("LTL", "Lithuanian litas");
            // currencyNameDictionary.Add("LVL", "Latvian lats");
            // currencyNameDictionary.Add("MYR", "Malaysian ringgit");
            // currencyNameDictionary.Add("NOK", "Norwegian krone");
            // currencyNameDictionary.Add("NZD", "New Zealand dollar");
            // currencyNameDictionary.Add("PHP", "Philippine peso");
            // currencyNameDictionary.Add("PLN", "Polish zloty");
            // currencyNameDictionary.Add("RON", "New Romanian leu");
            // currencyNameDictionary.Add("RUB", "Russian rouble");
            // currencyNameDictionary.Add("SEK", "Swedish krona");
            // currencyNameDictionary.Add("SGD", "Singapore dollar");
            // currencyNameDictionary.Add("SKK", "Slovak koruna");
            // currencyNameDictionary.Add("THB", "Thai baht");
            // currencyNameDictionary.Add("TRY", "New Turkish lira");
            // currencyNameDictionary.Add("USD", "US dollar");
            // currencyNameDictionary.Add("ZAR", "South African rand");
            // currencyNameDictionary.Add("BRL", "Brazilian real");
            // currencyNameDictionary.Add("MXN", "Mexican peso");
            // currencyNameDictionary.Add("INR", "Indian rupee");
            // currencyNameDictionary.Add("CYP", "Cypriot pound, replaced with the EUR 2008");
            // currencyNameDictionary.Add("MTL", "Maltese lira, replaced with the EUR 2008");
            // currencyNameDictionary.Add("ROL", "Romanian leu, replaced with RON 2005");
            // currencyNameDictionary.Add("SIT", "Slovenian tolar, replaced with the EUR 2007");
            // currencyNameDictionary.Add("TRL", "Turkish lira, replaced with TRY 2005");
        }

        private string PathFromCurrencySymbol(string currencySymbol)
        {
            return string.Concat(root, SymbolFromCurrencySymbol(currencySymbol));
        }
        private string SymbolFromCurrencySymbol(string currencySymbol)
        {
            return string.Format(symbolFormat, currencySymbol);
        }
        private string NoAttribute(string attribute)
        {
            return $"line {xmlLineInfo.LineNumber}: {attribute} attribute not found";
        }

        public void Import()
        {
            using (Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache))
            {
                if (Properties.Settings.Default.HistoryFetch)
                    ImportUrl(Properties.Settings.Default.HistoryURL, repository);
                if (Properties.Settings.Default.Last90DaysFetch)
                    ImportUrl(Properties.Settings.Default.Last90DaysURL, repository);
                if (Properties.Settings.Default.DailyFetch)
                    ImportUrl(Properties.Settings.Default.DailyURL, repository);
                currencyDictionary.Keys.ToList().ForEach(s =>
                {
                    currencyDictionary[s].Flush();
                    currencyDictionary[s].Close();
                });
                currencyDictionary.Clear();
            }
        }

        private long TimeToTicks(string input)
        {
            string[] s = input.Split('-');
            int year = int.Parse(s[0], CultureInfo.InvariantCulture);
            int month = int.Parse(s[1], CultureInfo.InvariantCulture);
            int day = int.Parse(s[2], CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, 0, 0, 0).Ticks;
        }

        private void ImportUrl(string url, Repository repository)
        {
            var scalar = new Scalar();
            var scalarList = new List<Scalar>();
            Trace.TraceInformation("Downloading URL " + url);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = WebRequest.DefaultWebProxy;
            // DefaultCredentials represents the system credentials for the current 
            // security context in which the application is running. For a client-side 
            // application, these are usually the Windows credentials 
            // (user name, password, and domain) of the user running the application. 
            webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webRequest.UserAgent = Properties.Settings.Default.UserAgent;
            webRequest.Timeout = Properties.Settings.Default.DownloadTimeout;
            WebResponse webResponse = webRequest.GetResponse();
            Stream responseStream = webResponse.GetResponseStream();
            if (null == responseStream)
            {
                Trace.TraceError("Received null response stream.");
                return;
            }
            // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
            using (XmlReader xmlReader = XmlReader.Create(responseStream))
            {
                xmlLineInfo = (IXmlLineInfo)xmlReader;
                while (xmlReader.Read())
                {
                    if (XmlNodeType.Element == xmlReader.NodeType)
                    {
                        // <Cube>
                        //     <Cube time="2008-07-08">
                        //         <Cube currency="USD" rate="1.5687"/>
                        //         ...
                        //         <Cube currency="JPY" rate="167.96"/>
                        //     </Cube>
                        //     <Cube time="2008-07-07">
                        //     ...
                        //     </Cube>
                        // </Cube>
                        if ("Cube" == xmlReader.LocalName)
                        {
                            while (xmlReader.Read())
                            {
                                if (XmlNodeType.Element == xmlReader.NodeType)
                                {
                                    if ("Cube" == xmlReader.LocalName)
                                    {
                                        string date = xmlReader.GetAttribute("time");
                                        if (null == date)
                                            throw new ArgumentException(NoAttribute("time"));
                                        long dateValue = TimeToTicks(date);
                                        while (xmlReader.Read())
                                        {
                                            if (XmlNodeType.Element == xmlReader.NodeType)
                                            {
                                                if ("Cube" == xmlReader.LocalName)
                                                {
                                                    string currency = xmlReader.GetAttribute("currency");
                                                    if (null == currency)
                                                        throw new ArgumentException(NoAttribute("currency"));
                                                    string rate = xmlReader.GetAttribute("rate");
                                                    if (null == rate)
                                                        throw new ArgumentException(NoAttribute("rate"));
                                                    if (!currencyDictionary.TryGetValue(currency, out var scalarData))
                                                    {
                                                        using (Instrument instrument = repository.Open(PathFromCurrencySymbol(currency), true))
                                                        {
                                                            scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);
                                                            currencyDictionary.Add(currency, scalarData);
                                                        }
                                                    }
                                                    if (null != scalarData)
                                                    {
                                                        scalar.dateTimeTicks = dateValue;
                                                        double rateValue = double.Parse(rate, CultureInfo.InvariantCulture);
                                                        scalar.value = rateValue;
                                                        scalarList.Clear();
                                                        scalarList.Add(scalar);
                                                        scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                                                    }
                                                }
                                            }
                                            else if (XmlNodeType.EndElement == xmlReader.NodeType)
                                                break;
                                        }
                                    }
                                }
                                else if (XmlNodeType.EndElement == xmlReader.NodeType)
                                    break;
                            }
                        }
                    }
                }
                // Explicitly call Close on the XmlReader to reduce strain on the GC.
                xmlReader.Close();
            }
        }
    }
}
