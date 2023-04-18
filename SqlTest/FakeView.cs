using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace SqlTest
{
    internal class FakeView
    {
        private string Name { get; set; }
        private string Schema { get; set; }
        private bool Exists { get; set; }
        private Database TargetDb { get; set; }
        internal FakeView(Server server, string database, string name, string schema, bool keepIdentity = false)
        {
            Name = name;
            Schema = schema;
            TargetDb = server.Databases[database];
            Exists = TargetDb.Views[$"{name}_faked", schema] != null;
        }

        internal void Create()
        {
            if (Exists)
            {
                TestContext.WriteLine($"The view '{Schema}.{Name}' was faked.  Undoing fake...");
                this.Drop();
            }
            View viewToFake = TargetDb.Views[Name, Schema] ?? throw new Exception($"CreateFakeView failed, table does not exist: '{Schema}.{Name}'");
            var fake = new Table(TargetDb, Name, Schema);
            foreach (Column column in viewToFake.Columns)
            {
                var copyofCol = new Column(fake, column.Name, column.DataType);
                fake.Columns.Add(copyofCol);
            }
            viewToFake.Rename($"{Name}_faked");
            fake.Create();
        }

        internal void Drop()
        {
            if (TargetDb.Views[$"{Name}_faked", Schema] != null)
            {
                if (TargetDb.Tables[Name, Schema] != null)
                {
                    TargetDb.Tables[Name, Schema].DropIfExists();
                }
                TargetDb.Views[$"{Name}_faked", Schema].Rename(Name);
            }
        }
    }
}
