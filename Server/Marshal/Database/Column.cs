namespace Marshal.Database
{

    public class Column
    {
        public string Name { get; private set; }
        public FieldType Type { get; private set; }
        public PyObject Value { get; set; }

        public Column(string name, FieldType type)
        {
            Name = name;
            Type = type;
        }
    }

}