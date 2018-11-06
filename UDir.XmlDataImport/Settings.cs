#if NETCORE
using Microsoft.Extensions.Configuration;

namespace UDir.XmlDataImport
{
    static class Settings
    {
        private static IConfigurationRoot configuration;
        private static string DBInstanceNameKeyName = "DBInstanceName";
        private static string _dBInstanceName;

        private static string _dbVendor;
        private static string DBVendorKeyName = "DBVendor";

        static Settings()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();            
        }

        /// <summary>
        /// Returns a string from the config file identifying the name of the DB instance.
        /// </summary>                
        public static string DbInstanceName => _dBInstanceName ?? (_dBInstanceName = configuration[DBInstanceNameKeyName]);

        /// <summary>
        /// Returns a string from the config file identifying the name of the DB instance.
        /// </summary>                
        public static string DbVendor => _dbVendor ?? (_dbVendor = configuration[DBVendorKeyName] ?? string.Empty);

        public static string ConnString => configuration.GetConnectionString(DbInstanceName);
    }
}
#endif
#if NETFULL

using System.Configuration;

namespace UDir.XmlDataImport
{
    static class Settings
    {        
        private static string DBInstanceNameKeyName = "DBInstanceName";
        private static string _dBInstanceName;

        private static string _dbVendor;
        private static string DBVendorKeyName = "DBVendor";
        private static string _connString;

        /// <summary>
        /// Returns a string from the config file identifying the key of the connection string  to use.
        /// </summary>                
        public static string DbInstanceName
        {
            get
            {
                return _dBInstanceName ?? (_dBInstanceName =  ConfigurationManager.AppSettings[DBInstanceNameKeyName]);                
            }
        }


        /// <summary>
        /// Returns a string from the config file identifying the name of the database vendor (MSSQL or Oracle).
        //  Value defaults to MSSQL
        /// </summary>
        public static string DbVendor
        {
            get
            {
                return _dbVendor ?? (_dbVendor = ConfigurationManager.AppSettings[DBVendorKeyName] ?? "mssql");
            }
        }

        /// <summary>
        /// Returns connecton string from config file identified by the value provided for DB instance.
        /// </summary>
        public static string ConnString 
        {
            get
            {
                return _connString ?? (_connString = ConfigurationManager.ConnectionStrings[DbInstanceName].ConnectionString);
            }
        }
    }
}
#endif