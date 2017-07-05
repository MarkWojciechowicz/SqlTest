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
            string[] schema = schemaAndTableName.Split('.');
            if (schema.Count() == 1)
            {
                return "dbo";
            }
            return schema[0];
        }

        internal static string GetTableName(string schemaAndTableName)
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
