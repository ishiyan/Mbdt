using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;

namespace mbdt.Utils
{
    /// <summary>
    /// Compression / decompression utilities.
    /// </summary>
    static class Packager
    {
        #region ZipCsvDirectory
        /// <summary>
        /// Makes a recursive zip of all CSV files in a directory.
        /// </summary>
        /// <param name="zip">A file name of the zip file.</param>
        /// <param name="directory">A directory.</param>
        /// <param name="delete">Indicates the deletion of the directory after processing.</param>
        public static void ZipCsvDirectory(string zip, string directory, bool delete)
        {
            ZipHomogenousDirectory(zip, directory, "*.csv*", System.Net.Mime.MediaTypeNames.Text.Plain, delete);
        }
        #endregion

        #region ZipCsv2Directory
        /// <summary>
        /// Makes a recursive zip of all CSV2 files in a directory.
        /// </summary>
        /// <param name="zip">A file name of the zip file.</param>
        /// <param name="directory">A directory.</param>
        /// <param name="delete">Indicates the deletion of the directory after processing.</param>
        public static void ZipCsv2Directory(string zip, string directory, bool delete)
        {
            ZipHomogenousDirectory(zip, directory, "*.csv2*", System.Net.Mime.MediaTypeNames.Text.Plain, delete);
        }
        #endregion

        #region ZipJsDirectory
        /// <summary>
        /// Makes a recursive zip of all JS files in a directory.
        /// </summary>
        /// <param name="zip">A file name of the zip file.</param>
        /// <param name="directory">A directory.</param>
        /// <param name="delete">Indicates the deletion of the directory after processing.</param>
        public static void ZipJsDirectory(string zip, string directory, bool delete)
        {
            ZipHomogenousDirectory(zip, directory, "*.js*", System.Net.Mime.MediaTypeNames.Text.Plain, delete);
        }
        #endregion

        #region ZipTxtDirectory
        /// <summary>
        /// Makes a recursive zip of all TXT files in a directory.
        /// </summary>
        /// <param name="zip">A file name of the zip file.</param>
        /// <param name="directory">A directory.</param>
        /// <param name="delete">Indicates the deletion of the directory after processing.</param>
        public static void ZipTxtDirectory(string zip, string directory, bool delete)
        {
            ZipHomogenousDirectory(zip, directory, "*.txt*", System.Net.Mime.MediaTypeNames.Text.Plain, delete);
        }
        #endregion

        #region ZipXmlDirectory
        /// <summary>
        /// Makes a recursive zip of all XML files in a directory.
        /// </summary>
        /// <param name="zip">A file name of the zip file.</param>
        /// <param name="directory">A directory.</param>
        /// <param name="delete">Indicates the deletion of the directory after processing.</param>
        public static void ZipXmlDirectory(string zip, string directory, bool delete)
        {
            ZipHomogenousDirectory(zip, directory, "*.xml*", System.Net.Mime.MediaTypeNames.Text.Plain, delete);
        }
        #endregion

        #region ZipHomogenousDirectory
        private static void ZipHomogenousDirectory(string zip, string directory, string filePattern, string fileType, bool delete)
        {
            directory = TrimTrailingDirectorySeparator(directory);
            using (var package = (ZipPackage)Package.Open(zip, FileMode.OpenOrCreate))
            {
                PackageTextFiles(package, directory, Path.GetFileName(directory), filePattern, fileType);
            }
            if (delete)
                Directory.Delete(directory, true);
        }
        #endregion

        #region TrimTrailingDirectorySeparator
        private static string TrimTrailingDirectorySeparator(string directory)
        {
            if (directory.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                directory = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                directory = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            return directory;
        }
        #endregion

        #region PackageTextFiles
        private static void PackageTextFiles(Package package, string directory, string prefix, string filePattern, string contentType)
        {
            prefix = string.Concat(prefix, Path.DirectorySeparatorChar);
            directory = string.Concat(directory, Path.DirectorySeparatorChar);
            foreach (string file in Directory.GetFiles(directory, filePattern))
            {
                Uri partUri = PackUriHelper.CreatePartUri(new Uri(string.Concat(prefix, Path.GetFileName(file)), UriKind.Relative));
                PackagePart part = package.CreatePart(partUri, contentType, CompressionOption.Maximum);
                if (null != part)
                {
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        CopyStream(fileStream, part.GetStream());
                }
            }
            foreach (string dir in Directory.GetDirectories(directory))
                PackageTextFiles(package, dir, string.Concat(prefix, Path.GetFileName(dir)), filePattern, contentType);
        }
        #endregion

        #region CopyStream
        private static void CopyStream(Stream source, Stream target)
        {
            const int bufferSize = 0x1000;
            var buffer = new byte[bufferSize];
            int bytesRead;
            while (0 < (bytesRead = source.Read(buffer, 0, bufferSize)))
                target.Write(buffer, 0, bytesRead);
        }
        #endregion

    }
}
