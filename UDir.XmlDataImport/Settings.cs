#if NETCORE
using Microsoft.Extensions.Configuration;

namespace UDir.XmlDataImport
{
    public class Settings
    {
        private IConfigurationRoot configuration;
        private string DBInstanceNameKeyName = "DBInstanceName";        

        private string _dbVendor;
        private string DBVendorKeyName = "DBVendor";
        private string _connectionStringId;

        public Settings()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();            
        }

        public Settings(string connectionStringId, string dbVendor): this()
        {
            _connectionStringId = connectionStringId;
            _dbVendor = dbVendor;
        }

        /// <summary>
        /// Returns a string from the config file identifying the name of the DB instance.
        /// </summary>                
        public string DbInstanceName => _connectionStringId ?? (_connectionStringId = configuration[DBInstanceNameKeyName]);

        /// <summary>
        /// Returns a string from the config file identifying the name of the DB instance.
        /// </summary>                
        public string DbVendor
        {
            get
            {
                return _dbVendor ?? (_dbVendor = configuration[DBVendorKeyName] ?? string.Empty);
            }
            set { _dbVendor = value; }
        }

        public string ConnString => configuration.GetConnectionString(DbInstanceName);
    }
}
#endif
#if NETFULL

using System.Configuration;

namespace UDir.XmlDataImport
{
    public class Settings
    {
        public Settings()
        {
        }

        public Settings(string connectionStringId, string dbVendor)
        {
            _connectionStringId = connectionStringId;
            _dbVendor = dbVendor;
        }
        private string DBInstanceNameKeyName = "DBInstanceName";

        private string _dbVendor;
        private string DBVendorKeyName = "DBVendor";
        private string _connectionStringId;

        /// <summary>
        /// Returns a string from the config file identifying the key of the connection string  to use.
        /// </summary>                
        public string DbInstanceName
        {
            get
            {
                return _connectionStringId ?? (_connectionStringId =  ConfigurationManager.AppSettings[DBInstanceNameKeyName]);                
            }
        }

        /// <summary>
        /// Returns a string from the config file identifying the name of the database vendor(MSSQL or Oracle).
        /// Value defaults to MSSQL
        /// </summary>        
        public string DbVendor
        {
            get
            {
                return _dbVendor ?? (_dbVendor = ConfigurationManager.AppSettings[DBVendorKeyName] ?? "mssql");
            }
        }

        /// <summary>
        /// Returns connecton string from config file identified by the value provided for DB instance.
        /// </summary>
        public string ConnString => ConfigurationManager.ConnectionStrings[DbInstanceName].ConnectionString;
    }
}
#endif