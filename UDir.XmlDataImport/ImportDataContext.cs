using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Oracle.ManagedDataAccess.Client;

namespace UDir.XmlDataImport
{
    public interface IImportDataContext
    {        
        int ExecBatch(List<string> batch);
        object ExecuteScalar(string sql);
        int GetCount(List<string> batch);
    }
    
    class ImportDataContext : IImportDataContext
    {
        private readonly Database _db;

        public ImportDataContext()
        {
            var connString = ConnectionStringParser.parseConnectionString(Settings.ConnString);

            if (Settings.DbVendor.ToLower() == "oracle")
            {
                _db = new GenericDatabase(connString, new OracleClientFactory());
            }
            else
            {
                _db = new SqlDatabase(connString);
            }
        }
        public int GetCount(List<string> batch)
        {
            int count = 0;
            using (DbConnection conn = _db.CreateConnection())
            {
                conn.Open();
                batch.ForEach(statement =>
                {
                    object result = _db.ExecuteScalar(CommandType.Text, statement);
                    count += int.Parse(result.ToString());
                });
            }

            return count;
        }

        public int ExecBatch(List<string> batch)
        {
            int changedCount = 0;
            using (DbConnection conn = _db.CreateConnection())
            {
                conn.Open();
                var trans = conn.BeginTransaction();

                try
                {
                    batch.ForEach(statement =>
                    {
                        changedCount += Exec(trans, statement);
                    });

                    // Commit the transaction.
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();

                    throw;
                }                
            }

            return changedCount;
        }

        public int Exec(DbTransaction trans, string sql)
        {
            return _db.ExecuteNonQuery(trans, CommandType.Text, sql.Trim());
        }

        public object ExecuteScalar(string sql)
        {
            return _db.ExecuteScalar(CommandType.Text, sql);
        }
    }
}
