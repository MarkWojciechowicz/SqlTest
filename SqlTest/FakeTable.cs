using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace SqlTest
{
    internal class FakeTable
    {
        private string Name { get; set; }
        private string Schema { get; set; }
        private bool KeepIdentity { get; set; }
        private bool Exists { get; set; }
        private Database TargetDb { get; set; }
        internal FakeTable(Server server, string database, string name, string schema, bool keepIdentity = false) 
        {
            Name = name;
            Schema = schema;
            KeepIdentity = keepIdentity;
            TargetDb = server.Databases[database]; 
            Exists = TargetDb.Tables[$"{name}_faked", schema] != null;
        }

        internal void Create() 
        {
            if (Exists)
            {
                TestContext.WriteLine($"The table '{Schema}.{Name}' was faked.  Undoing fake...");
                this.Drop();
            }

            Table tableToFake = TargetDb.Tables[Name, Schema];
            if (tableToFake == null)
            {
                throw new Exception($"CreateFakeTable failed, table does not exist: '{Schema}.{Name}'");
            }

            var fake = new Table(TargetDb, Name, Schema);
            foreach(Column column in tableToFake.Columns)
            {
                var copyofCol = new Column(fake, column.Name, column.DataType);
                if (KeepIdentity)
                {
                    copyofCol.Identity = column.Identity;
                }
                if (column.DefaultConstraint != null)
                {
                    copyofCol.AddDefaultConstraint($"{column.DefaultConstraint.Name}_faked");
                    copyofCol.DefaultConstraint.Text = column.DefaultConstraint.Text;
                }
                fake.Columns.Add(copyofCol);
            }
            tableToFake.Rename($"{Name}_faked");
            fake.Create();
        }

        internal void Drop()
        {
            if (TargetDb.Tables[$"{Name}_faked", Schema] != null)
            {
                if (TargetDb.Tables[Name, Schema] != null)
                {
                    TargetDb.Tables[Name, Schema].DropIfExists();
                }
                TargetDb.Tables[$"{Name}_faked", Schema].Rename(Name);
            }
        }
    }
}
