using Node.Dogma;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Dogma
{
    public class Expression
    {
        public int ID { get; init; }
        public EffectOperand Operand { get; init; }
        public string ExpressionValue { get; init; }
        public string ExpressionName { get; init; }
        public Expression FirstArgument { get; set; }
        public Expression SecondArgument { get; set; }
        public AttributeEnum? AttributeID { get; init; }
        public int? ItemTypeID { get; init; }
        public byte[] VMCode { get; private set; }

        public void Compile()
        {
            this.VMCode = new Compiler().CompileExpression(this);
        }
    }
}