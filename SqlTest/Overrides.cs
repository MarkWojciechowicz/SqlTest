namespace SqlTest
{
    internal class Overrides
    {
        public Override[] Override { get; set; }
        public string GetValue(string columnName)
        {
            string value = "";
            foreach(var o in this.Override)
            {
                if(o.ColumnName == columnName)
                {  value = o.Value; break; }
            }
            return value;
        }

        public Overrides(Override[] overrides)
        {
            Override = overrides;
        }
    }

    internal class Override
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }

        public Override(string columnName, string value)
        {
            ColumnName = columnName;
            Value = value;
        }

    }
}