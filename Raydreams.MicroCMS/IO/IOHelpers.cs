using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;

namespace Raydreams.MicroCMS.IO
{
    /// <summary>IO Helpers are just IO utility classes for common IO opertations.</summary>
    public static class IOHelpers
    {
        #region [ Methods ]

        /// <summary>Path to the user's desktop folder</summary>
        public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>Return all files in the specified folder path that match the specified filter. Top level only - no children</summary>
        /// <param name="dir"></param>
        /// <param name="filter">Send null to get every file</param>
        /// <returns></returns>
        public static FileInfo[] GetAllFiles(this DirectoryInfo dir, string filter, SearchOption op = SearchOption.TopDirectoryOnly)
        {
            // no filter will match everything
            if ( String.IsNullOrWhiteSpace(filter) )
                filter = "*";

            if (dir == null || !dir.Exists)
                return new FileInfo[] { };

            return dir.GetFiles(filter, op);
        }

        /// <summary>Returns the full path to the last created file with the given filter. Only searches in the input path, no children</summary>
        /// <param name="filter">Filter to use on filtering files such as MyFile*</param>
        /// <returns>Full file path</returns>
        public static FileInfo LatestFile(this DirectoryInfo dir, string filter)
        {
            if (dir == null || !dir.Exists)
                return null;

            if (String.IsNullOrWhiteSpace(filter))
                filter = "*";

            return dir.GetFiles(filter).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        }

        /// <summary>Given just the parent directory path and full file path, returns JUST the part of the path after the project directory name including the file name. No root folder or project name is included.</summary>
        /// <returns></returns>
        /// <remarks>
        /// dir = /users/bob/Documents/
        /// file = /users/bob/Documents/subfolder/file1.txt
        /// </remarks>
        public static string PathDiff(this DirectoryInfo dir, FileInfo fi, bool includeFile = true)
        {
            int len = fi.FullName.Length - dir.FullName.Length;
            if (!includeFile)
                len -= fi.Name.Length;

            // check for overlap
            int begin = fi.FullName.IndexOf(dir.FullName, StringComparison.InvariantCultureIgnoreCase);

            if ( begin < 0 )
                return null;

            return fi.FullName.Substring(dir.FullName.Length, len);
        }

        /// <summary>Reads every line into a string array</summary>
        /// <param name="path">Path to a physical file to test</param>
        /// <remarks>Just move this to IO Helpers</remarks>
        public static List<string> ReadFile(string path, bool trimLines = false)
        {
            List<string> data = new List<string>();

            FileInfo fi = new FileInfo(path);

            if ( !fi.Exists )
                return data;

            data = File.ReadAllLines(fi.FullName).ToList();

            if (trimLines)
                data.ForEach(s => s = s.Trim());

            return data;
        }

        /// <summary></summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static RawFileWrapper ReadFile(string path)
        {
            RawFileWrapper file = new RawFileWrapper();

            FileInfo fi = new FileInfo(path);

            if (!fi.Exists)
                return file;

            file.Filename = fi.Name;
            file.Data = File.ReadAllBytes(fi.FullName);
            file.ContentType = MimeTypeMap.GetMimeType(fi.Extension);

            return file;
        }

        /// <summary>Makes a copy of the source file with the same name + the specified suffix in the destination folder</summary>
        /// <param name="suffix">an additional suffix to add to the end of the file name</param>
        public static int CopyFile(string srcPath, string destPath, string suffix = "", bool overwrite = true)
        {
            if (String.IsNullOrWhiteSpace(suffix))
                suffix = String.Empty;

            FileInfo fi = new FileInfo(srcPath);
            DirectoryInfo di = new DirectoryInfo(destPath);

            if (!fi.Exists || !di.Exists)
                return 0;

            // Rename the file
            var filePart = Path.GetFileNameWithoutExtension(fi.FullName);
            var filePartExt = Path.GetExtension(fi.FullName);
            var targetPath = Path.Combine(destPath, String.Format("{0}{1}{2}", filePart, suffix, filePartExt));

            try
            {
                File.Copy(fi.FullName, targetPath, overwrite);
            }
            catch (System.Exception)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>Moves a file from one folder to another the <see cref="SourceFileName"/> to the <see cref="ArchiveFolder"/></summary>
        /// <param name="suffix">an additional suffix to add to the end of the file name</param>
        public static int MoveFile(string srcPath, string destPath, string suffix = "_bkup")
        {
            if (String.IsNullOrWhiteSpace(suffix))
                suffix = String.Empty;

            FileInfo fi = new FileInfo(srcPath);
            DirectoryInfo di = new DirectoryInfo(destPath);

            if (!fi.Exists || !di.Exists)
                return 0;

            // Rename the file
            var filePart = Path.GetFileNameWithoutExtension(fi.FullName);
            var filePartExt = Path.GetExtension(fi.FullName);
            var targetPath = Path.Combine(destPath, String.Format("{0}{1}{2}", filePart, suffix, filePartExt));

            try
            {
                File.Move(fi.FullName, targetPath);
            }
            catch (System.Exception)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>Deletes the directory at the specified path if you have persmission</summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void DeleteDirectory(string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                Directory.Delete(path, true);
        }

        /// <summary>Creates a directory at the specified path if you have permission</summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DirectoryInfo CreateDirectory(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                return null;

            return Directory.CreateDirectory(path);
        }

        /// <summary>Loads an XML file into a Dataset given the specified physical file path</summary>
        /// <param name="name">Optional name to set on the DataSet</param>
        /// <returns>A populated DataSet object</returns>
        public static DataSet LoadXMLFile(string srcPath, string name = null)
        {
            // also need to check the file exists and it is an XML file
            if (String.IsNullOrWhiteSpace(srcPath))
                return new DataSet();

            FileInfo fi = new FileInfo(srcPath);

            // check it is an XML file
            if (!fi.Exists || fi.Extension.ToLower() != ".xml")
                return new DataSet();

            // name is optional
            name = (String.IsNullOrWhiteSpace(name)) ? Guid.NewGuid().ToString() : name.Trim();

            // create the data set
            DataSet ds = new DataSet(name);
            _ = ds.ReadXml(srcPath, XmlReadMode.InferSchema);

            // return
            return ds;
        }

        #endregion [ Methods ]
    }
}
