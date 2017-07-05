using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Transactions;

namespace SqlTest.Tests
{
    public class Sql
    {
        TransactionScope scope;

        [SetUp]
        public void Setup()
        {
            scope = new TransactionScope();
        }

        [TearDown]
        public void Teardown()
        {
            if (Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
            {
                scope.Dispose();
            }
        }

        [Test]
        public void Sql_ExecuteSetup_NoExceptionThrown()
        {
            SqlTest.Sql.ExecuteAdhoc($"Select 1;");
        }

        [Test]
        public void Sql_ExecuteSetup_ExceptionThrown()
        {
            Assert.Throws(typeof(Exception),
                delegate { SqlTest.Sql.ExecuteAdhoc($"Select 1/0;"); }
            );
        }

        [TestCase("Select 1 as MyInt", 1)]
        [TestCase("Select '1' as MyString", "1")]
        public void Sql_ExecuteGetValue_ReturnsValue(string sql, object expected)
        {
            var actual = SqlTest.Sql.GetActual(sql);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
