using System.Collections.Generic;

namespace UDir.XmlDataImport
{
    class SqlPack
    {
        public SqlPack()
        {
            StartupScripts = new List<string>();
            SetupScripts = new List<string>();
            TearDownScripts = new List<string>();
            Inserts = new List<string>();
        }

        public List<string> StartupScripts { get; set; }
        public List<string> SetupScripts { get; set; }
        public List<string> Inserts { get; set; }
        public List<string> TearDownScripts { get; set; }
    }
}
