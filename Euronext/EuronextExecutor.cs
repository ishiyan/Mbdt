using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;

namespace mbdt.Euronext
{
    static class EuronextExecutor
    {
        #region Instrument
        /// <summary>
        /// Encapsulates selected instrument attributes.
        /// </summary>
        [DataContract]
        public class Instrument
        {
            #region Members and accessors
            #region Isin
            /// <summary>
            /// The International Securities Identifying Number.
            /// </summary>
            [DataMember]
            public string Isin;
            #endregion

            #region Mep
            /// <summary>
            /// The Market Entry Place.
            /// </summary>
            [DataMember]
            public string Mep;
            #endregion

            #region Mic
            /// <summary>
            /// The MIC.
            /// </summary>
            [DataMember]
            public string Mic;
            #endregion

            #region Name
            /// <summary>
            /// The name.
            /// </summary>
            [DataMember]
            public string Name;
            #endregion

            #region Symbol
            /// <summary>
            /// The symbol (ticker).
            /// </summary>
            [DataMember]
            public string Symbol;
            #endregion

            #region SecurityType
            /// <summary>
            /// The type of security.
            /// </summary>
            [DataMember]
            public string SecurityType;
            #endregion

            #region MillisecondsSince1970
            /// <summary>
            /// Milliseconds since 01/Jan/1970.
            /// </summary>
            [DataMember]
            public long MillisecondsSince1970;
            #endregion

            #region File
            /// <summary>
            /// The relative xml file path.
            /// </summary>
            [DataMember]
            public string File;
            #endregion
            #endregion
        }
        #endregion

        #region ConsecutiveFailInfo
        /// <summary>
        /// Encapsulates the consecutive fail info.
        /// </summary>
        public class ConsecutiveFailInfo
        {
            #region Members and accessors
            #region Count
            /// <summary>
            /// The count.
            /// </summary>
            public int Count;
            #endregion

            #region LimitReached
            /// <summary>
            /// Indicates that the limit was reached.
            /// </summary>
            public bool LimitReached;
            #endregion
            #endregion
        }
        #endregion

        #region Members and accessors
        private static readonly DateTime Year1970 = new DateTime(1970, 1, 1);

        #region xmlReaderSettings
        /// <summary>
        /// A xml reader settings template.
        /// </summary>
        static private readonly XmlReaderSettings xmlReaderSettings;
        #endregion
        #endregion

        #region Construction
        /// <summary>
        /// Static constructor.
        /// </summary>
        static EuronextExecutor()
        {
            xmlReaderSettings = new XmlReaderSettings {CheckCharacters = false, CloseInput = true, ConformanceLevel = ConformanceLevel.Auto, IgnoreComments = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true, ValidationType = ValidationType.None};
        }
        #endregion

        #region State
        /// <summary>
        /// Passes arguments to a thread.
        /// </summary>
        private class State
        {
            #region Members and accessors
            #region FileList
            /// <summary>
            /// The list of files.
            /// </summary>
            public readonly List<string> FileList;
            #endregion

            #region InstrumentList
            /// <summary>
            /// The Instrument list.
            /// </summary>
            public readonly List<Instrument> InstrumentList;
            #endregion

            #region EuronextInstrumentContextList
            /// <summary>
            /// The EuronextInstrumentContext list.
            /// </summary>
            public readonly List<EuronextInstrumentContext> EuronextInstrumentContextList;
            #endregion

            #region ManualResetEvent
            /// <summary>
            /// The ManualResetEvent.
            /// </summary>
            public readonly ManualResetEvent ManualResetEvent;
            #endregion

            #region EuronextInstrumentContext
            /// <summary>
            /// The EuronextInstrumentContext.
            /// </summary>
            public EuronextInstrumentContext EuronextInstrumentContext;
            #endregion

            #region ConsecutiveFailInfo
            /// <summary>
            /// The ConsecutiveFailInfo.
            /// </summary>
            public readonly ConsecutiveFailInfo ConsecutiveFailInfo;
            #endregion
            #endregion

            #region Construction
            /// <summary>
            /// Constructs an instabce of the class.
            /// </summary>
            /// <param name="instrumentList">The Instrument list.</param>
            /// <param name="manualResetEvent">The ManualResetEvent.</param>
            /// <param name="euronextInstrumentContext">The EuronextInstrumentContext.</param>
            /// <param name="consecutiveFailInfo">The ConsecutiveFailInfo.</param>
            public State(List<Instrument> instrumentList, ManualResetEvent manualResetEvent, EuronextInstrumentContext euronextInstrumentContext, ConsecutiveFailInfo consecutiveFailInfo)
            {
                FileList = null;
                InstrumentList = instrumentList;
                EuronextInstrumentContextList = null;
                ManualResetEvent = manualResetEvent;
                EuronextInstrumentContext = euronextInstrumentContext;
                ConsecutiveFailInfo = consecutiveFailInfo;
            }

            /// <summary>
            /// Constructs an instabce of the class.
            /// </summary>
            /// <param name="fileList">The list of files.</param>
            /// <param name="manualResetEvent">The ManualResetEvent.</param>
            /// <param name="euronextInstrumentContext">The EuronextInstrumentContext.</param>
            public State(List<string> fileList, ManualResetEvent manualResetEvent, EuronextInstrumentContext euronextInstrumentContext)
            {
                FileList = fileList;
                InstrumentList = null;
                EuronextInstrumentContextList = null;
                ManualResetEvent = manualResetEvent;
                EuronextInstrumentContext = euronextInstrumentContext;
                ConsecutiveFailInfo = null;
            }

            /// <summary>
            /// Constructs an instabce of the class.
            /// </summary>
            /// <param name="contextList">The EuronextInstrumentContext list.</param>
            /// <param name="manualResetEvent">The ManualResetEvent.</param>
            /// <param name="consecutiveFailInfo">The ConsecutiveFailInfo.</param>
            public State(List<EuronextInstrumentContext> contextList, ManualResetEvent manualResetEvent, ConsecutiveFailInfo consecutiveFailInfo)
            {
                FileList = null;
                InstrumentList = null;
                EuronextInstrumentContextList = contextList;
                ManualResetEvent = manualResetEvent;
                EuronextInstrumentContext = null;
                ConsecutiveFailInfo = consecutiveFailInfo;
            }
            #endregion
        }
        #endregion

        #region Recurse
        /// <summary>
        /// Enumerates recursively all files in a directory tree performing an action on each file.
        /// </summary>
        /// <param name="directory">Th directory to recurse.</param>
        /// <param name="prefix">The directory at the currect recursion depth.</param>
        /// <param name="filePattern">The file pattern.</param>
        /// <param name="action">The action receiving a file name.</param>
        private static void Recurse(string directory, string prefix, string filePattern, Action<string> action)
        {
            prefix = string.Concat(prefix, Path.DirectorySeparatorChar);
            directory = string.Concat(directory, Path.DirectorySeparatorChar);
            foreach (string file in Directory.GetFiles(directory, filePattern))
                action(file);
            foreach (string dir in Directory.GetDirectories(directory))
                Recurse(dir, string.Concat(prefix, Path.GetFileName(dir)), filePattern, action);
        }
        #endregion

        #region Iterate
        /// <summary>
        /// Performs parallel iterations through the instrument list, performing an action on every instrument.
        /// </summary>
        /// <param name="splitList">A list of instrument lists representing parallel iteration lines.</param>
        /// <param name="action">An action to perform on every instrument.</param>
        /// <param name="delay">A delay.</param>
        /// <returns>A EuronextInstrumentContext instance needed to package downloaded data.</returns>
        static public EuronextInstrumentContext Iterate(List<List<Instrument>> splitList, Action<EuronextInstrumentContext, ConsecutiveFailInfo> action, int delay)
        {
            int splitCount = splitList.Count;
            var contexts = new EuronextInstrumentContext[splitCount];
            var fails = new ConsecutiveFailInfo[splitCount];
            var manualResetEvents = new ManualResetEvent[splitCount];
            for (int i = 0; i < splitCount; i++)
            {
                manualResetEvents[i] = new ManualResetEvent(false);
                contexts[i] = new EuronextInstrumentContext();
                fails[i] = new ConsecutiveFailInfo();
                var worker = new Thread(delegate(object o)
                {
                    var state = (State)o;
                    EuronextInstrumentContext esc = null;
                    ConsecutiveFailInfo cfi = state.ConsecutiveFailInfo;
                    try
                    {
                        foreach (Instrument s in state.InstrumentList)
                        {
                            esc = new EuronextInstrumentContext { Mep = s.Mep, Mic = s.Mic, Isin = s.Isin, Symbol = s.Symbol, Name = s.Name, SecurityType = s.SecurityType, MillisecondsSince1970 = s.MillisecondsSince1970, RelativePath = s.File };
                            action(esc, cfi);
                        }
                    }
                    finally
                    {
                        state.ManualResetEvent.Set();
                        state.EuronextInstrumentContext = esc;
                    }
                }) {Name = string.Concat("intraday worker ", i.ToString(CultureInfo.InvariantCulture))};
                if (0 != i && 0 < delay)
                    Thread.Sleep(delay);
                worker.Start(new State(splitList[i], manualResetEvents[i], contexts[i], fails[i]));
            }
            WaitHandle.WaitAll(manualResetEvents);
            return contexts[0];
        }

        /// <summary>
        /// Performs parallel iterations through the instrument list, performing an action on every instrument.
        /// </summary>
        /// <param name="splitList">A list of instrument lists representing parallel iteration lines.</param>
        /// <param name="action">An action to perform on every instrument.</param>
        /// <param name="delay">A delay.</param>
        /// <returns>A EuronextInstrumentContext instance needed to package downloaded data.</returns>
        static public void Iterate(List<List<EuronextInstrumentContext>> splitList, Action<EuronextInstrumentContext, ConsecutiveFailInfo> action, int delay)
        {
            int splitCount = splitList.Count;
            var fails = new ConsecutiveFailInfo[splitCount];
            var manualResetEvents = new ManualResetEvent[splitCount];
            for (int i = 0; i < splitCount; i++)
            {
                if (0 != i && 0 < delay)
                    Thread.Sleep(delay);
                manualResetEvents[i] = new ManualResetEvent(false);
                fails[i] = new ConsecutiveFailInfo();
                var worker = new Thread(delegate(object o)
                {
                    var state = (State)o;
                    ConsecutiveFailInfo cfi = state.ConsecutiveFailInfo;
                    try
                    {
                        foreach (EuronextInstrumentContext eic in state.EuronextInstrumentContextList)
                        {
                            action(eic, cfi);
                        }
                    }
                    finally
                    {
                        state.ManualResetEvent.Set();
                    }
                }) {Name = string.Concat("intraday worker ", i.ToString(CultureInfo.InvariantCulture))};
                worker.Start(new State(splitList[i], manualResetEvents[i], fails[i]));
            }
            WaitHandle.WaitAll(manualResetEvents);
        }
        #endregion

        #region Split
        public static int StartDateDaysBack;
        /// <summary>
        /// Exclude all instruments with the following MICs.
        /// </summary>
        public static List<string> ExcludeMics = new List<string>();

        /// <summary>
        /// Splits instruments into splitCount parallel lists.
        /// </summary>
        /// <param name="instrumentIndexFile">A xml file conatining instruments.</param>
        /// <param name="splitCount">A number of parallel split lines.</param>
        /// <returns>A list of instrument lists representing parallel iteration lines.</returns>
        static public List<List<Instrument>> Split(string instrumentIndexFile, int splitCount)
        {
            DateTime dateTime = DateTime.Now;
            if (dateTime.Hour < 8)
                dateTime = dateTime.AddDays(-1);
            if (StartDateDaysBack > 0)
                dateTime = dateTime.AddDays(-StartDateDaysBack);
            var millisecondsSince1970 = (long)(new DateTime(dateTime.Year, dateTime.Month, dateTime.Day) - Year1970).TotalMilliseconds;

            int i, current = 0;
            var fileSplitDictionary = new Dictionary<string, int>(1 < splitCount ? 4096 : 1);
            var splitList = new List<List<Instrument>>(splitCount);
            for (i = 0; i++ < splitCount; )
                splitList.Add(new List<Instrument>(1024));
            try
            {
                // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
                using (XmlReader xmlReader = XmlReader.Create(instrumentIndexFile, xmlReaderSettings))
                {
                    var xmlLineInfo = (IXmlLineInfo)xmlReader;
                    while (xmlReader.Read())
                    {
                        if (XmlNodeType.Element == xmlReader.NodeType &&
                            "instrument" == xmlReader.LocalName)
                        {
                            var instrument = new Instrument { Mic = xmlReader.GetAttribute("mic"), Isin = xmlReader.GetAttribute("isin"), Symbol = xmlReader.GetAttribute("symbol"), Name = xmlReader.GetAttribute("name"), File = xmlReader.GetAttribute("file"), MillisecondsSince1970 = millisecondsSince1970 };
                            if (string.IsNullOrEmpty(instrument.Mic) || string.IsNullOrEmpty(instrument.Isin) ||
                                //string.IsNullOrEmpty(instrument.Symbol) || string.IsNullOrEmpty(instrument.Name) ||
                                string.IsNullOrEmpty(instrument.File))
                            {
                                Trace.TraceError("{0} line {1}: Malfomed instrument element: mic=[{2}] symbol=[{3}] name=[{4}] isin=[{5}] file=[{6}], skipping", instrumentIndexFile, xmlLineInfo.LineNumber, instrument.Mic, instrument.Symbol, instrument.Name, instrument.Isin, instrument.File);
                                continue;
                            }
                            if (ExcludeMics.Count > 0 && ExcludeMics.Contains(instrument.Mic))
                                continue;
                            if (null == instrument.Symbol)
                                instrument.Symbol = "";
                            instrument.Mep = xmlReader.GetAttribute("mep");
                            if (string.IsNullOrEmpty(instrument.Mep))
                                instrument.Mep = null;
                            instrument.SecurityType = xmlReader.GetAttribute("type");
                            if (string.IsNullOrEmpty(instrument.SecurityType))
                            {
                                if (instrument.File.Contains("indices/") || instrument.File.Contains("index/"))
                                    instrument.SecurityType = "index";
                                else if (instrument.File.Contains("stocks/") || instrument.File.Contains("stock/"))
                                    instrument.SecurityType = "stock";
                                else if (instrument.File.Contains("funds/") || instrument.File.Contains("fund/"))
                                    instrument.SecurityType = "fund";
                                else if (instrument.File.Contains("/nav/") || instrument.File.Contains("inav/"))
                                    instrument.SecurityType = "inav";
                                else if (instrument.File.Contains("/etf/") || instrument.File.Contains("etf/"))
                                    instrument.SecurityType = "etf";
                                else if (instrument.File.Contains("/etv/") || instrument.File.Contains("etv/"))
                                    instrument.SecurityType = "etv";
                                else
                                {
                                    Trace.TraceError("{0} line {1}: Malformed/unsupported security type instrument element: mic=[{2}] symbol=[{3}] name=[{4}] isin=[{5}] file=[{6}], skipping", instrumentIndexFile, xmlLineInfo.LineNumber, instrument.Mic, instrument.Symbol, instrument.Name, instrument.Isin, instrument.File);
                                    continue;
                                }
                            }
                            string str = instrument.File;
                            if (string.IsNullOrEmpty(instrument.Symbol) || "null" == instrument.Symbol || "NULL" == instrument.Symbol)
                            {
                                if (!fileSplitDictionary.ContainsKey(str))
                                    fileSplitDictionary.Add(str, 0);
                                splitList[0].Add(instrument);
                            }
                            else
                            {
                                if (1 == splitCount)
                                    splitList[0].Add(instrument);
                                else
                                {
                                    if (fileSplitDictionary.ContainsKey(str))
                                    {
                                        i = fileSplitDictionary[str];
                                        splitList[i].Add(instrument);
                                    }
                                    else
                                    {
                                        fileSplitDictionary.Add(str, current);
                                        splitList[current].Add(instrument);
                                        if (splitCount == ++current)
                                            current = 0;
                                    }
                                }
                            }
                        }
                    }
                    // Explicitly call Close on the XmlReader to reduce strain on the GC.
                    xmlReader.Close();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("{0} exception: {1}", instrumentIndexFile, e.Message);
            }
            return splitList;
        }

        /// <summary>
        /// Recurses a directory and splits filenames into splitCount prallel lists.
        /// </summary>
        /// <param name="importPath">A path to a directory or to a file.</param>
        /// <param name="dictionaryApproved">The dictionary containing an approved  mapping from a mep_symbol_isin_ string to a Instrument.</param>
        /// <param name="dictionaryDiscovered">The dictionary containing a discovered mapping from a mic_symbol_isin_ string to a Instrument.</param>
        /// <param name="splitCount">A number of parallel split lines.</param>
        /// <param name="orphanedList">A list of filenames not found in a dictionary.</param>
        /// <returns>A list of filename lists representing parallel iteration lines.</returns>
        static public List<List<string>> Split(string importPath, Dictionary<string, Instrument> dictionaryApproved, Dictionary<string, Instrument> dictionaryDiscovered, int splitCount, out List<string> orphanedList)
        {
            int i, current = 0;
            bool isFile = false;
            if (!string.IsNullOrEmpty(Path.GetExtension(importPath)))
            {
                splitCount = 1;
                isFile = true;
            }
            if (1 > splitCount)
                splitCount = 1;
            var splitList = new List<List<string>>(splitCount);
            var orphaned = new List<string>(1 < splitCount ? 4096 : 1);
            for (i = 0; i++ < splitCount; )
                splitList.Add(new List<string>(1024));
            try
            {
                if (isFile)
                {
                    string str = Path.GetExtension(importPath);
                    if (".js" == str || ".csv" == str || ".csvh" == str)
                        splitList[0].Add(Path.GetFullPath(importPath));
                    else
                        Trace.TraceError("Illegal file extension [{0}] in the file [{1}]", str, importPath);
                }
                else
                {
                    // The path is a directory.
                    var fileSplitDictionary = new Dictionary<string, int>(1 < splitCount ? 4096 : 1);
                    if (importPath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                        importPath = importPath.TrimEnd(Path.DirectorySeparatorChar);
                    if (importPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                        importPath = importPath.TrimEnd(Path.AltDirectorySeparatorChar);
                    Recurse(importPath, Path.GetFileName(importPath), "*.?s*", file => { // *.js, *.csv, *.csvh
                        try
                        {
                            string[] splitted = Path.GetFileNameWithoutExtension(file).Split('_');
                            if (3 < splitted.Length)
                            {
                                string s = string.Format("{0}_{1}_{2}_", splitted[0], splitted[1], splitted[2]);
                                if (dictionaryApproved.ContainsKey(s))
                                {
                                    Instrument sec = dictionaryApproved[s];
                                    string f = sec.File;
                                    if (fileSplitDictionary.ContainsKey(f))
                                    {
                                        i = fileSplitDictionary[f];
                                        splitList[i].Add(Path.GetFullPath(file));
                                    }
                                    else
                                    {
                                        fileSplitDictionary.Add(f, current);
                                        splitList[current].Add(Path.GetFullPath(file));
                                        if (splitCount == ++current)
                                            current = 0;
                                    }
                                }
                                else if (dictionaryDiscovered.ContainsKey(s))
                                {
                                    Instrument sec = dictionaryDiscovered[s];
                                    string f = sec.File;
                                    if (fileSplitDictionary.ContainsKey(f))
                                    {
                                        i = fileSplitDictionary[f];
                                        splitList[i].Add(Path.GetFullPath(file));
                                    }
                                    else
                                    {
                                        fileSplitDictionary.Add(f, current);
                                        splitList[current].Add(Path.GetFullPath(file));
                                        if (splitCount == ++current)
                                            current = 0;
                                    }
                                }
                                else
                                    orphaned.Add(file);
                            }
                            else
                            {
                                Trace.TraceError("Failed to split: invalid file name [{0}], skipping", file);
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Failed to split: file [{0}] exception [{1}], skipping", file, e.Message);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to split: exception [{0}], aborted", e.Message);
            }
            orphanedList = orphaned;
            return splitList;
        }

        /// <summary>
        /// Splits a list into splitCount parallel lists.
        /// </summary>
        /// <param name="sourceList">A source list.</param>
        /// <param name="splitCount">A number of parallel split lines.</param>
        /// <returns>A list of instrument context lists representing parallel iteration lines.</returns>
        static public List<List<EuronextInstrumentContext>> Split(List<EuronextInstrumentContext> sourceList, int splitCount)
        {
            int i;
            var fileSplitDictionary = new Dictionary<string, int>(1 < splitCount ? 4096 : 1);
            var splitList = new List<List<EuronextInstrumentContext>>(splitCount);
            for (i = 0; i++ < splitCount; )
                splitList.Add(new List<EuronextInstrumentContext>(1024));
            try
            {
                //if (1 == splitCount)
                //    splitList[0].AddRange(sourceList);
                //else
                {
                    int current = 0;
                    foreach (EuronextInstrumentContext context in sourceList)
                    {
                        if (ExcludeMics.Count > 0 && ExcludeMics.Contains(context.Mic))
                            continue;
                        string str = context.RelativePath;
                        if (string.IsNullOrEmpty(context.Symbol) || "null" == context.Symbol || "NULL" == context.Symbol || context.H5FilePath.EndsWith("/.h5"))
                        {
                            if (!fileSplitDictionary.ContainsKey(str))
                                fileSplitDictionary.Add(str, 0);
                            splitList[0].Add(context);
                        }
                        else
                        {
                            if (fileSplitDictionary.ContainsKey(str))
                            {
                                i = fileSplitDictionary[str];
                                splitList[i].Add(context);
                            }
                            else
                            {
                                fileSplitDictionary.Add(str, current);
                                splitList[current].Add(context);
                                if (splitCount == ++current)
                                    current = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("exception: {0}", e.Message);
            }
            return splitList;
        }

        /// <summary>
        /// Splits a list into splitCount parallel lists.
        /// </summary>
        /// <param name="sourceList">A source list.</param>
        /// <param name="splitCount">A number of parallel split lines.</param>
        /// <returns>A list of instrument context lists representing parallel iteration lines.</returns>
        static public List<List<Instrument>> Split(List<Instrument> sourceList, int splitCount)
        {
            int i;
            var fileSplitDictionary = new Dictionary<string, int>(1 < splitCount ? 4096 : 1);
            var splitList = new List<List<Instrument>>(splitCount);
            for (i = 0; i++ < splitCount; )
                splitList.Add(new List<Instrument>(1024));
            try
            {
                //if (1 == splitCount)
                //    splitList[0].AddRange(sourceList);
                //else
                {
                    int current = 0;
                    foreach (Instrument instrument in sourceList)
                    {
                        if (ExcludeMics.Count > 0 && ExcludeMics.Contains(instrument.Mic))
                            continue;
                        string str = instrument.File;
                        if (string.IsNullOrEmpty(instrument.Symbol) || "null" == instrument.Symbol || "NULL" == instrument.Symbol)
                        {
                            if (!fileSplitDictionary.ContainsKey(str))
                                fileSplitDictionary.Add(str, 0);
                            splitList[0].Add(instrument);
                        }
                        else
                        {
                            if (fileSplitDictionary.ContainsKey(str))
                            {
                                i = fileSplitDictionary[str];
                                splitList[i].Add(instrument);
                            }
                            else
                            {
                                fileSplitDictionary.Add(str, current);
                                splitList[current].Add(instrument);
                                if (splitCount == ++current)
                                    current = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("exception: {0}", e.Message);
            }
            return splitList;
        }
        #endregion

        #region ScanIndex
        /// <summary>
        /// Scans instruments and makes a dictionary from a mic_symbol_isin_ string to an Instrument instance.
        /// </summary>
        /// <param name="instrumentIndexFile">A xml file conatining instruments.</param>
        /// <returns>A dictionary from a mep_symbol_isin_ string to an Instrument instance.</returns>
        static public Dictionary<string, Instrument> ScanIndex(string instrumentIndexFile)
        {
            var dictionary = new Dictionary<string, Instrument>(4096);
            try
            {
                // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
                using (XmlReader xmlReader = XmlReader.Create(instrumentIndexFile, xmlReaderSettings))
                {
                    var xmlLineInfo = (IXmlLineInfo)xmlReader;
                    while (xmlReader.Read())
                    {
                        if (XmlNodeType.Element == xmlReader.NodeType &&
                            "instrument" == xmlReader.LocalName)
                        {
                            var instrument = new Instrument {Mic = xmlReader.GetAttribute("mic"), Isin = xmlReader.GetAttribute("isin"), Symbol = xmlReader.GetAttribute("symbol"), Name = xmlReader.GetAttribute("name"), File = xmlReader.GetAttribute("file")};
                            if (string.IsNullOrEmpty(instrument.Mic) || string.IsNullOrEmpty(instrument.Isin) ||
                                //string.IsNullOrEmpty(instrument.Symbol) || string.IsNullOrEmpty(instrument.Name) ||
                                string.IsNullOrEmpty(instrument.File))
                            {
                                Trace.TraceError("{0} line {1}: Malfomed instrument element: mic=[{2}] symbol=[{3}] name=[{4}] isin=[{5}] file=[{6}], skipping", instrumentIndexFile, xmlLineInfo.LineNumber, instrument.Mic, instrument.Symbol, instrument.Name, instrument.Isin, instrument.File);
                                continue;
                            }
                            if (null == instrument.Symbol)
                                instrument.Symbol = "";
                            instrument.Mep = xmlReader.GetAttribute("mep");
                            if (string.IsNullOrEmpty(instrument.Mep))
                                instrument.Mep = null;
                            instrument.SecurityType = xmlReader.GetAttribute("type");
                            if (string.IsNullOrEmpty(instrument.SecurityType))
                            {
                                if (instrument.File.Contains("indices/") || instrument.File.Contains("index/"))
                                    instrument.SecurityType = "index";
                                else if (instrument.File.Contains("stocks/") || instrument.File.Contains("stock/"))
                                    instrument.SecurityType = "stock";
                                else if (instrument.File.Contains("funds/") || instrument.File.Contains("fund/"))
                                    instrument.SecurityType = "fund";
                                else if (instrument.File.Contains("/nav/") || instrument.File.Contains("inav/"))
                                    instrument.SecurityType = "inav";
                                else if (instrument.File.Contains("/etf/") || instrument.File.Contains("etf/"))
                                    instrument.SecurityType = "etf";
                                else if (instrument.File.Contains("/etv/") || instrument.File.Contains("etv/"))
                                    instrument.SecurityType = "etv";
                                else
                                {
                                    Trace.TraceError("{0} line {1}: Malformed/unsupported security type instrument element: mic=[{2}] symbol=[{3}] name=[{4}] isin=[{5}] file=[{6}], skipping", instrumentIndexFile, xmlLineInfo.LineNumber, instrument.Mic, instrument.Symbol, instrument.Name, instrument.Isin, instrument.File);
                                    continue;
                                }
                            }
                            string str = string.Format("{0}_{1}_{2}_", instrument.Mic, instrument.Symbol, instrument.Isin);
                            if (dictionary.ContainsKey(str))
                                Trace.TraceError("{0}: Duplicate {1} key, skipping", instrumentIndexFile, str);
                            else
                                dictionary.Add(str, instrument);
                        }
                    }
                    // Explicitly call Close on the XmlReader to reduce strain on the GC.
                    xmlReader.Close();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("{0} exception: {1}", instrumentIndexFile, e.Message);
            }
            return dictionary;
        }
        #endregion

        #region Iterate
        /// <summary>
        /// Performs parallel iterations through the file list, performing an action on every file.
        /// </summary>
        /// <param name="splitList">A list of file lists representing parallel iteration lines.</param>
        /// <param name="dictionaryApproved">An approved dictionary from a mep_symbol_isin_ string to a Instrument instance.</param>
        /// <param name="dictionaryDiscovered">A discovered dictionary from a mep_symbol_isin_ string to a Instrument instance.</param>
        /// <param name="yyyymmdd">The target date.</param>
        /// <param name="action">An action to perform on every file.</param>
        /// <param name="intraday">Is this an intraday iteration or not..</param>
        static public void Iterate(List<List<string>> splitList, Dictionary<string, Instrument> dictionaryApproved, Dictionary<string, Instrument> dictionaryDiscovered, string yyyymmdd, Action<string, EuronextInstrumentContext> action, bool intraday)
        {
            int splitCount = splitList.Count;
            var contexts = new EuronextInstrumentContext[splitCount];
            var manualResetEvents = new ManualResetEvent[splitCount];
            for (int i = 0; i < splitCount; i++)
            {
                manualResetEvents[i] = new ManualResetEvent(false);
                contexts[i] = new EuronextInstrumentContext();
                contexts[i].SetDate(yyyymmdd);
                var worker = new Thread(delegate(object o)
                {
                    var state = (State)o;
                    var esc = new EuronextInstrumentContext();//state.EuronextInstrumentContext;
                    esc.SetDate(yyyymmdd);
                    try
                    {
                        foreach (string file in state.FileList)
                        {
                            string[] splitted = Path.GetFileNameWithoutExtension(file).Split('_');
                            if (3 < splitted.Length)
                            {
                                string s = string.Format("{0}_{1}_{2}_", splitted[0], splitted[1], splitted[2]);
                                if (dictionaryApproved.ContainsKey(s))
                                {
                                    Instrument sec = dictionaryApproved[s];
                                    esc.Mep = sec.Mep;
                                    esc.Mic = sec.Mic;
                                    esc.Isin = sec.Isin;
                                    esc.Symbol = sec.Symbol;
                                    esc.Name = sec.Name;
                                    esc.SecurityType = sec.SecurityType;
                                    esc.MillisecondsSince1970 = sec.MillisecondsSince1970;
                                    esc.DownloadedPath = file;
                                    esc.RelativePath = sec.File;
                                    s = string.Concat(intraday ? EuronextInstrumentContext.IntradayRepositoryPath : EuronextInstrumentContext.EndofdayRepositoryPath, esc.RelativePath);
                                    action(s, esc);
                                }
                                else if (dictionaryDiscovered.ContainsKey(s))
                                {
                                    Instrument sec = dictionaryDiscovered[s];
                                    esc.Mep = sec.Mep;
                                    esc.Mic = sec.Mic;
                                    esc.Isin = sec.Isin;
                                    esc.Symbol = sec.Symbol;
                                    esc.Name = sec.Name;
                                    esc.SecurityType = sec.SecurityType;
                                    esc.MillisecondsSince1970 = sec.MillisecondsSince1970;
                                    esc.DownloadedPath = file;
                                    esc.RelativePath = sec.File;
                                    s = string.Concat(intraday ? EuronextInstrumentContext.IntradayDiscoveredRepositoryPath : EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc.RelativePath);
                                    action(s, esc);
                                }
                                else
                                    Trace.TraceError("Failed to iterate: invalid file name [{0}], skipping", file);
                            }
                            else
                            {
                                Trace.TraceError("Failed to iterate: invalid file name [{0}], skipping", file);
                            }
                        }
                    }
                    finally
                    {
                        state.ManualResetEvent.Set();
                    }
                }) {Name = string.Concat("intraday worker ", i.ToString())};
                worker.Start(new State(splitList[i], manualResetEvents[i], contexts[i]));
            }
            WaitHandle.WaitAll(manualResetEvents);
        }
        #endregion
    }
}
