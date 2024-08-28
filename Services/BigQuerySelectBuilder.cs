using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlQueryGenerator.Models;
using SqlQueryGenerator.Services;

namespace SqlQueryGenerator.Services
{
public class BigQuerySelectBuilder
{
    public string BuildSelectQuery(Table table, Dictionary<string, string> filters)
    {
        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.Append($"SELECT * FROM `{table.Name}` WHERE ");

        foreach (var filter in filters)
        {
            string[] filterParts = filter.Key.Split('.');

            // Handle JSON fields using JSON_EXTRACT_SCALAR
            if (filterParts.Length > 1)
            {
                var jsonColumn = table.Columns.FirstOrDefault(c => c.Name == filterParts[0] && c.IsJson);
                if (jsonColumn != null)
                {
                    queryBuilder.Append($"JSON_EXTRACT_SCALAR({jsonColumn.Name}, '$.{filterParts[1]}') = '{filter.Value}' AND ");
                }
            }
            else
            {
                var column = table.Columns.FirstOrDefault(c => c.Name == filter.Key);
                if (column != null)
                {
                    queryBuilder.Append($"{column.Name} = '{filter.Value}' AND ");
                }
            }
        }

        // Remove the trailing "AND" and finalize the query
        string query = queryBuilder.ToString().TrimEnd(" AND ".ToCharArray());
        return query;
    }
}
}