using System;
using Microsoft.SqlServer.Management.Smo;

namespace SqlTest
{
    public class FakeView
    {
        internal static void Create(Server server, string databaseName, string schemaAndVew)
        {
            string schemaName = SqlTestTable.GetSchemaName(schemaAndVew);
            string viewName = SqlTestTable.GetTableName(schemaAndVew);
            Database database = server.Databases[databaseName];
            View viewToBeFaked = database.Views[viewName, schemaName];

            if (database.Tables[$"{viewName}_Faked", schemaName] != null)
            {
                Console.WriteLine($"View: {schemaAndVew} has already been faked, dropping and restoring...");
                FakeView.Drop(server, databaseName, schemaAndVew);
            }

            if (viewToBeFaked == null)
            {
                throw new Exception($"Error creating fake view:  View not found: {schemaAndVew}");
            }

            Table fakeTable = new Table(database, viewName, schemaName);

            foreach (Column column in viewToBeFaked.Columns)
            {
                Column copyofCol = new Column(fakeTable, column.Name, column.DataType);
                fakeTable.Columns.Add(copyofCol);
            }
            viewToBeFaked.Rename($"{viewName}_Faked");
            fakeTable.Create();
        }

        internal static void Drop(Server server, string databaseName, string schemaAndView)
        {
            string schemaName = SqlTestTable.GetSchemaName(schemaAndView);
            string viewName = SqlTestTable.GetTableName(schemaAndView);
            Database database = server.Databases[databaseName];
            database.Tables[viewName, schemaName].DropIfExists();
            database.Views[$"{viewName}_Faked", schemaName].Rename(viewName);
        }
    }
}
