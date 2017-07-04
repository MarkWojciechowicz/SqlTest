using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using System.Configuration;
using System.Data.OleDb;

namespace SqlTest
{
    public class FakeTable
    {

        public static void CreateShell(string dbName, string schemaAndTableName, Boolean keepIdentity = false)
        {
            Server server = GetServerName();
            string schemaName = GetSchemaName(schemaAndTableName);
            string tableName = GetTableName(schemaAndTableName);
            Database database = server.Databases[dbName];
            Table tableToFake = database.Tables[tableName, schemaName];
            if(tableToFake == null)
            {
                if (database.Tables[$"{tableName}_Faked", schemaName] != null)
                {
                    throw new Exception($"Table already faked: {schemaAndTableName}.  Rename original and drop fake.");
                }
                else
                {
                    throw new Exception($"Table not found: {schemaAndTableName}");
                }
            }
            Table fakeTable = new Table(database, tableName, schemaName);

            foreach(Column column in tableToFake.Columns)
            {
                Column copyofCol = new Column(fakeTable, column.Name, column.DataType);
                if(keepIdentity)
                {
                    copyofCol.Identity = column.Identity;
                }
                if (column.DefaultConstraint != null)
                {
                    copyofCol.AddDefaultConstraint($"{column.DefaultConstraint.Name}_fake");
                    copyofCol.DefaultConstraint.Text = column.DefaultConstraint.Text;
                }
                fakeTable.Columns.Add(copyofCol);
            }
            tableToFake.Rename($"{tableName}_Faked");
            fakeTable.Create();
        }

        public static object Drop(string dbName, string schemaAndTableName, string getActual)
        {
            // TODO: implement fake table drop
            object Result =  "";
            return Result;
        }

        private static Server GetServerName()
        {
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["testTarget"].ConnectionString;
                OleDbConnectionStringBuilder sb = new OleDbConnectionStringBuilder(conn);
                return new Server(sb.DataSource);
            }
            catch(NullReferenceException)
            {
                throw new NullReferenceException("App.Config setting not found 'testTarget', be sure to add this to connectionStrings.");
            }
        }

        private static string GetSchemaName(string schemaAndTableName)
        {
            string[] schema = schemaAndTableName.Split('.');
            if(schema.Count() == 1)
            {
                return "dbo";
            }
            return schema[0];
        }

        private static string GetTableName(string schemaAndTableName)
        {
            string[] table = schemaAndTableName.Split('.');
            if (table.Count() == 1)
            {
                return table[0];
            }
            return table[1];
        }
    }
}
