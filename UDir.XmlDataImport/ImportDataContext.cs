using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;

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
            // Configure the DatabaseFactory to read its configuration from the .config file 
            DatabaseProviderFactory factory = new DatabaseProviderFactory();

            // Create a Database object from the factory using the connection string name. 
            _db = factory.Create(Settings.DbInstanceName);
        }

        public int GetCount(List<string> batch)
        {
            int count = 0;
            using (DbConnection conn = _db.CreateConnection())
            {
                conn.Open();
                batch.ForEach(statement =>
                {
                    count += (int) _db.ExecuteScalar(CommandType.Text, statement);
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
            return _db.ExecuteNonQuery(trans, CommandType.Text, sql);
        }

        public object ExecuteScalar(string sql)
        {
            return _db.ExecuteScalar(CommandType.Text, sql);
        }
    }
}
