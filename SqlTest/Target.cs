using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace SqlTest
{
    /// <summary>
    /// Database target used for connecting to the database to execute adhoc sql, get actual results and create fakes
    /// </summary>
    public class Target
    {
        private Setting Setting { get; set;}

        /// <summary>
        /// Creates a target database object bases on configuration in appsettings.json
        /// </summary>
        /// <param name="targetName">Name of the target in appsettings.json</param>
        public Target(string targetName) {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var setting = config.GetRequiredSection($"Targets:{targetName}").Get<Setting>();
            if(setting == null)
            {
                throw new Exception($"Setting not found: '{targetName}'");
            }
            else
            {
                Setting = setting;
            }
        }

        private Server GetServer(string? databaseName)
        {
            ValidateSetting(Setting);
            var connString = new SqlConnectionStringBuilder
            {
                DataSource = Setting.ServerName,
                InitialCatalog = databaseName,
                IntegratedSecurity = Setting.IntegratedSecurity,
                TrustServerCertificate = true
            };
            if (!Setting.IntegratedSecurity)
            {
                connString.UserID = Setting.User;
                connString.Password = Setting.Password;
            }
            var sqlConn = new SqlConnection(connString.ConnectionString);
            var serverConn = new ServerConnection(sqlConn);
            return new Server(serverConn);
        }

        private string GetDatabase(string? databaseName)
        {
            string _database;
            if (databaseName != null)
            {
                _database = databaseName;
            }
            else if (Setting.DatabaseName == null)
            {
                throw new ArgumentNullException(nameof(Setting.DatabaseName));
            }
            else 
            { 
                _database = Setting.DatabaseName; 
            }
            return _database;
        }

        private static void ValidateSetting(Setting? setting)
        {
            if (setting == null)
            {
                throw new Exception("Setting is null. Is there an appsettings.json file?  Does the target exist?");
            }
        }

        /// <summary>
        /// Executes adhoc sql
        /// </summary>
        /// <param name="sql">Statement to be executed</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <exception cref="Exception"></exception>
        public void ExecuteSql(string sql, string? databaseName = null)
        {
            try
            {
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                server.Databases[_db].ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                throw new Exception($"ExecuteSql failed with: '{e.GetBaseException().Message}', executing the statement: '{sql}'");
            }
        }

        /// <summary>
        /// Gets scalar value from the target database.
        /// </summary>
        /// <param name="sql">The statement to be executed</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <returns>object</returns>
        /// <exception cref="Exception">Returns base error message from exception</exception>
        public object GetActual(string sql, string? databaseName = null)
        {
            try
            {
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                return server.Databases[_db].ExecuteWithResults(sql).Tables[0].Rows[0][0];
            }
            catch (Exception e)
            {
                throw new Exception($"GetActual failed with: '{e.GetBaseException().Message}', executing the statement: '{sql}'");
            }
        }

        /// <summary>
        /// Renames the existing table as 'myTable_faked' and creates a copy based on the same schema without indexes.
        /// All columns are nullable.
        /// </summary>
        /// <param name="table">Name of the table to be faked.  Dbo will be used if the schema is not specified.</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <param name="keepIdentity">True if the identity attribute should be added to the fake</param>
        public void CreateFakeTable(string table, string? databaseName = null, bool keepIdentity = false)
        {
            try
            {
                var sqlObject = new SqlObject(table);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                FakeTable fakeTable = new FakeTable(server, _db, sqlObject.Name, sqlObject.Schema, keepIdentity);
                fakeTable.Create();
            }
            catch (Exception e)
            {
                throw new Exception($"CreateFakeTable failed with: '{e.GetBaseException().Message}', for '{table}'");
            }
        }

        /// <summary>
        /// Drops a fake table and restores the name of the original table
        /// </summary>
        /// <param name="table">The original table name</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <exception cref="Exception">Returns base exception</exception>
        public void DropFakeTable(string table, string? databaseName = null)
        {
            try
            {
                var sqlObject = new SqlObject(table);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                FakeTable fakeTable = new FakeTable(server, _db, sqlObject.Name, sqlObject.Schema);
                fakeTable.Drop();
            }
            catch (Exception e)
            {
                throw new Exception($"DropFakeTable failed with: '{e.GetBaseException().Message}', for '{table}'");
            }
        }

        /// <summary>
        /// Creates a table based on the schema of the view to be faked.  All columns are nullable.
        /// </summary>
        /// <param name="view">Name of view to be faked. Dbo will be used if the schema is not specified</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <exception cref="Exception"></exception>
        public void CreateFakeView(string view, string? databaseName = null)
        {
            try
            {
                var so = new SqlObject(view);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                FakeView fakeView = new FakeView(server, _db, so.Name, so.Schema);
                fakeView.Create();
            }
            catch (Exception e)
            {
                throw new Exception($"CreateFakeView failed with: '{e.GetBaseException().Message}', for '{view}'");
            }
        }

        /// <summary>
        /// Drops fake and renames view back to original name.
        /// </summary>
        /// <param name="view">Table name, if schema is not specified, 'dbo' is used</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <exception cref="Exception"></exception>
        public void DropFakeView(string view, string? databaseName = null)
        {
            try
            {
                var so = new SqlObject(view);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                FakeView fakeView = new FakeView(server, _db, so.Name, so.Schema);
                fakeView.Drop();
            }
            catch (Exception e)
            {
                throw new Exception($"DropFakeView failed with: '{e.GetBaseException().Message}', for '{view}'");
            }
        }

        /// <summary>
        /// Inserts the specified number of rows into a table
        /// </summary>
        /// <param name="table">Table name, if schema is not specified, 'dbo' is used</param>
        /// <param name="databaseName">Optional: Override target database</param>
        /// <param name="rows">Number of rows to insert, default is 1</param>
        /// <param name="columnOverrides">Set values for specific columns with a delimited list, i.e. MyStringCol='this string';MyIntCol=0</param>
        /// <param name="showStatement">When true the insert statement to be executed will print in the test log.</param>
        /// <param name="randomize">When true, returns random values for string, numbers and guids</param>
        /// <exception cref="Exception"></exception>
        public void InsertRows(string table, 
                               string? databaseName = null, 
                               int rows = 1, 
                               string? columnOverrides = null, 
                               bool showStatement = false,
                               bool randomize = false)
        {
            string sql = "";
            try
            {
                var sqlObject = new SqlObject(table);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                Table _table = server.Databases[_db].Tables[sqlObject.Name, sqlObject.Schema];
                DataGenerator gen = new(_table, columnOverrides, randomize);
                for(int i = 0; i < rows; i++)
                {
                    sql = gen.GenerateInsert();
                    if (showStatement) { TestContext.WriteLine(sql); }
                    server.Databases[_db].ExecuteNonQuery(sql);
                }
            }
            catch (Exception e)
            {
                TestContext.WriteLine($"Attempted to execute: \"{sql}\"");
                throw new Exception($"InsertRows failed with: '{e.GetBaseException().Message}', for '{table}'");
            }
        }

        public void CreateFakeFunction(string function, string newFunctionTextBody, string? databaseName = null)
        {
            try
            {
                var so = new SqlObject(function);
                var _db = GetDatabase(databaseName);
                Server server = GetServer(_db);
                FakeFunction fakeFunction = new(server, so.Name, so.Schema, _db, newFunctionTextBody);
                fakeFunction.Create();
            }
            catch (Exception e)
            {
                throw new Exception($"CreateFakeFunction failed with: '{e.GetBaseException().Message}', for '{function}'");
            }
        }
    }
}