using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Transactions;
using Microsoft.SqlServer.Management.Smo;

namespace SqlTest.Tests
{
    public class SqlTestTarget
    {
        private TransactionScope _scope;
        private SqlTest.SqlTestTarget _testTarget;

        [SetUp]
        public void Setup()
        {
            _scope = new TransactionScope();
            _testTarget = new SqlTest.SqlTestTarget("testTarget");
        }

        [TearDown]
        public void Teardown()
        {
            if (Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
            {
                _scope.Dispose();
            }
        }

        [Test]
        public void SqlTestTarget_ExecuteSetup_NoExceptionThrown()
        {
            _testTarget.ExecuteAdhoc($"Select 1;");
        }

        [Test]
        public void SqlTestTarget_ExecuteSetup_ExceptionThrown()
        {
            Assert.Throws(typeof(Exception),
                delegate { _testTarget.ExecuteAdhoc($"Select 1/0;"); }
            );
        }

        [TestCase("Select 1 as MyInt", 1)]
        [TestCase("Select '1' as MyString", "1")]
        public void SqlTestTarget_ExecuteGetValue_ReturnsValue(string sql, object expected)
        {
            var actual = _testTarget.GetActual(sql);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SqlTestTarget_GetServerFromOledbConnString_ReturnsServer()
        {
            SqlTest.SqlTestTarget testTarget = new SqlTest.SqlTestTarget("testTarget");
            var actual = testTarget.GetServer();
            Assert.That(actual, Is.Not.Null);
        }

        //[Test]
        //public void SqlTestTarget_InitialInstance_

        
    }
}
