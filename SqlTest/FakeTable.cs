using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTest
{
    public class FakeTable
    {

        public void Create(string dbName, string schemaAndTableName, Boolean exactCopy = false, Boolean dropExisting = false)
        {
            // TODO: implement fake table as shell
        }

        public object Drop(string dbName, string schemaAndTableName)
        {
            // TODO: implement fake table drop
            object Result =  "";
            return Result;
        }

    }
}
