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
        SqlTest.SqlTestTarget testTarget;

        [SetUp]
        public void Setup()
        {
            scope = new TransactionScope();
            testTarget = new SqlTest.SqlTestTarget("testTarget");
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
            testTarget.CreateFakeView("MyTests");
            Assert.DoesNotThrow(
                delegate { testTarget.ExecuteAdhoc($"Select 1 From dbo.MyTests_Faked;"); }
                );
        }

        [Test]
        public void FakeView_ExecuteDrop_DropsFakeView()
        {
            testTarget.CreateFakeView("MyTests");
            testTarget.DropFakeView("MyTests");
            Assert.Throws(typeof(Exception),
                delegate { testTarget.ExecuteAdhoc($"Select 1 From dbo.MyTests_Faked;"); }
                );
        }

    }
}
