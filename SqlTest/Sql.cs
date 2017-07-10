using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Configuration;

namespace SqlTest
{
    public class Sql
    {
        public static void ExecuteAdhoc(string sql)
        {
            try
            {
                OleDbConnection conn = new OleDbConnection(ConfigurationManager.ConnectionStrings["testTarget"].ConnectionString);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                conn.Open();
                using (cmd)
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("App.Config setting not found 'testTarget', be sure to add this to connectionStrings.");
            }
            catch(Exception e)
            {
                throw new Exception( $"Error executing Adhoc Sql: {e.Message}. Statement: {sql}");
            }
        }

        public static object GetActual(string sql)
        {
            object result= "";
            try
            {
                OleDbConnection conn = new OleDbConnection(ConfigurationManager.ConnectionStrings["testTarget"].ConnectionString);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                conn.Open();
                using (cmd)
                {
                    result = cmd.ExecuteScalar();
                }
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("App.Config setting not found 'testTarget', be sure to add this to connectionStrings.");
            }
            catch (Exception e)
            {
                throw new Exception($"Error executing GetValue: {e.Message}. Statement: {sql}");
            }
            return result;
        }

        public static void InsertOneRow(string dbName, string schemaAndTable)
        {
            //TODO: Implement insert a row
            throw new NotImplementedException();
        }
    }
}
