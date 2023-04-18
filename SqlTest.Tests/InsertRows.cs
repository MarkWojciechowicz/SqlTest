using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace SqlTest.Tests
{
    [TestFixture]
    public class InsertRows
    {
        TransactionScope? scope;
        [SupportedOSPlatform("windows")]
        [SetUp]
        public void SetUp()
        {
            scope = new TransactionScope();
            TransactionManager.ImplicitDistributedTransactions = true;

        }
        [TearDown]
        public void TearDown()
        {
            if (Transaction.Current != null && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
            {
                scope?.Dispose();
            }
        }

        [TestCase("VARCHAR(3)", "aaa")]
        [TestCase("VARCHAR(1)", "a")]
        [TestCase("NVARCHAR(3)", "aaa")]
        [TestCase("NVARCHAR(1)", "a")]
        [TestCase("CHAR(3)", "aaa")]
        [TestCase("CHAR(1)", "a")]
        [TestCase("NCHAR(3)", "aaa")]
        [TestCase("NCHAR(1)", "a")]
        [TestCase("text", "aaaaaaaaaaaaaaaa")]
        [TestCase("ntext", "aaaaaaaaaaaaaaaa")]
        [TestCase("sysname", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase("NVARCHAR(MAX)", "aaaaaaaa")]
        [TestCase("VARCHAR(MAX)", "aaaaaaaa")]
        [TestCase("INT", 0)]
        [TestCase("BIGINT", 0)]
        [TestCase("SMALLINT", 0)]
        [TestCase("TINYINT", 0)]
        [TestCase("Numeric(10,0)", 0)]
        [TestCase("Numeric(10,2)", 0)]
        [TestCase("Decimal(10,0)", 0)]
        [TestCase("Decimal(10,2)", 0)]
        [TestCase("money", 0)]
        [TestCase("smallmoney", 0)]
        [TestCase("float", 0)]
        [TestCase("real", 0)]
        [TestCase("bit", false)]
        public void InsertRows_InsertStringAndNumericDataTypes_ColumnReturnsExpectedValue(string datatype, object expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT myCol FROM dbo.DataTypeTest;");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("Date", "2010-01-01")]
        [TestCase("Datetime", "2010-01-01")]
        [TestCase("SmallDatetime", "2010-01-01")]
        [TestCase("Datetime2", "2010-01-01")]
        [TestCase("Datetime2(3)", "2010-01-01")]
        [TestCase("DatetimeOffset", "2010-01-01")]
        public void InsertRows_InsertDateDataTypes_ColumnReturnsExpectedValue(string datatype, DateTime expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT convert(datetime, myCol) as myCol FROM dbo.DataTypeTest;");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("timestamp")]
        [TestCase("INT DEFAULT (1)")]
        [TestCase("VARCHAR(10) DEFAULT ('Hi')")]
        public void InsertRows_ColumnHasDefault_ColumnValueIsNotNull(string datatype)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT 1 FROM dbo.DataTypeTest WHERE myCol Is not null;");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("uniqueidentifier", "EAB932A9-3B1A-460B-9FFD-BAC9F2B95D92")]
        public void InsertRows_InsertUniqueIdentifier_ColumnReturnsExpectedValue(string datatype, Guid expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT myCol FROM dbo.DataTypeTest;");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("binary")]
        [TestCase("binary(2)")]
        [TestCase("varbinary")]
        [TestCase("varbinary(16)")]
        [TestCase("varbinary(max)")]
        public void InsertRows_InsertBinaryTypes_ColumnReturnsExpectedValue(string datatype)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT 1 FROM dbo.DataTypeTest WHERE myCol is not null;");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("Time")]
        public void InsertRows_InsertTime_ColumnReturnsExpectedValue(string datatype)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual("SELECT 1 as myCol FROM dbo.DataTypeTest WHERE myCol is not null;");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("myCol")]
        [TestCase("id")]
        public void InsertRows_TableWithMultipleColumns_EachColumnHasValue(string column)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(id int NOT NULL, myCol varchar(10))");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest");
            var actual = target.GetActual($"SELECT 1 as myCol FROM dbo.DataTypeTest WHERE {column} is not null;");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("id", "id=4", 4)]
        [TestCase("myCol", "myCol='x'", "x")]
        [TestCase("myCol", "myCol='x';myDate='1/1/1900'", "x")]
        [TestCase("id", "myCol='x';id=8", 8)]
        public void InsertRows_UseColumnOVerrides_ColumnIsGivenValue(string column, string columnOverrides, object expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(id int NOT NULL, myCol varchar(10), myDate date)");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest", columnOverrides: columnOverrides) ;
            var actual = target.GetActual($"SELECT {column} FROM dbo.DataTypeTest;");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertRows_Insert3Rows_RowCountIs3()
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(id int NOT NULL, myCol varchar(10))");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest", rows: 3);
            var actual = target.GetActual($"SELECT count(*) FROM dbo.DataTypeTest;");
            Assert.That(actual, Is.EqualTo(3));
        }

        [TestCase("VARCHAR(10)", "aaaaaaaaaa")]
        [TestCase("VARCHAR(5)", "aaaaa")]
        public void InsertRows_InsertWithRandomizeOn_StringsAreRandomized(string datatype, object expected)
        { 
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest", randomize: true);
            var actual = target.GetActual("SELECT myCol FROM dbo.DataTypeTest;");
            Assert.AreNotEqual(actual, expected);
        }

        [TestCase("INT", 0)]
        [TestCase("Numeric(10,2)", 0)]
        public void InsertRows_InsertWithRandomizeOn_NumbersAreRandomized(string datatype, object expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest", randomize: true);
            var actual = target.GetActual("SELECT myCol FROM dbo.DataTypeTest;");
            Assert.AreNotEqual(actual, expected);
        }

        [TestCase("uniqueidentifier", "EAB932A9-3B1A-460B-9FFD-BAC9F2B95D92")]
        public void InsertRows_InsertUniqueIdentifierWithRandomize_ReturnsNewId(string datatype, Guid expected)
        {
            Target target = new("TestDb");
            StringBuilder sql = new();
            sql.AppendLine("DROP TABLE IF EXISTS dbo.DataTypeTest;");
            sql.AppendLine($"CREATE TABLE dbo.DataTypeTest(myCol {datatype})");
            target.ExecuteSql(sql.ToString());
            target.InsertRows("DataTypeTest", randomize: true);
            var actual = target.GetActual("SELECT myCol FROM dbo.DataTypeTest;");
            Assert.AreNotEqual(actual, expected);
        }
    }
}
