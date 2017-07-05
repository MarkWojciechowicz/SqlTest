using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace SqlTest
{
    public class FakeView
    {
        public static void Create(string dbName, string schemaAndVew)
        {
            Server server = SqlTestServer.GetServerName();
            string schemaName = SqlTestTable.GetSchemaName(schemaAndVew);
            string viewName = SqlTestTable.GetTableName(schemaAndVew);
            Database database = server.Databases[dbName];
            View viewToBeFaked = database.Views[viewName, schemaName];
            if (viewToBeFaked == null)
            {
                if (database.Tables[$"{viewName}_Faked", schemaName] != null)
                {
                    throw new Exception($"View is already faked: {schemaAndVew}.  Rename original and drop fake.");
                }
                else
                {
                    throw new Exception($"View not found: {schemaAndVew}");
                }
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

        public static void Drop(string dbName, string schemaAndView)
        {
            Server server = SqlTestServer.GetServerName();
            string schemaName = SqlTestTable.GetSchemaName(schemaAndView);
            string viewName = SqlTestTable.GetTableName(schemaAndView);
            Database database = server.Databases[dbName];
            database.Tables[viewName, schemaName].DropIfExists();
            database.Views[$"{viewName}_Faked", schemaName].Rename(viewName);
        }
    }
}
