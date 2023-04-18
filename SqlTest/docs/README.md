## About
SqlTest is a C# library which provides scaffolding to write tests for SQL Server projects.

## Key Uses
 - Incorporate with NUNIT or other test frameworks
 - Write database unit tests / destructive with rollback
 - Mock Tables and Views
 - Test SSIS Packages
 - Test ADF (Azure Data Factory) Pipelines

## Getting Started

See the sample files in the Samples folder for the framework for executing tests within transactions and an example of how to call SqlTest.  
Be sure to update the App.config file to point to your local instance.  SSIS projects in the project deployment model must be deployed to the catalog.

## Example
The following example shows a SQL Test class. Note that the setup and teardown methods are used to create and rollback each test in a transaction. This is a function of the test harness (nunit) which will call the setup method before each test and the teardown after each test. Because tests are rolled back, they can be written in a destructive way in order to create a consistent setup - truncating or faking tables and views and inserting data.

All that said, **use with caution**.  This is best applied to a local database that can be rebuilt as needed.  Failures in test code can leave things in an undesired state.

```
using System;
using NUnit.Framework;
using SqlTest;
using System.Transactions;

    namespace Sample.Tests
    {
    public class CustomerLoad
    {
        TransactionScope scope;
	    SqlTestTarget dataWarehouseDb;
        SqlTestTarget staging;

    [SetUp]
    public void Setup()
    {
        scope = new TransactionScope();
        dataWarehouseDb = new SqlTestTarget("DataWarehouse");
        staging= new SqlTest.SqlTestTarget("Staging");
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
    public void CustomerLoad_DuplicateCustomer_FirstInstanceIsInserted()
    {
        //Arrange
        dataWarehouseDb.ExecuteAdhoc(@"Truncate Dimension.Customer;");
        staging.ExecuteAdhoc(@"Truncate staging.Customer;
                              Insert into Staging.Customer (Id, Name)
                                Values (1, 'Test Customer'), (2, 'Test Customer');"

        //Act
        staging.ExecuteAdhoc($"Exec Staging.CustomerLoad;");

        //Assert
        var actual = dataWarehouseDb.GetActual("Select top (1) CustomerId from Dimension.Customer Order by 1 desc;");
        Assert.That(actual, Is.EqualTo(1));

      } 
     }
    }
```

## Related Packages
[NUnit](https://www.nuget.org/packages/NUnit)
[NUnit3TestAdapter](https://www.nuget.org/packages/NUnit3TestAdapter/4.5.0-alpha.4)
[System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient)

## Documentation
[Wiki on github](https://github.com/MarkWojciechowicz/SqlTest/wiki)