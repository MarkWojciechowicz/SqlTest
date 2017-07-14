using System;
using System.Transactions;
using NUnit.Framework;

namespace SqlTest.Tests
{
    public class FakeTable
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

        [TestCase("Test", "Test_Faked")]
        [TestCase("[Test]", "[Test_Faked]")]
        [TestCase("dbo.Test", "dbo.Test_Faked")] 
        [TestCase("sales.Customer", "sales.Customer_Faked")]
        [TestCase("[sales].[Customer]", "[sales].[Customer_Faked]")]
        [TestCase("[dbo].[Table With Spaces]", "[dbo].[Table With Spaces]")]
        public void FakeTable_ExecuteCreateShell_CreatesFakeTable(string tableName, string fakeTableName)
        {
            testTarget.CreateFakeTableShell(tableName);
            Assert.DoesNotThrow(
                delegate { testTarget.ExecuteAdhoc($"Select 1 From {fakeTableName};"); }
                );
        }

        [Test]
        public void FakeTable_ExecuteCreateShell_FakeColumnsAreNullable()
        {
            testTarget.CreateFakeTableShell("Test");
            Assert.DoesNotThrow(
                delegate { testTarget.ExecuteAdhoc($"Insert into dbo.Test (Description) Values('Test');"); }
                );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void FakeTable_ExecuteCreateShell_IdentityHandledCorrectly(bool keepIdentity, bool expected)
        {
            testTarget.CreateFakeTableShell("HasIdentity", keepIdentity);    
            var actual = testTarget.GetActual(@"SELECT c.is_identity 
                                                FROM sys.all_columns c 
                                                    JOIN sys.objects o on c.object_id = o.object_id
                                                 WHERE o.name = 'HasIdentity'");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FakeTable_ExecuteCreateShell_DefaultConstraintIsKept()
        {
            testTarget.CreateFakeTableShell("HasDefault");
            var actual = testTarget.GetActual(@"Declare @description as table (Description varchar(50));
                                                Insert into dbo.HasDefault (ID)
	                                                OUTPUT inserted.Description into @description (Description)
	                                                Values(1) ;
                                                SELECT Description from @Description;");
            Assert.That(actual, Is.EqualTo("Test"));
        }

        [Test]
        public void FakeTable_ExecuteTableDrop_SourceTableIsRenamed()
        {
            testTarget.CreateFakeTableShell("Test");
            testTarget.DropFakeTable("Test");
            Assert.Throws(typeof(Exception), delegate { testTarget.GetActual("SELECT 1 FROM Test_Faked;"); });
        }

        [Test]
        public void FakeTable_ExecuteTableDrop_ReturnsActualResult()
        {
            testTarget.CreateFakeTableShell("Test");
            testTarget.ExecuteAdhoc("Insert into Test (Id) Values (1);");
            var actual = testTarget.DropFakeTable("Test", "Select Id From Test");
            Assert.That(actual, Is.EqualTo(1));

        }
    }
}
