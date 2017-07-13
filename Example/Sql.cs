using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlTest;
using System.Transactions;

namespace Example
{
    [TestFixture]
    public class Sql
    {
        TransactionScope scope;
        SqlTestTarget dbUnderTest;

        [SetUp]
        public void Setup()
        {
            scope = new TransactionScope();
            dbUnderTest = new SqlTestTarget("testTarget");
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
        public void Sql_ExecuteSetup_TableCreated()
        {
            //Create table
            dbUnderTest.ExecuteAdhoc($"Create table test(Id Int);");

            var result = dbUnderTest.GetActual("Select count(*) from Information_Schema.Tables WHERE Table_Name = 'Test'");

            Assert.That(result, Is.EqualTo(1));
        }
    }
}
