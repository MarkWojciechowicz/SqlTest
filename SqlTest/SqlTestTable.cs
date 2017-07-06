using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTest
{
    class SqlTestTable
    {
        internal static string GetSchemaName(string schemaAndTableName)
        {
            string[] schemas = schemaAndTableName.Split('.');
            string schema = schemas.Count() == 1 ? "dbo" : schemas[0];
            return schema.Replace("[", "").Replace("]", "");
        }

        internal static string GetTableName(string schemaAndTableName)
        {
            string[] tables = schemaAndTableName.Split('.');
            string table = tables.Count() == 1 ? tables[0] : tables[1];
            return table.Replace("[", "").Replace("]", "");
        }

    }
}
