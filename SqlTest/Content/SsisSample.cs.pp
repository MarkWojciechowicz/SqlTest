using System;
using NUnit.Framework;
using SqlTest;

namespace $rootnamespace$.Tests
{
    public class UnitUnderTest
    {
		SqlTestTarget dataWarehouseDb;
		SsisTestTarget ssis;

        [SetUp]
        public void Setup()
        {
			dataWarehouseDb = new SqlTestTarget("DataWarehouse");
			ssis = new SsisTestTarget(
                    ssisServerAppSetting: "ssisServer"
                    , packageFolder: "ETL"
                    , projectName: "DataWarehouse"
                    , useEnvironment: true
                    , environmentFolder: "ETL"
                    , environmentName: "Datawarehouse");
        }

        [TestCase("Extract")]
        [TestCase("Transform")]
        [TestCase("Load")]
        public void UnitUnderTest_RunPackage_NoErrorsAreThrown(string packageName)
        {
            Assert.DoesNotThrow(delegate { ssisServer.ExecutePackage(packageName); });
        }

		public void UnitUnderTest_TransformCustomer_NameIsConformed()
        {
            try
            {
                //Arrange
				dataWarehouseDb.CreateFakeTableShell("Dimension.Customer")
                dataWarehouseDb.ExecuteAdhoc($@"Truncate table stage.Customer;
                        Insert into stage.customer(id, name) values(1, 'MY CUSTOMER')';");

                //act
                ssisServer.ExecutePackage("Transform_Customer.dtsx");

                //Assert
                var result = dataWarehouseDb.DropFakeTable("Dimension.Customer", "SELECT Customer_Name FROM Dimension.Customer;");
                Assert.That(result, Is.EqualTo("My Customer"));
            }

            catch(Exception e)
            {
                dataWarehouseDb.DropFakeTable("Dimension.Customer");
                throw e;
            }

        }

    }
}
