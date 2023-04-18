using NUnit.Framework;
using System.Runtime.Versioning;
using System.Transactions;

namespace SqlTest.Tests
{
    [TestFixture]
    public class FakeView
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
                if (scope != null)
                {
                    scope.Dispose();
                }
            }
        }

        [Test]
        public void FakeView_CreateFakeView_ViewRenamed()
        {
            Target target = new("TestDb");
            target.CreateFakeView("MyTests");
            var actual = target.GetActual($"SELECT COUNT(*) FROM sys.views WHERE name = 'MyTests_faked'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeView_CreateFakeView_TableIsCreated()
        {
            Target target = new("TestDb");
            target.CreateFakeView("MyTests");
            var actual = target.GetActual($"SELECT COUNT(*) FROM sys.tables WHERE name = 'MyTests'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeView_CreateFakeViewAlreadyExists_DoesNotThrowError()
        {
            Target target = new("TestDb");
            Assert.DoesNotThrow(
                delegate
                {
                    target.CreateFakeView("MyTests");
                    target.CreateFakeView("MyTests");
                });
        }

        [Test]
        public void FakeView_CreateFakeViewInsertRow_RowIsInserted()
        {
            Target target = new("TestDb");
            target.CreateFakeView("MyTests");
            target.ExecuteSql("INSERT INTO dbo.MyTests(Description) VALUES('test')");
            var actual = target.GetActual($"SELECT Description FROM MyTests");
            Assert.That(actual, Is.EqualTo("test"));
        }

        [Test]
        public void FakeView_CreateFakeViewOverrideDatabase_ViewIsFaked()
        {
            string databaseOverride = "TestDb";
            Target target = new("ConfigWithoutDatabase");
            target.CreateFakeView("MyTests", databaseOverride);
            var actual = target.GetActual($"SELECT COUNT(*) FROM sys.tables WHERE name = 'MyTests'", databaseOverride);
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeView_DropFakeView_ViewIsRestored()
        {
            Target target = new("TestDb");
            target.CreateFakeView("MyTests");
            target.DropFakeView("MyTests");
            var actual = target.GetActual($"SELECT COUNT(*) FROM sys.views WHERE name = 'MyTests'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeView_DropFakeViewTwice_DoesNotThrowError()
        {
            Target target = new("TestDb");
            target.CreateFakeView("MyTests");
            Assert.DoesNotThrow(
                delegate
                {
                    target.DropFakeView("MyTests");
                    target.DropFakeView("MyTests");
                });
        }
    }
}
