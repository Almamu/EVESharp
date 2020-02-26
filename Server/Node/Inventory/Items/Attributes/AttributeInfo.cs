namespace Node.Inventory.Items.Attributes
{
    public class AttributeInfo
    {
        public int ID { get; }
        public string Name { get; }
        public int Category { get; }
        public string Description { get; }
        public AttributeInfo MaxAttribute { get; } // TODO: REFERENCE TO ANOTHER ATTRIBUTE INFO
        public int AttributeIDX { get; }
        public int GraphicID { get; }
        public int ChargeRechargeTimeID { get; }
        public double DefaultValue { get; }
        public int Published { get; }
        public string DisplayName { get; }
        public int UnitID { get; } // TODO: STORE UNITS IN MEMORY TOO
        public int Stackable { get; }
        public int HighIsGood { get; }
        public int CategoryID { get; }

        public AttributeInfo(
            int id, string name, int category, string description, AttributeInfo maxAttribute, int attributeIdx, int graphicId,
            int chargeRechargeTimeId, double defaultValue, int published, string displayName, int unitId, int stackable,
            int highIsGood, int categoryId)
        {
            ID = id;
            Name = name;
            Category = category;
            Description = description;
            MaxAttribute = maxAttribute;
            AttributeIDX = attributeIdx;
            GraphicID = graphicId;
            ChargeRechargeTimeID = chargeRechargeTimeId;
            DefaultValue = defaultValue;
            Published = published;
            DisplayName = displayName;
            UnitID = unitId;
            Stackable = stackable;
            HighIsGood = highIsGood;
            CategoryID = categoryId;
        }
    }
}