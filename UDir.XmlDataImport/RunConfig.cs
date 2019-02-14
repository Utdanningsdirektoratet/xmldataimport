using System.Collections.Generic;

namespace UDir.XmlDataImport
{
    public class RunConfig
    {
        public RunConfig()
        {
            Variables = new Dictionary<string, object>();
            Paths = new List<string>();
        }
        public bool IgnoreNoChange { get; set; }
        public Dictionary<string, object> Variables { get; set; }

        public string ConnectionStringId { get; set; }

        public string DbVendor { get; set; }

        public List<string> Paths { get; set; }
    }
}
