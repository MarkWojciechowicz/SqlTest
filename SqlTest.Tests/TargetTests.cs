using NUnit.Framework;
using System.Transactions;

namespace SqlTest.Tests
{
    [TestFixture]
    public class TargetTests
    {
        [Test]
        public void Target_ExecuteSql_CanConnect()
        {
            var target = new Target("TestDb");
            Assert.DoesNotThrow(
                delegate { target.ExecuteSql("select 1"); }
                );
        }

        [Test]
        public void Target_ExecuteSqlWithSqlLogin_DoesNotThrowError()
        {
            var target = new Target("SqlLogin");
            Assert.DoesNotThrow(
                delegate { target.ExecuteSql("select 1"); }
                );
        }

        [Test]
        public void Target_GetActual_DoesNotThrowError()
        {
            var target = new Target("TestDb");
            Assert.DoesNotThrow(
                delegate { target.GetActual("select 1"); }
                );
        }

        [TestCase("TestDb", "TestDb")]
        [TestCase("master", "master")]
        public void Target_GetActualWithDifferentConfigs_ReturnsServer(string setting, string expected)
        {
            var target = new Target(setting);
            var actual = target.GetActual("SELECT DB_NAME()");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Target_GetActualDivideByZero_SqlReturnsError()
        {
            var target = new Target("TestDb");
            var ex = Assert.Throws<Exception>(
                delegate { target.GetActual("SELECT 1/0"); },
                "GetActual failed with: '{e.Message}', executing the statement: '{sql}'"
                );
            Assert.That(ex.Message, Is.EqualTo("GetActual failed with: 'Divide by zero error encountered.', executing the statement: 'SELECT 1/0'"));
        }

        [TestCase("SELECT 1", 1)]
        [TestCase("SELECT 'test'", "test")]
        [TestCase("SELECT CONVERT(bit, 0)", false)]
        public void Target_GetActualWithDifferentReturnTypes_TypesMatchExpected(string sql, object expected)
        {
            var target = new Target("TestDb");
            var actual = target.GetActual(sql);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Target_CreateTableWithRollback_TableDoesNotExist()
        {
            TransactionScope _scope = new TransactionScope();
            var target = new Target("TestDb");
            target.ExecuteSql("Create table [t](t int)");
            if (Transaction.Current != null && Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
            {
                _scope.Dispose();
            }
            var actual = target.GetActual("SELECT COUNT(*) FROM sys.tables t WHERE name = 't'");
            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void Target_OverrideDatabase_msdbIsRetuned()
        {
            var target = new Target("ConfigWithoutDatabase");
            var actual = target.GetActual("SELECT DB_NAME()", "msdb");
            Assert.That(actual, Is.EqualTo("msdb"));
        }
    }
}