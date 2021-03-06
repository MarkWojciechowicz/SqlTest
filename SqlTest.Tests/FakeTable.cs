﻿using System;
using System.Transactions;
using NUnit.Framework;

namespace SqlTest.Tests
{
    public class FakeTable
    {
        TransactionScope scope;
        SqlTest.SqlTestTarget testTarget;
        SqlTest.SqlTestTarget sqlUser;

        [SetUp]
        public void Setup()
        {
            scope = new TransactionScope();
            testTarget = new SqlTest.SqlTestTarget("testTarget");
            sqlUser = new SqlTest.SqlTestTarget("sqlUser");
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

        [Test]
        public void FakeTable_ExecuteCreateShellWithSqlAccount_OledbConvertedToSqlConnAndNoErrorThrown()
        {
           
            Assert.DoesNotThrow(
                delegate { sqlUser.CreateFakeTableShell("Test"); }
                );
        }

        [TestCase("test","test_faked")]
        [TestCase("sales.Customer", "customer_Faked")]
        public void FakeTable_TableIsAlreadyFaked_FakeIsDroppedAndTableRenamed(string table, string fakedName)
        {
            testTarget.CreateFakeTableShell(table);
            testTarget.CreateFakeTableShell(table);

            var actual = testTarget.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = '{fakedName}'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [TestCase("test")]
        [TestCase("test_Faked")]
        public void FakeTable_TableIsAlreadyFakedButFakeDoesNotExist_TableRenamedFakeCreated(String tableExists)
        {
            testTarget.CreateFakeTableShell("Test");
            testTarget.ExecuteAdhoc("Drop table Test;");
            testTarget.CreateFakeTableShell("Test");

            var actual = testTarget.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = '{tableExists}'");
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void FakeTable_DropTableCalledTwice_SourceTableRetained()
        {
            testTarget.CreateFakeTableShell("Test");
            testTarget.DropFakeTable("Test");
            testTarget.DropFakeTable("Test");

            var actual = testTarget.GetActual($"SELECT 1 FROM Information_Schema.Tables Where Table_Name = 'Test'");
            Assert.That(actual, Is.EqualTo(1));
        }

    }
}
