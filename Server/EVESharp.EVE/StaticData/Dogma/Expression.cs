using EVESharp.EVE.Dogma;
using EVESharp.EVE.StaticData.Inventory;

namespace EVESharp.EVE.StaticData.Dogma;

public class Expression
{
    public int                              ID              { get; init; }
    public EffectOperand                    Operand         { get; init; }
    public string                           ExpressionValue { get; init; }
    public string                           ExpressionName  { get; init; }
    public Expression                       FirstArgument   { get; set; }
    public Expression                       SecondArgument  { get; set; }
    public AttributeTypes? AttributeID     { get; init; }
    public byte []                          VMCode          { get; private set; }

    public void Compile ()
    {
        VMCode = new Compiler ().CompileExpression (this);
    }
}