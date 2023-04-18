## About
SqlTest is a C# library which provides scaffolding to write tests for SQL Database projects.

## Key Uses
 - Incorporate with NUNIT or other test frameworks
 - Write database unit tests / destructive with rollback
 - Mock Tables, Views and functions
 - Generate test data
 - Setup and teardown for integration testing

## Getting Started

Create a .net core class library. Install SqlTest nuget package: (https://www.nuget.org/packages/SqlTest/).  Optionally, add the following for using the nunit test harness:

 - [NUnit](https://www.nuget.org/packages/NUnit)
 - [NUnit3TestAdapter](https://www.nuget.org/packages/NUnit3TestAdapter/4.5.0-alpha.4)
 - [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

Add an appsettings.json file and set the `Copy to output directory` property to `Copy always`.  The file should be in the following form to set connection properties for the target database:
```
{
  "Targets": {
    "TestDb": {
      "ServerName": "localhost",
      "Databasename": "TestDb",
      "IntegratedSecurity": true,
      "TrustServerCertificate": true
    },
    "SqlLogin": {
      "ServerName": "localhost",
      "Databasename": "TestDb",
      "User": "SqlUser",
      "Password": "<some password>",
      "TrustServerCertificate": true
    }
  }
}
```

## Example
The following example shows a SQL Test class. Note that the setup and teardown methods are used to create and rollback each test in a transaction. This is a function of the test harness (nunit) which will call the setup method before each test and the teardown after each test. Because tests are rolled back, they can be written in a destructive way in order to create a consistent setup - truncating or faking tables and views and inserting data.

All that said, **use with caution**.  This is best applied to a local database that can be rebuilt as needed.  Failures in test code can leave things in an undesired state.

```
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
        public void LoadCustomer_CustomerExists_NameIsUpdated()
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
```

## Documentation
[Wiki on github](https://github.com/MarkWojciechowicz/SqlTest/wiki)