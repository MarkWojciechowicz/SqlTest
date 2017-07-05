using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Transactions;

namespace SqlTest.Tests
{
    public class FakeView
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
        public void FakeView_ExecuteCreate_CreatesFakeView()
        {
            SqlTest.FakeView.Create("TestDB", "MyTests");
            Assert.DoesNotThrow(
                delegate { SqlTest.Sql.ExecuteAdhoc($"Select 1 From dbo.MyTests_Faked;"); }
                );
        }

        [Test]
        public void FakeView_ExecuteDrop_DropsFakeView()
        {
            SqlTest.FakeView.Create("TestDB", "MyTests");
            SqlTest.FakeView.Drop("TestDB", "MyTests");
            Assert.Throws(typeof(Exception),
                delegate { SqlTest.Sql.ExecuteAdhoc($"Select 1 From dbo.MyTests_Faked;"); }
                );
        }

    }
}
