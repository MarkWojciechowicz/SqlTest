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
        public void FakeTable_ExecuteCreateShell_CreatesFakeTable()
        {
            SqlTest.FakeTable.CreateShell("TestDB", "Test");
            Assert.DoesNotThrow(
                delegate { SqlTest.Sql.SetUp($"Select 1 From dbo.Test_Faked;"); }
                );
        }

        [Test]
        public void FakeTable_ExecuteCreateShell_FakeColumnsAreNullable()
        {
            SqlTest.FakeTable.CreateShell("TestDB", "Test");
            Assert.DoesNotThrow(
                delegate { SqlTest.Sql.SetUp($"Insert into dbo.Test (Description) Values('Test');"); }
                );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void FakeTable_ExecuteCreateShell_IdentityHandledCorrectly(bool keepIdentity, bool expected)
        {
            SqlTest.FakeTable.CreateShell("TestDB", "HasIdentity", keepIdentity);
            var actual = SqlTest.Sql.GetActual(@"SELECT c.is_identity 
                                                FROM sys.all_columns c 
                                                    JOIN sys.objects o on c.object_id = o.object_id
                                                 WHERE o.name = 'HasIdentity'");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FakeTable_ExecuteCreateShell_DefaultConstraintIsKept()
        {
            SqlTest.FakeTable.CreateShell("TestDB", "HasDefault");
            var actual = SqlTest.Sql.GetActual(@"Declare @description as table (Description varchar(50));
                                                Insert into dbo.HasDefault (ID)
	                                                OUTPUT inserted.Description into @description (Description)
	                                                Values(1) ;
                                                SELECT Description from @Description;");
            Assert.That(actual, Is.EqualTo("Test"));
        }
     }
}
