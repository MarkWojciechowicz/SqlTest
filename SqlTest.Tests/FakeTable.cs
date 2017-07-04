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
        public void FakeTable_ExecuteFake_CreatesFakeTable()
        {
            SqlTest.FakeTable.CreateShell("TestDB", "Test");
            Assert.DoesNotThrow( 
                delegate { SqlTest.Sql.SetUp($"Select 1 From TestDb.dbo.Test_Faked;"); }
                );
        }

        
    }
}
