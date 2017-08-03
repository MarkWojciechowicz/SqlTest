using System;
using System.Configuration;
using System.Data.OleDb;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SqlTest
{
    public class SqlTestTarget
    {
        internal string ConnectionString { get; set; }
        internal string ConfigurationName { get; set; }
        internal Server TargetServer { get; set; }
        internal string DatabaseName { get; set; }

        public SqlTestTarget(string connectionStringConfigName)
        {
            try
            {
                string exeConfigPath = System.Reflection.Assembly.GetCallingAssembly().Location;
                var config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
                var value = config.ConnectionStrings.ConnectionStrings[connectionStringConfigName].ConnectionString;
                this.ConnectionString = String.IsNullOrEmpty(value) ? "" : value;
                this.ConfigurationName = connectionStringConfigName;         
                this.TargetServer = GetServer();
                this.DatabaseName = GetConnectionStringProperty("Initial Catalog");
               
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
                OleDbConnection conn = new OleDbConnection(this.ConnectionString);
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
                OleDbConnection conn = new OleDbConnection(this.ConnectionString);
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
            throw new NotImplementedException("Sorry about that, haven't gotten to the InsertOneRow method yet");
        }


        internal string GetConnectionStringProperty(string property)
        {
            OleDbConnectionStringBuilder sb = new OleDbConnectionStringBuilder(this.ConnectionString);
            object value = null;
            if(!sb.TryGetValue(property, out value))
            {
                throw new Exception($"The property: '{property}' was not found while searching the connection string in the configuration: '{this.ConfigurationName}'");
            }
            return value.ToString();

        }

        internal Server GetServer()
        {
            try
            {
                string[] connectionString = this.ConnectionString.Split(';');
                List<string> finalString = new List<string>();
                foreach (string s in connectionString)
                {
                    if (!s.Contains("Provider"))
                    {
                        finalString.Add(s);
                    }
                }

                SqlConnection sqlConn = new SqlConnection(String.Join(";", finalString));
                ServerConnection serverConn = new ServerConnection(sqlConn);
                return new Server(serverConn);
            }

            catch(Exception e)
            {
                throw new Exception($"Failed to create SMO Server.  The following error was thrown:  '{e.Message}'");
            }
        }

        public void CreateFakeTableShell(string schemaAndTableName, Boolean keepIdentity = false)
        {
            try
            {
                FakeTable.CreateShell(this.TargetServer, this.DatabaseName, schemaAndTableName, keepIdentity);
            }
            catch (Exception e)
            {

                throw new Exception($"CreateFakeTableShell failed for '{schemaAndTableName}' with the error: {e.Message}; Stacktace: {e.StackTrace}");
            }
        }

        public object DropFakeTable(string schemaAndTableName, string getActualSql = "")
        {
            object result = "";
            try
            {
                if (!String.IsNullOrEmpty(getActualSql))
                {
                    result = GetActual(getActualSql);
                }
                FakeTable.Drop(this.TargetServer, this.DatabaseName, schemaAndTableName);
            }
            catch (Exception e)
            {

                throw new Exception($"DropFakeTable failed for '{schemaAndTableName}' with the Error: {e.Message}; Stacktrace: '{e.StackTrace}'");
            }

            return result;
        }

        public void CreateFakeView(string schemaAndVew)
        {
            FakeView.Create(this.TargetServer, this.DatabaseName, schemaAndVew);
        }

        public void DropFakeView(string schemaAndView)
        {
            FakeView.Drop(this.TargetServer, this.DatabaseName, schemaAndView);
        }
    }
}
