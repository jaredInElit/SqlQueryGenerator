using System.Text;
using SqlQueryGenerator.Models;
namespace SqlQueryGenerator.Services
{
    public class SqlQueryGenerator
    {
        public string GenerateSelectQuery(Table table, Dictionary<string, string> filters)
        {
            StringBuilder queryBuilder = new StringBuilder();

            queryBuilder.Append($"SELECT * FROM {table.Name} WHERE ");
            foreach (var filter in filters)
            {
                var column = table.Columns.FirstOrDefault(c=> c.Name == filter.Key);
                if (column != null)
                {
                    if (column.IsJson)
                    {
                        queryBuilder.Append($"JSON_EXTRACT_SCALAR({column.Name}, '$.{filter.Key}') = '{filter.Value}' AND ");
                    }
                    else
                    {
                        queryBuilder.Append($"{column.Name} = '{filter.Value}' AND ");
                    }
                }
            }
            string query = queryBuilder.ToString().TrimEnd(" AND ".ToCharArray());

            return query;
        }
    }
    public class DatabaseSchema
    {
        public stirng Name {get; set;} = string.Empty;
        public List<Table> Tables {get; set} = new List<Table>();
        public DatabaseSchema(string name)
        {
            Name = name;
        }
        public Table? GetTableByName(string tableName)
        {
            return Tables.FirstOrDefault(t => t.Name == tableName);
        }
    }
}