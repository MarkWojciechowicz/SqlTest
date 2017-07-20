# SqlTest
SqlTest provides the scaffolding to write tests for SQL Server and SSIS in C# using nunit or another test harness.  

## Classes
SqlTestTarget  

## Examples



Represents an instance of a sql database to connect to.  
**Constructor**   

    SqlTestTarget(string connectionStringConfigName)

 - _connectionStringConfigName_: a string which identifies the name of a connectionsting in the app.config file.  
 For example, "staging" would be used to identify the connectionstring below:

    \<connectionStrings>    
		\<add name="Staging" connectionString="Data Source=.;Initial Catalog=Staging;Provider=sqloledb;Integrated Security=SSPI;"/>    
	\</connectionStrings>    

**Exceptions**
 - _NullReferenceException_:  thrown in the configuration is not found.

**Methods**  

    ExecuteAdhoc(string sql)

Used to execute any sql statement for the setup, teardown or test action.
 - _sql_: is any string of SQL to be executed on the target database.  The

**Exceptions**
 - _Exception_:  All exceptions are rethrown giving the context of the method that was called and the sql statement.  Note that an error in SQL syntax will cause a test to fail without any additional special handling.  

**Example** 