namespace SqlQueryGenerator.Models
{
    public class TableColumn
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsJson { get; set; }
    }

    public class Table
    {
        public string Name { get; set; }
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
    }

    public class DatabaseSchema
    {
        public string Name { get; set; }
        public List<Table> Tables { get; set; } = new List<Table>();

        public Table? GetTableByName(string tableName)
        {
            return Tables.Find(t => t.Name == tableName);
        }
    }
    public class Schemas
    {
        public List<DatabaseSchema> DatabaseSchemas { get; set; } = new List<DatabaseSchema>();
    }
}