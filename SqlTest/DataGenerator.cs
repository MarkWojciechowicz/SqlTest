using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework.Internal;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SqlTest
{
    internal class DataGenerator
    {
        public Table Table { get; set; }
        public Overrides Overrides { get; set; }
        public bool Randomize { get; set; }
        private static Random random = new Random();
        public DataGenerator(Table table, string? columnOverrides, bool randomize = false) 
        {
            Table = table;
            if (String.IsNullOrEmpty(columnOverrides))
            {
                Override[] o = new Override[0];
                Overrides = new(o);
            }
            else
            {
                string[] colOverrides = columnOverrides.Split(';');
                Override[] o = new Override[colOverrides.Length];
                for (int i = 0; i < o.Length; i++)
                {
                    string column = colOverrides[i].Split('=')[0].Trim();
                    string value = colOverrides[i].Split("=")[1].Trim();
                    o[i] = new Override(column, value);
                }
                Overrides = new Overrides(o);
            }
            Randomize = randomize;
        }

        public string GenerateInsert()
        {
            StringBuilder sql = new();
            sql.AppendLine($"INSERT INTO [{Table.Schema}].[{Table.Name}] (");
            string comma = "";
            foreach(Column column in Table.Columns)
            {
                if (!column.Identity)
                {
                    sql.AppendLine($"{comma} [{column.Name}]");
                    comma = ",";
                }
            }
            sql.AppendLine(") VALUES (");
            comma = "";
            foreach (Column column in Table.Columns)
            {
                if (!column.Identity)
                {
                    sql.AppendLine($"{comma} {GetValueforType(column)}");
                    comma = ",";
                }
            }
            sql.AppendLine(")");
            return sql.ToString();
        }

        private string GetValueforType(Column column)
        {
            var _randomize = this.Randomize;
            string value;
            value = Overrides.GetValue(column.Name);
            if (String.IsNullOrEmpty(value))
            {
                if (!String.IsNullOrEmpty(column.Default) || column.DataType.Name == "timestamp") { value = "DEFAULT"; }
                else if (column.DataType.IsStringType) { value = GetStringValue(column, _randomize); }
                else if (column.DataType.IsNumericType) { value = GetNumericValue(column, _randomize); }
                else
                {
                    value = column.DataType.SqlDataType.ToString() switch
                    {
                        "Date" or "DateTime" or "DateTime2" or "DateTimeOffset" or "SmallDateTime" or "Timestamp" => GetDateValue(column),
                        "Time" => GetTimeValue(column),
                        "Bit" => GetBitValue(column),
                        "Binary" or "VarBinary" or "VarBinaryMax" => GetBinaryValue(column),
                        "UniqueIdentifier" => GetUniqueIdentifierValue(column, _randomize),
                        "SysName" => GetStringValue(column),
                        _ => "null",
                    };
                }
            }
            return value;
        }

        private static string GetBinaryValue(Column column)
        {
            int len = column.DataType.MaximumLength == -1 ? 8 : column.DataType.MaximumLength;
            string _value = "0x0" + new string('0', len);
            return _value;
        }

        private static string GetStringValue(Column column, bool _randomize = false)
        {
            string _value;
            int len = column.DataType.MaximumLength == -1 ? 8 : column.DataType.MaximumLength;
            _value = _randomize ? GetRandomString(len) : new('a', len);
            _value = "'" + _value + "'";
            return _value;
        }

        private static string GetNumericValue(Column column, bool _randomize = false)
        {
            string _value = _randomize ? RandomNumberGenerator.GetInt32(1, 154).ToString() : "0";
            return _value;
        }

        private static string GetDateValue(Column column)
        {
            string _value = "1/1/2010";
            _value = "'" + _value + "'";
            return _value;
        }

        private static string GetTimeValue(Column column)
        {
            string _value = "12:00:00";
            _value = "'" + _value + "'";
            return _value;
        }

        private static string GetBitValue(Column column)
        {
            string _value = "0";
            return _value;
        }

        private static string GetUniqueIdentifierValue(Column column, bool _randomize = false)
        {
            var newid = Guid.NewGuid();
            string _value = _randomize ? newid.ToString() : "EAB932A9-3B1A-460B-9FFD-BAC9F2B95D92";
            _value = "'" + _value + "'";
            return _value;
        }

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklomnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
