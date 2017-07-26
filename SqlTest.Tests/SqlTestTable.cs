using NUnit.Framework;

namespace SqlTest.Tests
{
    [TestFixture]
    class SqlTestTable
    {
        [TestCase("stage.MyTable", "stage")]
        [TestCase("[stage].[MyTable]", "stage")]
        [TestCase("MyTable", "dbo")]
        [TestCase("[MyTable]", "dbo")]
        public void SqlTestTable_GetSchemaName_ReturnsParsedValue(string schemaAndTable, string expected)
        {
            var actual = SqlTest.SqlTestTable.GetSchemaName(schemaAndTable);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("stage.MyTable")]
        [TestCase("[stage].[MyTable]")]
        [TestCase("MyTable")]
        [TestCase("[MyTable]")]
        public void SqlTestTable_GetTableName_ReturnsParsedValue(string schemaAndTable)
        {
            var actual = SqlTest.SqlTestTable.GetTableName(schemaAndTable);
            Assert.That(actual, Is.EqualTo("MyTable"));
        }
    }
}
