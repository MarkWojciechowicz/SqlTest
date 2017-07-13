using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Configuration;
using System.Reflection;

namespace SqlTest
{
    public class SqlTestTarget
    {
        string connectionString { get; set; }

        public SqlTestTarget(string connectionStringConfigName)
        {
            try
            {
                string exeConfigPath = System.Reflection.Assembly.GetCallingAssembly().Location;
                var config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
                var value = config.ConnectionStrings.ConnectionStrings[connectionStringConfigName].ConnectionString;
                this.connectionString = String.IsNullOrEmpty(value) ? "" : value;
            }

            catch (NullReferenceException)
            {
                throw new NullReferenceException($"ConnectionString Setting not found in app.config: '{connectionStringConfigName}.'");
            }
        }

        public void ExecuteAdhoc(string sql)
        {
            try
            {
                OleDbConnection conn = new OleDbConnection(this.connectionString);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                conn.Open();
                using (cmd)
                {
                    cmd.ExecuteNonQuery();
                }
            }
            
            catch(Exception e)
            {
                throw new Exception( $"Error executing Adhoc Sql: {e.Message}. Statement: {sql}");
            }
        }

        public object GetActual(string sql)
        {
            object result= "";
            try
            {
                OleDbConnection conn = new OleDbConnection(this.connectionString);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                conn.Open();
                using (cmd)
                {
                    result = cmd.ExecuteScalar();
                }
            }

            catch (Exception e)
            {
                throw new Exception($"Error executing GetValue: {e.Message}. Statement: {sql}");
            }
            return result;
        }

        public void InsertOneRow(string dbName, string schemaAndTable)
        {
            //TODO: Implement insert a row
            throw new NotImplementedException();
        }

        
    }
}
