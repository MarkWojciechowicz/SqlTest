namespace SqlTest
{
    internal class SqlObject
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public SqlObject(string sqlObjectName)
        {
            string[] parts = sqlObjectName.Split('.');
            if (parts.Length == 1)
            {
                Name = parts[0].Replace("[", "").Replace("]", "");
                Schema = "dbo";
            }
            else if (parts.Length == 2)
            {
                Name = parts[1].Replace("[", "").Replace("]", "");
                Schema = parts[0].Replace("[", "").Replace("]", "");
            }
            else
            {
                throw new Exception($"Sql Object in unexpected form: '{sqlObjectName}'");
            }
        }
    }
}
