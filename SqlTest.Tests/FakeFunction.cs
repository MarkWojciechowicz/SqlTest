using System.Runtime.Versioning;
using System.Transactions;
using NUnit.Framework;

namespace SqlTest.Tests
{
    [TestFixture]
    public class FakeFunction
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
                scope?.Dispose();
            }
        }

        [Test]
        public void FakeFunction_ExecuteFake_ReturnsNewValue()
        {
            Target target = new("TestDb");
            string newTextBody = "BEGIN\r\n\tDECLARE @lastLoadDate DATETIME;\r\n\tSELECT @lastLoadDate = '1/1/2020';\r\n\tRETURN @lastLoadDate;\r\nEND";
            target.CreateFakeFunction("dbo.GetLastLoadDate", newTextBody);
            var actual = target.GetActual($"Select dbo.GetLastLoadDate();");
            DateTime expected = new DateTime(2020, 1, 1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FakeFunction_ExecuteFakeWithParameter_ReturnsNewValue()
        {
            Target target = new("TestDb");
            string newTextBody = "BEGIN\r\nRETURN 2\r\nEND";
            target.CreateFakeFunction("dbo.FunctionWithParameter", newTextBody);
            var actual = target.GetActual($"Select dbo.FunctionWithParameter(1);");
            Assert.That(actual, Is.EqualTo(2));
        }

        [Test]
        public void FakeFunction_ExecuteFakeWithTVF_ReturnsNewValue()
        {
            Target target = new("TestDb");
            string newTextBody = "BEGIN\r\n    INSERT @returntable\r\n\tSELECT 123\r\n\tRETURN\r\nEND";
            target.CreateFakeFunction("dbo.TVF", newTextBody);
            var actual = target.GetActual($"Select * FROM dbo.TVF(1);");
            Assert.That(actual, Is.EqualTo(123));
        }
    }
}


