using System;
using Microsoft.SqlServer.Management.Smo;

namespace SqlTest
{
    public class FakeView
    {
        internal static void Create(Server server, string databaseName, string schemaAndView)
        {
            try
            {
                string schemaName = SqlTestTable.GetSchemaName(schemaAndView);
                string viewName = SqlTestTable.GetTableName(schemaAndView);
                Database database = server.Databases[databaseName];
                View viewToBeFaked = database.Views[viewName, schemaName];

                if (database.Views[$"{viewName}_Faked", schemaName] != null)
                {
                    Console.WriteLine($"View: {schemaAndView} has already been faked, dropping and restoring...");
                    FakeView.Drop(server, databaseName, schemaAndView);
                }

                if (viewToBeFaked == null)
                {
                    throw new Exception($"Error creating fake view:  View not found: {schemaAndView}");
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
            catch (Exception e)
            {
                throw new Exception($"Error creating faked view '{schemaAndView}':  {e.Message}");
            }
        }


        internal static void Drop(Server server, string databaseName, string schemaAndView)
        {
            try
            {
                string schemaName = SqlTestTable.GetSchemaName(schemaAndView);
                string viewName = SqlTestTable.GetTableName(schemaAndView);
                Database database = server.Databases[databaseName];

                if (database.Tables[viewName, schemaName] == null)
                {
                    throw new NullReferenceException($"Error dropping fake view '{schemaAndView}':  Does not exist"); 
                }
                database.Tables[viewName, schemaName].DropIfExists();
                database.Views[$"{viewName}_Faked", schemaName].Rename(viewName);
            }

            catch(NullReferenceException e)
            {
                throw e;    
            }
           
            catch(Exception e)
            {
                throw new Exception($"Error dropping faked view '{schemaAndView}':  {e.Message}");
            }
        }
    }
}
