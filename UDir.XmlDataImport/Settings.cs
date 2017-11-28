using System.Configuration;

namespace UDir.XmlDataImport
{
    static class Settings
    {
        private static string DBInstanceNameKeyName = "DBInstanceName";
        private static string _dBInstanceName;

        /// <summary>
        /// Returns a string from the config file identifying the name of the DB instance.
        /// </summary>                
        public static string DbInstanceName
        {
            get
            {
                return _dBInstanceName ?? (_dBInstanceName =  ConfigurationManager.AppSettings[DBInstanceNameKeyName]);                
            }
        }
    }
}
