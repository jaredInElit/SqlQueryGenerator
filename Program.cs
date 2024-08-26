
using System;
using System.Collections.Generic;
using SqlQueryGenerator.Models;
using SqlQueryGenerator.Services;

namespace SqlQueryGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var table = new Table
            {
            Name = "AdvertiserTable",
            Columns = new List<TableColumn>
            {
                new TableColumn { Name = "id", DataType = "INT" },
                new TableColumn { Name = "name", DataType = "STRING" },
                new TableColumn { Name = "data", DataType = "JSON", IsJson = true }
            }
            };

            var filters = new Dictionary<string, string>
            {
                { "name", "ExampleName" },
                { "data.someField", "SomeValue" }
            };

            var sqlGenerator = new SqlQueryGenerator.Services.SqlQueryGenerator();
            string query = sqlGenerator.GenerateSelectQuery(table, filters);

            Console.WriteLine(query);
        }
    }
}