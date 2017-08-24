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

            if (database.Tables[$"{tableName}_Faked", schemaName] != null)
            {
                Console.WriteLine($"Table: {tableName} has already been faked, dropping and restoring...");
                FakeTable.Drop(server, databaseName, schemaAndTableName);
            }

            Table tableToFake = database.Tables[tableName, schemaName];
            if (tableToFake == null)
            {
                throw new Exception($"Error creating fake table:  Table not found: {schemaAndTableName}");
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

            try
            {
                tableToFake.Rename($"{tableName}_Faked");
                fakeTable.Create();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create fake table '{schemaAndTableName}': {e.Message}");
            }
        }

        internal static void Drop(Server server, string databaseName, string schemaAndTableName)
        {

            try
            {
                string schemaName = SqlTestTable.GetSchemaName(schemaAndTableName);
                string tableName = SqlTestTable.GetTableName(schemaAndTableName);
                Database database = server.Databases[databaseName];
                if (database.Tables[$"{tableName}_Faked", schemaName] != null)
                {
                    database.Tables[tableName, schemaName].DropIfExists();
                    database.Tables[$"{tableName}_Faked", schemaName].Rename(tableName);
                }
            }
            catch (Exception e)
            {

                throw new Exception($"Drop table failed for Server: '{server.Name}', Database: '{databaseName}', Table: '{schemaAndTableName}' with the error: {e.Message}, Source: {e.Source}");
            }
        }

    }
}
