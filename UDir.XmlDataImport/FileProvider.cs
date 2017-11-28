using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UDir.XmlDataImport
{
    interface IFileProvider
    {
        List<string> GetFiles(params string[] path);
    }

    class FileProvider : IFileProvider
    {
        const string XmlDataExtension = ".xmld";

        public List<string> GetFiles(params string[] paths)
        {
            var files = AddPaths(paths);

            var filteredFiles = files.Where(f => f.ToLower().EndsWith(XmlDataExtension)).ToList();

            filteredFiles.Sort();

            return filteredFiles;
        }

        private static List<string> AddPaths(string[] paths)
        {
            var files = new List<string>();

            foreach (var path in paths)
            {
                bool found = false;

                if (Directory.Exists(path))
                {
                    found = true;

                    files.AddRange(Directory.GetFiles(path).ToList());
                }

                if (File.Exists(path))
                {
                    found = true;

                    files.Add(path);
                }

                if(!found)
                {
                    throw new FileNotFoundException("Could not find path: {0}", path);
                }
            }
            
            return files;
        }
    }
}
