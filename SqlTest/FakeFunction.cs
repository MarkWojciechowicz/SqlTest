using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace SqlTest
{
    internal class FakeFunction
    {
        private string Name { get; set; }
        private string Schema { get; set; }
        private string TextBody { get; set; }
        private bool Exists { get; set; }
        private Database TargetDb { get; set; }
        internal FakeFunction(Server server, string name, string schema, string targetDb, string textBody)
        {
            Name = name;
            Schema = schema;
            TextBody = textBody;    
            TargetDb = server.Databases[targetDb];
            Exists = TargetDb.UserDefinedFunctions[$"{name}_faked", schema] != null;
        }

        internal void Create()
        {
            if (Exists)
            {
                TestContext.WriteLine($"The function '{Schema}.{Name}' was faked.  Undoing fake...");
                this.Drop();
            }

            UserDefinedFunction functionToFake = TargetDb.UserDefinedFunctions[Name, Schema] ?? throw new Exception($"CreateFakeFunction failed, function does not exist: '{Schema}.{Name}'");
            UserDefinedFunction fake = new(TargetDb, Name, Schema);
            fake.TextHeader = functionToFake.TextHeader;
            fake.TextBody = TextBody;
            fake.Name = Name;
            fake.Schema = Schema;
            if(functionToFake.DataType != null)
                {  fake.DataType = functionToFake.DataType;}
            functionToFake.Rename($"{Name}_faked");
            fake.Create();
        }

        internal void Drop()
        {
            if (TargetDb.UserDefinedFunctions[$"{Name}_faked", Schema] != null)
            {
                TargetDb.UserDefinedFunctions[Name, Schema]?.DropIfExists();
                TargetDb.UserDefinedFunctions[$"{Name}_faked", Schema].Rename(Name);
            }
        }
    }
}
