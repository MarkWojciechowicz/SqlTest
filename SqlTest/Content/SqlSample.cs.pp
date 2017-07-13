using System;
using NUnit.Framework;
using SqlTest;
using System.Transactions;

namespace $rootnamespace$.Tests
{
    public class UnitUnderTest
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
        public void UnitUnderTest_Action_ExpectedOutcome()
        {
            //Arrange
            SqlTest.Sql.ExecuteAdhoc($"Truncate table / Insert test data;");

            //Act
            SqlTest.Sql.ExecuteAdhoc($"Exec proc;");

            //Assert
            var actual = SqlTest.Sql.GetActual("Select scalar value to test outcome");
            Assert.That(actual, Is.EqualTo("Expected Value"));

            //The teardown method runs after every test and will rollback all actions in one transaction
        }

 
        [TestCase("SomeValue", 1)]
        [TestCase("SomeOtherValue", 2)]
        public void UnitUnderTest_Action_ExpectedOutcome(string param, object expected)
        {
            //Arrange
            SqlTest.Sql.ExecuteAdhoc($"Truncate table / Insert test data;");

            //Act
            SqlTest.Sql.ExecuteAdhoc($"Exec proc @param = {param};");

            //Assert
            var actual = SqlTest.Sql.GetActual("Select to check result");
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
