using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Xml;

using Mbh5;

namespace EoniaConvert
{
    static class Program
    {
        private static void TraverseTree(string root, Action<string> action)
        {
            if (Directory.Exists(root))
            {
                string[] entries = Directory.GetFiles(root);
                foreach (string entry in entries)
                    action(entry);
                entries = Directory.GetDirectories(root);
                foreach (string entry in entries)
                    TraverseTree(entry, action);
            }
            else if (File.Exists(root))
                action(root);
        }

        private static DateTime TimeToTicks(string input)
        {
            int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(input.Substring(4, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(input.Substring(6, 2), CultureInfo.InvariantCulture);
            return new DateTime(year, month, day, 0, 0, 0);
        }

        private static void CollectXml(string sourceFileName)
        {
            // Wrap the creation of the XmlReader in a 'using' block since it implements IDisposable.
            using (XmlReader xmlReader = XmlReader.Create(new StreamReader(sourceFileName)))
            {
                xmlLineInfo = (IXmlLineInfo)xmlReader;
                while (xmlReader.Read())
                {
                    if (XmlNodeType.Element == xmlReader.NodeType)
                    {
                        if ("q" == xmlReader.LocalName)
                        {
                            var rate = new Rate();
                            string str = xmlReader.GetAttribute("d");
                            if (null == str)
                                throw new ArgumentException(NoAttribute("d"));
                            rate.DateTime = TimeToTicks(str);
                            str = xmlReader.GetAttribute("r");
                            if (null == str)
                                throw new ArgumentException(NoAttribute("r"));
                            rate.Eonia = str;
                            rateList.Add(rate);
                        }
                    }
                }
                // Explicitly call Close on the XmlReader to reduce strain on the GC.
                xmlReader.Close();
            }
        }

        private static void CollectCsv(string sourceFileName)
        {
            using (var sourceFile = new StreamReader(sourceFileName))
            {
                string line;
                while (null != (line = sourceFile.ReadLine()))
                {
                    if (line.StartsWith(";"))
                        continue;
                    string[] splitted = line.Split(';');
                    if (2 < splitted.Length)
                        Trace.TraceError("file {0}: illegal line [{1}]", sourceFileName, line);
                    var rate = new Rate {DateTime = DateTime.ParseExact(splitted[0], Properties.Settings.Default.CsvDateFormat, CultureInfo.InvariantCulture), Eonia = splitted[1]};
                    rateList.Add(rate);
                }
            }
        }

        private class Rate
        {
            internal DateTime DateTime;
            internal string Eonia;
        }
        static private readonly List<Rate> rateList = new List<Rate>();
        static private IXmlLineInfo xmlLineInfo;
        static private string NoAttribute(string attribute)
        {
            return string.Format("line {0}: {1} attribute not found", xmlLineInfo.LineNumber, attribute);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: file_name");
            else
            {
                var scalar = new Scalar();
                var scalarList = new List<Scalar>();
                Repository.InterceptErrorStack();
                Data.DefaultMaximumReadBufferBytes = Properties.Settings.Default.Hdf5MaxReadBufferBytes;
                string h5File = Properties.Settings.Default.RepositoryFile;
                var fileInfo = new FileInfo(h5File);
                string directoryName = fileInfo.DirectoryName;
                if (null != directoryName && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                Repository repository = Repository.OpenReadWrite(h5File, true, Properties.Settings.Default.Hdf5CorkTheCache);
                Instrument instrument = repository.Open(Properties.Settings.Default.RepositoryRoot, true);
                ScalarData scalarData = instrument.OpenScalar(ScalarKind.Default, DataTimeFrame.Day1, true);

                if (args[0].EndsWith(".xml"))
                    TraverseTree(args[0], CollectXml);
                else
                {
                    TraverseTree(args[0], CollectCsv);
                    //rateList.Reverse();
                }
                foreach (var r in rateList)
                {
                    if (null == r.Eonia)
                        continue;
                    scalar.dateTimeTicks = r.DateTime.Ticks;
                    scalar.value = double.Parse(r.Eonia, CultureInfo.InvariantCulture);
                    scalarList.Add(scalar);
                }
                scalarData.Add(scalarList, DuplicateTimeTicks.Skip, true);
                scalarData.Flush();
                scalarData.Close();
                instrument.Close();
                repository.Close();
            }
        }
    }
}
