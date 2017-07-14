using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Configuration;
using Microsoft.SqlServer.Management.Smo;

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
                this.TargetServer = new Server(GetConnectionStringProperty("Data Source"));
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

        public void CreateFakeTableShell(string schemaAndTableName, Boolean keepIdentity = false)
        {
            FakeTable.CreateShell(this.TargetServer, this.DatabaseName, schemaAndTableName, keepIdentity);
        }

        public object DropFakeTable(string schemaAndTableName, string getActualSql = "")
        {
            object result = "";
            if (!String.IsNullOrEmpty(getActualSql))
            {
                result = GetActual(getActualSql);
            }
            FakeTable.Drop(this.TargetServer, this.DatabaseName, schemaAndTableName);

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
