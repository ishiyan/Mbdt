using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Mbh5;
using mbdt.Euronext;

namespace mbdt.EuronextBigConverter
{
    /// <summary>
    /// Euronext Audit utility.
    /// </summary>
    internal static class EuronextBigConverter
    {

        internal static void Task2(string indexFile)
        {
            Repository.InterceptErrorStack();
            XDocument xdoc = XDocument.Load(indexFile
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();
            bool isFailed = false;
            foreach (var xel in xelist)
            {
                if (string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Type)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.File)) || !xel.AttributeValue(EuronextInstrumentXml.File).EndsWith(".xml") || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Isin)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Mic)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Mep)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Symbol)))
                {
                    Trace.TraceError("Malformed instrument: type|file|isin|mic|mep|symbol is empty or undefined:{0}{1}", Environment.NewLine, xel.ToString(SaveOptions.None));
                    isFailed = true;
                }
            }
            if (isFailed)
                return;
            EuronextInstrumentXml.BackupXmlFile(indexFile, DateTime.Now);

            var dic = new Dictionary<string, List<XElement>>();
            var dicRemove = new Dictionary<string, List<XElement>>();
            foreach (var xel in xelist)
            {
                string file = xel.AttributeValue(EuronextInstrumentXml.File).Replace(".xml", ".h5");
                string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
                string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
                string mep = xel.AttributeValue(EuronextInstrumentXml.Mep);
                string symbol = xel.AttributeValue(EuronextInstrumentXml.Symbol);
                string type = xel.AttributeValue(EuronextInstrumentXml.Type);

                int i = file.LastIndexOf('/'); // foo/bar/name.h5 -> foo/bar/
                string instrumentOld = file.Substring(0, i + 1);
                instrumentOld = string.Concat("/", instrumentOld, mep, "_", symbol, "_", isin);
                string instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);

                string fileNew = string.Concat(mic.ToLowerInvariant(), "/", type.ToLowerInvariant(), "/");
                if (file.Contains("indices/icb/"))
                    fileNew = string.Concat(fileNew, "icb/");
                string nm = file.Substring(i + 1).Replace(".h5", "");
                if (nm != symbol)
                {
                    if (symbol == "GENP" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "ALSIP" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "AREVA" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "LVL" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "MDL" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "VTA" && nm == "null")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else if (symbol == "VERIZ" && nm == "TNLA")
                    {
                        fileNew = string.Concat(fileNew, symbol, ".h5");
                        instrumentOld = file.Substring(0, i + 1);
                        instrumentOld = string.Concat("/", instrumentOld, mep, "_", nm, "_", isin);
                        instrumentNew = string.Concat("/", mic, "_", symbol, "_", isin);
                    }
                    else
                    if (symbol == "AUX" && nm == "AUX_")
                    {
                        fileNew = string.Concat(fileNew, symbol, "_.h5");
                    }
                    else
                        fileNew = string.Concat(fileNew, symbol, ".h5");
                    Trace.WriteLine($"File [{file}] is referred by symbol [{symbol}], new file is [{fileNew}]");
                }
                else
                    fileNew = string.Concat(fileNew, file.Substring(i + 1));

                string attrNew = string.Concat(fileNew, ":", instrumentNew);

                if (!File.Exists(file))
                    Trace.WriteLine($"Orphaned entry: file [{file}] does not exist");
                else
                {
                    if (dic.TryGetValue(fileNew, out var list))
                        list.Add(xel);
                    else
                        dic.Add(fileNew, new List<XElement> { xel });

                    MoveInstrumentAndCopyFile(file, fileNew, instrumentOld, instrumentNew);
                    if (dicRemove.TryGetValue(file, out list))
                        list.Add(xel);
                    else
                        dicRemove.Add(file, new List<XElement> { xel });
                }
                xel.SetAttributeValue(EuronextInstrumentXml.File, attrNew);
                xdoc.Save(indexFile, SaveOptions.None);
                // Trace.WriteLine($"instrument: [{instrumentOld}]->[{instrumentNew}], file: [{file}]->[{fileNew}], attr: [{attrNew}]");

            }
            xdoc.Save(indexFile, SaveOptions.None);

            foreach (var kvp in dic)
            {
                RemoveEmptyGroups(kvp.Key);
            }
            foreach (var kvp in dicRemove)
            {
                if (kvp.Value.Count < 2)
                {
                    string fileNew = "removed_single/" + kvp.Key;
                    var fi = new FileInfo(fileNew);
                    EnsureDirectoryExists(fi);
                    File.Move(kvp.Key, fileNew);
                }
                else
                {
                    var d = new Dictionary<string, XElement>();
                    bool duplicate = false;
                    foreach (var xel in kvp.Value)
                    {
                        string key = string.Concat(xel.AttributeValue(EuronextInstrumentXml.Mic), "_", xel.AttributeValue(EuronextInstrumentXml.Symbol), "_", xel.AttributeValue(EuronextInstrumentXml.Isin));
                        if (d.ContainsKey(key))
                        {
                            duplicate = true;
                            break;
                        }
                        d.Add(key, xel);
                    }

                    string fileNew = (duplicate ? "removed_multiple_duplicate/" : "removed_multiple") + kvp.Key;
                    var fi = new FileInfo(fileNew);
                    EnsureDirectoryExists(fi);
                    File.Move(kvp.Key, fileNew);
                }
            }
            /*foreach (var kvp in dic)
            {
                if (kvp.Value.Count < 2)
                    continue;

                var d = new Dictionary<string, XElement>();
                bool duplicate = false;
                foreach (var xel in kvp.Value)
                {
                    string key = string.Concat(xel.AttributeValue(EuronextInstrumentXml.Mic), "_", xel.AttributeValue(EuronextInstrumentXml.Symbol), "_", xel.AttributeValue(EuronextInstrumentXml.Isin));
                    if (d.ContainsKey(key))
                    {
                        duplicate = true;
                        break;
                    }
                    d.Add(key, xel);
                }
                if (!duplicate)
                    continue;
                Trace.WriteLine("");
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("The same target file [{0}] for {1} instruments: (DUPLICATE KEYS)", kvp.Key, kvp.Value.Count));
                foreach (var xel in kvp.Value)
                {
                    Trace.WriteLine(xel.ToString(SaveOptions.None));
                }
                Trace.WriteLine("");
            }*/
            //Audit(Properties.Settings.Default.ApprovedIndexPath);
            //Audit(Properties.Settings.Default.DiscoveredIndexPath);
        }

        private static void MoveInstrumentAndCopyFile(string fileOld, string fileNew, string instPathOld, string instPathNew)
        {
            var fileInfo = new FileInfo(fileOld);
            DateTime dateTimeLastAccessUtc = fileInfo.LastAccessTimeUtc;
            DateTime dateTimeCreationUtc = fileInfo.CreationTimeUtc;
            DateTime dateTimeLastWriteUtc = fileInfo.LastWriteTimeUtc;
            bool isReadOnly = fileInfo.IsReadOnly;
            if (isReadOnly)
                fileInfo.IsReadOnly = false;
            Repository repository = Repository.OpenReadWrite(fileOld, false);
            if (null == repository)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] failed to open file", fileOld);
                Trace.TraceError(msg);
                if (isReadOnly)
                    fileInfo.IsReadOnly = true;
                throw new ApplicationException(msg);
            }
            try
            {
                Trace.WriteLine($"Begin moving file [{fileNew}] instrument path: [{instPathOld}]->[{instPathNew}]");
                repository.MoveInstrument(instPathOld, instPathNew);
                Trace.WriteLine("Finished moving");
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "[{0}]: failed to move instrument [{1}]->[{2}] exception: {3}", fileOld, instPathOld, instPathNew, ex.Message);
                Trace.TraceError(msg);
                throw;
            }
            finally
            {
                repository.Close();
                if (isReadOnly)
                    fileInfo.IsReadOnly = true;
                fileInfo.CreationTimeUtc = dateTimeCreationUtc;
                fileInfo.LastWriteTimeUtc = dateTimeLastWriteUtc;
                fileInfo.LastAccessTimeUtc = dateTimeLastAccessUtc;
            }
            // If we are here, exception was not thrown and file was updated.
            try
            {
                var fileInfoNew = new FileInfo(fileNew);
                EnsureDirectoryExists(fileInfoNew);
                File.Copy(fileOld, fileNew, true);
                if (isReadOnly)
                    fileInfoNew.IsReadOnly = true;
                fileInfoNew.CreationTimeUtc = dateTimeCreationUtc;
                fileInfoNew.LastWriteTimeUtc = dateTimeLastWriteUtc;
                fileInfoNew.LastAccessTimeUtc = dateTimeLastAccessUtc;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "[{0}]: failed to copy file to the  [{1}], exception: {2}", fileOld, fileNew, ex.Message);
                Trace.TraceError(msg);
                throw;
            }
        }

        private static void RemoveEmptyGroups(string file)
        {
            var fileInfo = new FileInfo(file);
            DateTime dateTimeLastAccessUtc = fileInfo.LastAccessTimeUtc;
            DateTime dateTimeCreationUtc = fileInfo.CreationTimeUtc;
            DateTime dateTimeLastWriteUtc = fileInfo.LastWriteTimeUtc;
            bool isReadOnly = fileInfo.IsReadOnly;
            if (isReadOnly)
                fileInfo.IsReadOnly = false;
            Repository repository = Repository.OpenReadWrite(file, false);
            if (null == repository)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "[{0}] failed to open file", file);
                Trace.TraceError(msg);
                if (isReadOnly)
                    fileInfo.IsReadOnly = true;
                throw new ApplicationException(msg);
            }

            List<DataInfo> ldi = repository.ContentList(true);
            GroupInfo gi = repository.ContentTree(true);
            foreach (var v in gi.Groups)
            {
                string name = "/" + v.Name + "/";
                bool found = false;
                foreach (var di in ldi)
                {
                    if (di.Path.StartsWith(name))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
                name = "/" + v.Name;
                repository.Delete(name, false);
                Trace.WriteLine($"Cleaning file [{file}]: deleting path: [{name}]");
            }
            int count = ldi.Count;
            if (count > 2)
                Trace.WriteLine($"Cleaning file [{file}]: {count} datasets found");

            repository.Close();
                if (isReadOnly)
                    fileInfo.IsReadOnly = true;
                fileInfo.CreationTimeUtc = dateTimeCreationUtc;
                fileInfo.LastWriteTimeUtc = dateTimeLastWriteUtc;
                fileInfo.LastAccessTimeUtc = dateTimeLastAccessUtc;
        }

        private static void EnsureDirectoryExists(FileInfo fileInfo)
        {
            DirectoryInfo directoryInfo = fileInfo.Directory;
            try
            {
                string directory = directoryInfo.FullName;
                if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) &&
                    !directory.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                    directory = string.Concat(directory, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Failed to ensure an existance of directory  [{0}], exception: {1}", directoryInfo.FullName, ex.Message);
                Trace.TraceError(msg);
                throw;
            }
        }

        internal static void Task(string indexFile)
        {
            string prefix;
            if (indexFile.StartsWith("discover"))
                prefix = "discovered\\";
            else if (indexFile.StartsWith("delist"))
                prefix = "delisted\\";
            else
                prefix = "";
            //Repository.InterceptErrorStack();
            XDocument xdoc = XDocument.Load(indexFile
                /*, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo*/);
            List<XElement> xelist = xdoc.XPathSelectElements("/instruments/instrument").ToList();
            bool isFailed = false;
            foreach (var xel in xelist)
            {
                if (string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Type)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.File)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Isin)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Mic)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Mep)) || string.IsNullOrEmpty(xel.AttributeValue(EuronextInstrumentXml.Symbol)))
                {
                    Trace.TraceError("Malformed instrument: type|file|isin|mic|mep|symbol is empty or undefined:{0}{1}", Environment.NewLine, xel.ToString(SaveOptions.None));
                    isFailed = true;
                }
                string file = xel.AttributeValue(EuronextInstrumentXml.File);
                if (!file.Contains(".h5:/"))
                {
                    Trace.TraceError("Malformed file [{0}]", file);
                    isFailed = true;
                }
            }
            if (isFailed)
                return;

            Trace.WriteLine("Non-existing files ------------------------------------------------------");
            foreach (var xel in xelist)
            {
                string file = xel.AttributeValue(EuronextInstrumentXml.File);
                int i = file.IndexOf(":/", StringComparison.Ordinal);// foo/bar.h5:/
                string f = prefix + file.Substring(0, i);
                //string foo = xel.AttributeValue("foo");
                //if ("disco" == xel.AttributeValue("foo"))
                //    f = "discovered/" + f;
                //else
                //    f = "euronext/" + f;
                if (!File.Exists(f))
                {
                    //xel.SetAttributeValue("foo", foo + " ----------");
                    Trace.WriteLine("");
                    Trace.WriteLine(
                        $"Non-existing file [{file}] in element{Environment.NewLine}{xel.ToString(SaveOptions.None)}");
                    Trace.WriteLine("");
                }
                else
                {
                    //var fi = new FileInfo(f);
                    //xel.SetAttributeValue("foo", foo + fi.LastWriteTime.ToString(" yyyy-MM-dd"));
                }
            }

            Trace.WriteLine("Duplicate mic+isin+symbol ------------------------------------------------------");
            var dic1 = new Dictionary<string, List<XElement>>();
            foreach (var xel in xelist)
            {
                string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
                string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
                string symbol = xel.AttributeValue(EuronextInstrumentXml.Symbol);
                string key = string.Concat(mic, "_", symbol, "_", isin);
                if (dic1.TryGetValue(key, out var list))
                    list.Add(xel);
                else
                    dic1.Add(key, new List<XElement> { xel });
            }
            var l = dic1.ToList();
            l.Sort((a, b) => b.Value.Count - a.Value.Count);
            foreach (var kvp in l)
            {
                if (kvp.Value.Count < 2)
                    continue;
                Trace.WriteLine("");
                Trace.WriteLine("");
                Trace.WriteLine($"Duplicate mic_symbol_isin [{kvp.Key}] for {kvp.Value.Count} instruments:");
                foreach (var xel in kvp.Value)
                {
                    Trace.WriteLine(xel.ToString(SaveOptions.None));
                }
                Trace.WriteLine("");
            }

            Trace.WriteLine("Duplicate mic+isin ------------------------------------------------------");
            dic1.Clear();
            l.Clear();
            foreach (var xel in xelist)
            {
                string isin = xel.AttributeValue(EuronextInstrumentXml.Isin);
                string mic = xel.AttributeValue(EuronextInstrumentXml.Mic);
                string key = string.Concat(mic, "_", isin);
                if (dic1.TryGetValue(key, out var list))
                    list.Add(xel);
                else
                    dic1.Add(key, new List<XElement> { xel });
            }
            l = dic1.ToList();
            l.Sort((a, b) => b.Value.Count - a.Value.Count);
            foreach (var kvp in l)
            {
                if (kvp.Value.Count < 2)
                    continue;
                Trace.WriteLine("");
                Trace.WriteLine("");
                Trace.WriteLine($"Duplicate mic_isin [{kvp.Key}] for {kvp.Value.Count} instruments:");
                foreach (var xel in kvp.Value)
                {
                    Trace.WriteLine(xel.ToString(SaveOptions.None));
                }
                Trace.WriteLine("");
            }

            Trace.WriteLine("Instruments with the same file ------------------------------------------------------");
            dic1.Clear();
            l.Clear();
            foreach (var xel in xelist)
            {
                string file = xel.AttributeValue(EuronextInstrumentXml.File);
                int i = file.IndexOf(":/", StringComparison.Ordinal);// foo/bar.h5:/
                string f = prefix + file.Substring(0, i);
                if (dic1.TryGetValue(f, out var list))
                    list.Add(xel);
                else
                    dic1.Add(f, new List<XElement> { xel });
            }
            l = dic1.ToList();
            l.Sort((a, b) => b.Value.Count - a.Value.Count);
            foreach (var kvp in l)
            {
                if (kvp.Value.Count < 2)
                    continue;
                Trace.WriteLine("");
                Trace.WriteLine("");
                Trace.WriteLine($"Duplicate file [{kvp.Key}] for {kvp.Value.Count} instruments:");
                foreach (var xel in kvp.Value)
                {
                    Trace.WriteLine(xel.ToString(SaveOptions.None));
                }
                Trace.WriteLine("");
            }

            // This should be the last one, since it removes elements from the xel list.
            Trace.WriteLine("Instrument clusters ------------------------------------------------------");
            var listOfLists = new List<List<XElement>>();
            for (int i = 0; i < xelist.Count; ++i)
            {
                XElement v = xelist[i];
                string isin = v.AttributeValue(EuronextInstrumentXml.Isin);
                string symbol = v.AttributeValue(EuronextInstrumentXml.Symbol);
                string name = v.AttributeValue(EuronextInstrumentXml.Name);
                string file = v.AttributeValue(EuronextInstrumentXml.File);
                int k = file.IndexOf(":/", StringComparison.Ordinal);// foo/bar.h5:/
                string f = prefix + file.Substring(0, k);
                var list = new List<XElement> { v };
                for (int j = k + 1; j < xelist.Count; ++j)
                {
                    XElement w = xelist[j];
                    string x = w.AttributeValue(EuronextInstrumentXml.File);
                    int z = x.IndexOf(":/", StringComparison.Ordinal);// foo/bar.h5:/
                    string y = x.Substring(0, z);
                    if (y == f || isin == w.AttributeValue(EuronextInstrumentXml.Isin) || symbol == w.AttributeValue(EuronextInstrumentXml.Symbol) || w.AttributeValue(EuronextInstrumentXml.Name).Contains(name))
                    {
                        list.Add(w);
                        xelist.Remove(w);
                    }
                }
                if (list.Count > 1)
                    listOfLists.Add(list);
            }
            listOfLists.Sort((a, b) => b.Count - a.Count);
            foreach (var list in listOfLists)
            {
                Trace.WriteLine("");
                Trace.TraceError("Cluster with {0} instruments", list.Count);
                Trace.WriteLine("");
                foreach (var v in list)
                {
                    Trace.WriteLine(v.ToString(SaveOptions.None));
                }
                Trace.WriteLine("");
                Trace.WriteLine("");
            }
        }
    }
}
