using NUnit.Framework;
using System.Runtime.Versioning;
using System.Transactions;

namespace SqlTest.Tests
{
    [TestFixture]
    public class FakeTable
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

        [TestCase("Test", "Test_faked")]
        [TestCase("[Test]", "[Test_faked]")]
        [TestCase("dbo.Test", "dbo.Test_faked")]
        [TestCase("sales.Customer", "sales.Customer_faked")]
        [TestCase("[sales].[Customer]", "[sales].[Customer_faked]")]
        [TestCase("[dbo].[Table With Spaces]", "[dbo].[Table With Spaces_faked]")]
        public void FakeTable_CreateFakeTable_FakeTableExists(string tableName, string fakeTableName)
        {
            Target target = new("TestDb");
            target.CreateFakeTable(tableName);
            Assert.DoesNotThrow(
                delegate { target.ExecuteSql($"Select 1 From {fakeTableName};"); }
                );
        }

        [Test]
        public void FakeTable_CreateFakeTable_FakeColumnsAreNullable()
        {
            Target target = new("TestDb");
            target.CreateFakeTable("Test");
            Assert.DoesNotThrow(
                delegate { target.ExecuteSql($"Insert into dbo.Test (Description) Values('Test');"); }
                );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void FakeTable_CreateFakeTable_IdentityHandledCorrectly(bool keepIdentity, bool expected)
        {
            Target target = new("TestDb");
            target.CreateFakeTable("HasIdentity", keepIdentity: keepIdentity);
            var actual = target.GetActual(@"SELECT c.is_identity 
                                                FROM sys.all_columns c 
                                                    JOIN sys.objects o on c.object_id = o.object_id
                                                 WHERE o.name = 'HasIdentity'");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FakeTable_CreateFakeTable_DefaultConstraintIsKept()
        {
            Target target = new("TestDb");
            target.CreateFakeTable("HasDefault");
            var actual = target.GetActual(@"Declare @description as table (Description varchar(50));
                                                Insert into dbo.HasDefault (ID)
	                                                OUTPUT inserted.Description into @description (Description)
	                                                Values(1) ;
                                                SELECT Description from @Description;");
            Assert.That(actual, Is.EqualTo("Test"));
        }

        [Test]
        public void FakeTable_ExecuteTableDrop_SourceTableIsRenamed()
        {
            Target target = new("TestDb");
            target.CreateFakeTable("Test");
            target.DropFakeTable("Test");
            Assert.Throws(typeof(Exception), delegate { target.ExecuteSql("SELECT 1 FROM Test_Faked;"); });
        }

        [Test]
        public void FakeTable_ExecuteTableDrop_ReturnsActualResult()
        {
            Target target = new("TestDb");
            target.CreateFakeTable("Test");
            target.ExecuteSql("Insert into Test (Id) Values (1);");
            var actual = target.GetActual("Select Id From Test");
            target.DropFakeTable("Test");
            Assert.That(actual, Is.EqualTo(1));

        }

        [TestCase("test", "test_faked")]
        [TestCase("sales.Customer", "customer_Faked")]
        public void FakeTable_TableIsAlreadyFaked_FakeIsDroppedAndTableRenamed(string table, string fakedName)
        {
            Target target = new("TestDb");
            target.CreateFakeTable(table);
            target.CreateFakeTable(table);

            var actual = target.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = '{fakedName}'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("test")]
        [TestCase("test_Faked")]
        public void FakeTable_TableIsAlreadyFakedButFakeDoesNotExist_TableRenamedFakeCreated(String tableExists)
        {
            Target target = new("TestDb");
            target.CreateFakeTable("Test");
            target.ExecuteSql("Drop table Test;");
            target.CreateFakeTable("Test");

            var actual = target.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = '{tableExists}'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeTable_DropTableCalledTwice_SourceTableRetained()
        {
            Target target = new("TestDb");
            target.CreateFakeTable("Test");
            target.DropFakeTable("Test");
            target.DropFakeTable("Test");

            var actual = target.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = 'Test'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeTable_FullTestExample_RowIsUpdated()
        {
            //arrange
            Target target = new("TestDb");
            target.CreateFakeTable("Customer");
            target.ExecuteSql("Truncate Table Stage.Customer");
            target.ExecuteSql("INSERT INTO dbo.Customer (Id, CustomerCode, Name) values (1, 'Cust123', 'Old Name')");
            target.ExecuteSql("INSERT INTO Stage.Customer (CustomerCode, Name) values ('Cust123', 'New Name')");

            //act
            target.ExecuteSql("Exec Stage.Load_Customer");

            //assert
            var actual = target.GetActual($"SELECT Name FROM dbo.Customer");
            Assert.That(actual, Is.EqualTo("New Name"));
        }
    }
}
