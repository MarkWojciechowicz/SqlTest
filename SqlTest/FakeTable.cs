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
            Server server = SqlTestServer.GetServerName();
            string schemaName = SqlTestTable.GetSchemaName(schemaAndTableName);
            string tableName = SqlTestTable.GetTableName(schemaAndTableName);
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

        public static object Drop(string dbName, string schemaAndTableName, string getActualSql = "")
        {
            object result =  "";
            if(!String.IsNullOrEmpty(getActualSql))
            {
                result = Sql.GetActual(getActualSql);
            }

            Server server = SqlTestServer.GetServerName();
            string schemaName = SqlTestTable.GetSchemaName(schemaAndTableName);
            string tableName = SqlTestTable.GetTableName(schemaAndTableName);
            Database database = server.Databases[dbName];
            database.Tables[tableName, schemaName].DropIfExists();
            database.Tables[$"{tableName}_Faked", schemaName].Rename(tableName);

            return result;
        }

    }
}
