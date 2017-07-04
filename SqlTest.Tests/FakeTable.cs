using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Transactions;

namespace SqlTest.Tests
{
    public class FakeTable
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
        public void FakeTable_ExecuteFakeTable_CreatesFakeTable()
        {
            SqlTest.FakeTable.CreateShell("TestDB", "Test");
            Assert.DoesNotThrow( 
                delegate { SqlTest.Sql.SetUp($"Select 1 From TestDb.dbo.Test_Faked;"); }
                );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void FakeTable_ExecuteFakeTable_IdentityHandledCorrectly(bool keepIdentity, bool expected)
        {
            SqlTest.FakeTable.CreateShell("TestDB", "HasIdentity", keepIdentity);
            var actual = SqlTest.Sql.GetActual(@"SELECT c.is_identity 
                                                FROM sys.all_columns c 
                                                    JOIN sys.objects o on c.object_id = o.object_id
                                                 WHERE o.name = 'HasIdentity'");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
