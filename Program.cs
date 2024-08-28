using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SqlQueryGenerator.Models;
using SqlQueryGenerator.Services;

namespace SqlQueryGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Path to the JSON schema file
            string jsonFilePath = @"C:\Users\jboschmann\Documents\K4P_Schemas.json";
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("The JSON file does not exist at the specified path.");
                return;
            }

            // Read and parse the JSON file
            var schemas = LoadSchemasFromJson(jsonFilePath);

            if (schemas == null)
            {
                Console.WriteLine("Failed to load schemas.");
                return;
            }

            // Allow the user to select a schema, table, and columns
            DatabaseSchema selectedSchema = SelectSchemaByName(schemas);
            Table selectedTable = SelectTable(selectedSchema);
            List<TableColumn> selectedColumns = SelectColumns(selectedTable);

            // Example: Build a query based on selected schema, table, and columns
            var filters = new Dictionary<string, string>
            {
                { "name", "ExampleName" },           // Filter for a regular column
                { "data.someField", "SomeValue" }    // Filter for a JSON field
            };

            var queryBuilder = new BigQuerySelectBuilder();
            string query = queryBuilder.BuildSelectQuery(selectedTable, selectedColumns, filters);

            Console.WriteLine("Generated Query:");
            Console.WriteLine(query);
        }

        static Schemas? LoadSchemasFromJson(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                Console.WriteLine("Loaded JSON Content:");
                Console.WriteLine(jsonString); // Print JSON content for verification
        
                var schemas = JsonSerializer.Deserialize<Schemas>(jsonString);
                if (schemas != null && schemas.DatabaseSchemas.Any())
                {
                    Console.WriteLine("Schemas loaded successfully:");
                    foreach (var schema in schemas.DatabaseSchemas)
                    {
                        Console.WriteLine($"- {schema.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("No schemas found in the JSON file.");
                }

                    return schemas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading JSON schema: {ex.Message}");
                return null;
            }
        }

        static DatabaseSchema SelectSchemaByName(Schemas schemas)
        {
            Console.WriteLine("Available schemas:");
            
            // Display the list of schema names
            foreach (var schema in schemas.DatabaseSchemas)
            {
                Console.WriteLine($"- {schema.Name}");
            }

            // Prompt the user to enter the schema name
            while (true)
            {
                Console.Write("Enter the schema name: ");
                string input = Console.ReadLine();

                // Attempt to find the schema by name
                var selectedSchema = schemas.DatabaseSchemas.FirstOrDefault(s => s.Name.Equals(input, StringComparison.OrdinalIgnoreCase));
                
                if (selectedSchema != null)
                {
                    return selectedSchema;
                }

                Console.WriteLine("Invalid schema name. Please enter a valid schema name from the list above.");
            }
        }

        static Table SelectTable(DatabaseSchema schema)
        {
            Console.WriteLine("Select a table:");

            // Display the list of tables
            for (int i = 0; i < schema.Tables.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {schema.Tables[i].Name}");
            }

            // Prompt the user to select a table by number
            int tableChoice = GetValidSelection(1, schema.Tables.Count);
            return schema.Tables[tableChoice - 1]; // Adjust index for zero-based list
        }

        static List<TableColumn> SelectColumns(Table table)
        {
            Console.WriteLine("Select columns (comma-separated numbers):");

            // Display the list of columns
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {table.Columns[i].Name} ({table.Columns[i].DataType})");
            }

            // Prompt the user to select columns by number
            var selectedColumns = new List<TableColumn>();
            while (true)
            {
                Console.Write($"Enter numbers between 1 and {table.Columns.Count} (e.g., 1,3,5): ");
                string input = Console.ReadLine();

                var indices = input?.Split(',')
                                    .Select(s => s.Trim())
                                    .Where(s => int.TryParse(s, out _))
                                    .Select(int.Parse)
                                    .Where(index => index >= 1 && index <= table.Columns.Count)
                                    .Distinct()
                                    .ToList();

                if (indices != null && indices.Any())
                {
                    foreach (var index in indices)
                    {
                        selectedColumns.Add(table.Columns[index - 1]); // Adjust index for zero-based list
                    }
                    break;
                }

                Console.WriteLine("Invalid input. Please enter valid numbers.");
            }

            return selectedColumns;
        }

        static int GetValidSelection(int min, int max)
        {
            while (true)
            {
                Console.Write($"Enter a number between {min} and {max}: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
                {
                    return choice;
                }

                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }
    }

    // Placeholder for your query builder class
    public class BigQuerySelectBuilder
    {
        public string BuildSelectQuery(Table table, List<TableColumn> selectedColumns, Dictionary<string, string> filters)
        {
            var columnList = string.Join(", ", selectedColumns.Select(c => c.Name));
            var queryBuilder = new System.Text.StringBuilder($"SELECT {columnList} FROM `{table.Name}` WHERE ");

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
