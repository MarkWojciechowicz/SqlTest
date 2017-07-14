using System;
using Microsoft.SqlServer.Management.Smo;

namespace SqlTest
{
    public class FakeTable
    {

        internal static void CreateShell(Server server, string databaseName, string schemaAndTableName, Boolean keepIdentity = false)
        {
             string schemaName = SqlTestTable.GetSchemaName(schemaAndTableName);
            string tableName = SqlTestTable.GetTableName(schemaAndTableName);
            Database database = server.Databases[databaseName];
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

        internal static void Drop(Server server, string databaseName, string schemaAndTableName)
        {
            string schemaName = SqlTestTable.GetSchemaName(schemaAndTableName);
            string tableName = SqlTestTable.GetTableName(schemaAndTableName);
            Database database = server.Databases[databaseName];
            database.Tables[tableName, schemaName].DropIfExists();
            database.Tables[$"{tableName}_Faked", schemaName].Rename(tableName);
        }

    }
}
