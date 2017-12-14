using System.Collections.Generic;

namespace UDir.XmlDataImport
{
    class SqlPack
    {
        public SqlPack()
        {
            StartupScripts = new List<string>();
            Variables = new List<string>();
            SetupScripts = new List<string>();
            TearDownScripts = new List<string>();
            Inserts = new List<string>();
        }

        public List<string> Variables { get; }
        public List<string> StartupScripts { get; }
        public List<string> SetupScripts { get; }
        public List<string> Inserts { get; }
        public List<string> TearDownScripts { get; }        
    }
}
