# SqlTest

This project provides a framework to aid in unit testing SQL Server and SSIS Packages.  It is intended to be used with Nunit with tests written in C#.  SqlTest takes care of all the boiler plate code to execute sql and fetch results.  It also offers a mocking framework for faking tables, views and functions inside or outside of a transaction.  Lastly, it provides the ability to execute packages, so ETL code can be tested end to end.

### Installing

SqlTest can be installed with Nuget by using the following command in the Package Manager Console:

```
Install-Package SqlTest
```
This will also install nunit if it's not already there.

## Getting Started

See the sample files in the Samples folder for the framework for executing tests within transactions and an example of how to call SqlTest.  
Be sure to update the App.config file to point to your local instance.  SSIS projects in the project deployment model must be deployed to the catalog.

### Prerequisites

 - Sql Server 2016 (Developer is a free edition)
 - Integration Services (install with Sql Server)
 - Visual Studio 2015 and up (Community edition is free)
 - SSDT - Installs dlls which SqlTest depends on

## Running the tests

The nunit test adapter is also installed with the download of SqlTest from nuget.  This allows for tests can be run from the Test Explorer window.  Once tests are deployed to a build server, they can be run using https://github.com/nunit/docs/wiki/NUnitLite-Runner from the commmand line.  I recommend excuting this tests from the build the server.  Distributed transactions will need to be enabled on both the target and build server and set to allow remote transactions.

## Authors

Mark Wojciechowicz

## License

This project is licensed under the GNU License - see the [LICENSE](LICENSE) file for details


