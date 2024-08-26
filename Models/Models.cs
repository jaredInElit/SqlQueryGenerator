namespace SqlQueryGenerator.Models
{
    public class TableColumn
    {
        public string Name {get; set;}
        public string DataType {get; set;}
        public bool IsJson {get; set;}
    }

    public class Table
    {
        public string Name {get; set;}
        public List<TableColumn> Columns {get; set;}
    
        public Table()
        {
            Columns = new List<TableColumn>();
        }
    }
}