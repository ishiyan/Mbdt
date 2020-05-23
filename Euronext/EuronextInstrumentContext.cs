using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

using mbdt.Utils;

namespace mbdt.Euronext
{
    /// <summary>
    /// Encapsulates Euronext instrument info.
    /// </summary>
    class EuronextInstrumentContext
    {
        #region Members and accessors
        private static readonly DateTime Year1970 = new DateTime(1970, 1, 1);

        #region Mep
        /// <summary>
        /// The Market Entry Place.
        /// </summary>
        public string Mep { get; set; }
        #endregion

        #region Mic
        private string mic;
        /// <summary>
        /// The MIC based on MEP.
        /// </summary>
        public string Mic
        {
            get
            {
                if (null != mic)
                    return mic;
                if (Mep == "AMS")
                    return "XAMS";
                if (Mep == "PAR")
                    return "XPAR";
                if (Mep == "BRU")
                    return "XBRU";
                if (Mep == "LIS")
                    return "XLIS";
                if (Mep == "DUB")
                    return "XDUB";
                return "XXXX";
            }
            set { mic = value; }
        }
        #endregion

        #region Isin
        /// <summary>
        /// The International Securities Identifying Number.
        /// </summary>
        public string Isin { get; set; }
        #endregion

        #region Symbol
        /// <summary>
        /// The symbol (ticker).
        /// </summary>
        public string Symbol { get; set; }
        #endregion

        #region Name
        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region SecurityType
        /// <summary>
        /// The type of security.
        /// </summary>
        public string SecurityType { get; set; }
        #endregion

        #region MillisecondsSince1970
        /// <summary>
        /// The number of milliseconds since 01/Jan/1970.
        /// </summary>
        public long MillisecondsSince1970 { get; set; }
        #endregion

        #region Yyyymmdd
        /// <summary>
        /// The date stamp in YYYYMMDD format.
        /// </summary>
        public string Yyyymmdd { get; private set; }
        #endregion

        #region RelativePath
        /// <summary>
        /// The relative path to an xml data file.
        /// </summary>
        public string RelativePath { get; set; }
        #endregion

        #region H5FilePath
        /// <summary>
        /// The relative path to the H5 file.
        /// </summary>
        internal string H5FilePath
        {
            get
            {
                string filePath;
                int index = RelativePath.IndexOf(".h5:/", StringComparison.Ordinal);
                if (0 > index)
                {
                    if (RelativePath.EndsWith(".xml"))
                        filePath = RelativePath.Replace(".xml", ".h5");
                    else if (RelativePath.EndsWith(".h5"))
                        filePath = RelativePath;
                    else
                        filePath = string.Concat(RelativePath, ".h5");
                }
                else
                    filePath = RelativePath.Substring(0, index + 3);
                return filePath;
            }
        }
        #endregion

        #region H5InstrumentPath
        /// <summary>
        /// The path to the instrument from the root within a H5 file.
        /// </summary>
        internal string H5InstrumentPath
        {
            get
            {
                string instrumentPath;
                int index = RelativePath.IndexOf(".h5:/", StringComparison.Ordinal);
                if (0 > index)
                {
                    instrumentPath = string.Concat("/",
                        string.IsNullOrEmpty(Mep) ? "" : Mic, "_",
                        string.IsNullOrEmpty(Symbol) ? "" : Symbol, "_",
                        string.IsNullOrEmpty(Isin) ? "" : Isin);

                    //index = RelativePath.LastIndexOf("/", StringComparison.Ordinal);
                    //if (1 > index)
                    //    instrumentPath = string.Concat("/",
                    //        string.IsNullOrEmpty(Mep) ? "" : Mep, "_",
                    //        string.IsNullOrEmpty(Symbol) ? "" : Symbol, "_",
                    //        string.IsNullOrEmpty(Isin) ? "" : Isin);
                    //else
                    //    instrumentPath = string.Concat("/",
                    //        RelativePath.Substring(0, index), "/",
                    //        string.IsNullOrEmpty(Mep) ? "" : Mep, "_",
                    //        string.IsNullOrEmpty(Symbol) ? "" : Symbol, "_",
                    //        string.IsNullOrEmpty(Isin) ? "" : Isin);
                }
                else
                    instrumentPath = RelativePath.Substring(index + 4);
                return instrumentPath;
            }
        }
        #endregion

        #region DownloadedPath
        /// <summary>
        /// The absolute path to a downloaded file.
        /// </summary>
        public string DownloadedPath { get; set; }
        #endregion

        #region DownloadRepositorySuffix
        /// <summary>
        /// The Euronext download repository suffix.
        /// </summary>
        public string DownloadRepositorySuffix {get; private set;}
        #endregion

        #region DownloadRepositoryPath
        static private string downloadRepositoryPath = @"downloads\euronext\";

        /// <summary>
        /// The absolute path to the Euronext download repository.
        /// </summary>
        static public string DownloadRepositoryPath
        {
            get
            {
                return downloadRepositoryPath;
            }
            set
            {
                downloadRepositoryPath = VerifyDirectory(value);
            }
        }
        #endregion

        #region IntradayRepositoryPath
        static private string intradayRepositoryPath = @"repository\intraday\";

        /// <summary>
        /// The absolute path to the Euronext intraday repository.
        /// </summary>
        static public string IntradayRepositoryPath
        {
            get
            {
                return intradayRepositoryPath;
            }
            set
            {
                intradayRepositoryPath = VerifyDirectory(value);
            }
        }
        #endregion

        #region IntradayDiscoveredRepositoryPath
        static private string intradayDiscoveredRepositoryPath = @"repository\intraday\euronext-discovered\";

        /// <summary>
        /// The absolute path to the Euronext intraday discovered repository.
        /// </summary>
        static public string IntradayDiscoveredRepositoryPath
        {
            get
            {
                return intradayDiscoveredRepositoryPath;
            }
            set
            {
                intradayDiscoveredRepositoryPath = VerifyDirectory(value);
            }
        }
        #endregion

        #region EndofdayRepositoryPath
        static private string endofdayRepositoryPath = @"repository\endofday\";

        /// <summary>
        /// The absolute path to the Euronext endofday repository.
        /// </summary>
        static public string EndofdayRepositoryPath
        {
            get
            {
                return endofdayRepositoryPath;
            }
            set
            {
                endofdayRepositoryPath = VerifyDirectory(value);
            }
        }
        #endregion

        #region EndofdayDiscoveredRepositoryPath
        static private string endofdayDiscoveredRepositoryPath = @"repository\endofday\euronext-discovered\";

        /// <summary>
        /// The absolute path to the Euronext endofday discovered repository.
        /// </summary>
        static public string EndofdayDiscoveredRepositoryPath
        {
            get
            {
                return endofdayDiscoveredRepositoryPath;
            }
            set
            {
                endofdayDiscoveredRepositoryPath = VerifyDirectory(value);
            }
        }
        #endregion

        #region WorkerThreads
        /// <summary>
        /// The number of worker threads.
        /// </summary>
        static public int WorkerThreads { get; set; }
        #endregion

        #region DownloadPasses
        /// <summary>
        /// The number of download passes.
        /// </summary>
        static public int DownloadPasses { get; set; }
        #endregion

        #region DownloadRetries
        /// <summary>
        /// The number of download retires.
        /// </summary>
        static public int DownloadRetries {get; set; }
        #endregion

        #region DownloadTimeout
        /// <summary>
        /// The download timeout in milliseconds.
        /// </summary>
        static public int DownloadTimeout {get; set; }
        #endregion

        #region IntradayDownloadMinimalLength
        /// <summary>
        /// The minimal length of a downloaded file in bytes.
        /// </summary>
        static public long IntradayDownloadMinimalLength { get; set; }
        #endregion

        #region IntradayDownloadOverwriteExisting
        /// <summary>
        /// If a file to download already exists, overwrite it.
        /// </summary>
        static public bool IntradayDownloadOverwriteExisting { get; set; }
        #endregion

        #region IntradayDownloadMaximumLimitConsecutiveFails
        /// <summary>
        /// The maximal number of consecutive intraday download fails.
        /// </summary>
        static public int IntradayDownloadMaximumLimitConsecutiveFails { get; set; }
        #endregion

        #region IntradayWorkerThreadDelayMilliseconds
        /// <summary>
        /// The intraday worker thread start delay in milliseconds.
        /// </summary>
        static public int IntradayWorkerThreadDelayMilliseconds { get; set; }
        #endregion

        #region HistoryDownloadMinimalLength
        /// <summary>
        /// The minimal length of a downloaded file in bytes.
        /// </summary>
        static public long HistoryDownloadMinimalLength { get; set; }
        #endregion

        #region HistoryDownloadOverwriteExisting
        /// <summary>
        /// If a file to download already exists, overwrite it.
        /// </summary>
        static public bool HistoryDownloadOverwriteExisting { get; set; }
        #endregion

        #region HistoryDownloadMaximumLimitConsecutiveFails
        /// <summary>
        /// The maximal number of consecutive history download fails.
        /// </summary>
        static public int HistoryDownloadMaximumLimitConsecutiveFails { get; set; }
        #endregion

        #region HistoryWorkerThreadDelayMilliseconds
        /// <summary>
        /// The history worker thread start delay in milliseconds.
        /// </summary>
        static public int HistoryWorkerThreadDelayMilliseconds { get; set; }
        #endregion

        #region IsHistoryAdjusted
        /// <summary>
        /// If an adjusted history should be downloaded.
        /// </summary>
        static public bool IsHistoryAdjusted { get; set; }
        #endregion

        #region MillisecondsSinceBegin1970
        /// <summary>
        /// The number of milliseconds since 1 january 1970.
        /// </summary>
        static internal long MillisecondsSinceBegin1970(DateTime dateTime)
        {
            return (long)(dateTime - Year1970).TotalMilliseconds;
        }

        static internal DateTime DateTimeFromMillisecondsSince1970(long milliseconds)
        {
            return Year1970.AddMilliseconds(milliseconds);
        }
        #endregion

        #region ApprovedIndexPath
        static private string approvedIndexPath = @"repository\endofday\instruments.xml";

        /// <summary>
        /// The path to the aproved instruments index xml file.
        /// </summary>
        static public string ApprovedIndexPath
        {
            get
            {
                return approvedIndexPath;
            }
            set
            {
                approvedIndexPath = value;
            }
        }
        #endregion

        #region DiscoveredIndexPath
        static private string discoveredIndexPath = @"repository\endofday\euronext-discovered\instruments.xml";

        /// <summary>
        /// The path to the discovered instruments index xml file.
        /// </summary>
        static public string DiscoveredIndexPath
        {
            get
            {
                return discoveredIndexPath;
            }
            set
            {
                discoveredIndexPath = value;
            }
        }
        #endregion
        #endregion

        #region Construction
        /// <summary>
        /// The static constructor.
        /// </summary>
        static EuronextInstrumentContext()
        {
            DownloadPasses = 10;
            DownloadRetries = 20;
            DownloadTimeout = 180000;
            IntradayDownloadMinimalLength = 10;
            IntradayDownloadOverwriteExisting = true;
            HistoryDownloadMinimalLength = 10;
            HistoryDownloadOverwriteExisting = true;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="EuronextInstrumentContext"/> class.
        /// </summary>
        public EuronextInstrumentContext()
        {
            SetDate(DateTime.Now);
        }
        #endregion

        #region SetDate
        /// <summary>
        /// Sets the current date.
        /// </summary>
        /// <param name="dateTime">The current date.</param>
        private void SetDate(DateTime dateTime)
        {
            int jdn = JulianDayNumber.ToJdn(dateTime);
            if (dateTime.Hour < 8)
                jdn--;
            Yyyymmdd = JulianDayNumber.ToYyyymmdd(jdn);
            char separatorChar = Path.DirectorySeparatorChar;
            var stringBuilder = new StringBuilder(512);
            stringBuilder.Append(separatorChar);
            stringBuilder.Append(Yyyymmdd.Substring(0, 4));
            stringBuilder.Append(separatorChar);
            stringBuilder.Append(
                JulianDayNumber.IsSaturday(jdn) ? "saturdays" :
                JulianDayNumber.IsSunday(jdn) ? "sundays" :
                Euronext.IsHoliday(jdn) ? "holidays" :
                "workdays");
            stringBuilder.Append(separatorChar);
            stringBuilder.Append(Yyyymmdd);
            stringBuilder.Append(separatorChar);
            DownloadRepositorySuffix = stringBuilder.ToString();
            int year, month, day;
            JulianDayNumber.ToYmd(jdn, out year, out month, out day);
            MillisecondsSince1970 = MillisecondsSinceBegin1970(new DateTime(year, month, day));
        }

        /// <summary>
        /// Sets the current date.
        /// </summary>
        /// <param name="yyyymmdd">The current date.</param>
        public void SetDate(string yyyymmdd)
        {
            Yyyymmdd = yyyymmdd;
            DownloadRepositorySuffix = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        #region VerifyDirectory
        /// <summary>
        /// Ensures that a directory exists and has trailing separator.
        /// </summary>
        /// <param name="directory">A directory.</param>
        /// <returns>The directory with trailing separator.</returns>
        static private string VerifyDirectory(string directory)
        {
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) &&
                !directory.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                directory = string.Concat(directory, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }
        #endregion

        #region VerifyFile
        /// <summary>
        /// Ensures that the parent directory of a file exists.
        /// </summary>
        /// <param name="file">A file.</param>
        static public void VerifyFile(string file)
        {
            if (!File.Exists(file))
            {
                var fileInfo = new FileInfo(file);
                DirectoryInfo directoryInfo = fileInfo.Directory;
                if (null != directoryInfo)
                    VerifyDirectory(directoryInfo.FullName);
            }
        }
        #endregion
    }
}
