using System.IO;

namespace EVESharp.Database.Dogma;

/// <summary>
/// Compiler tools to take dogma expressions and convert them to VM code that can be run inside C#
///
/// For now a proof of concept, but in the future it should be able to produce proper IL code to be run
/// for improved performance
/// </summary>
public class Compiler
{
    /// <summary>
    /// Takes a dogma expression and outputs the proper bytecode for the VM to interpret it
    /// </summary>
    /// <param name="rootExpression">The expression to compile</param>
    /// <returns>The bytecode for the expression</returns>
    public byte [] CompileExpression (Expression rootExpression)
    {
        MemoryStream stream = new MemoryStream ();
        BinaryWriter writer = new BinaryWriter (stream);

        this.CompileOpcode (rootExpression, writer);

        // return the full buffer
        return stream.GetBuffer ();
    }

    private void CompileOpcode (Expression expression, BinaryWriter writer)
    {
        // as long as there's a first argument, that one goes before the current one
        writer.WriteOperand (expression.Operand);

        // write the first argument
        if (expression.FirstArgument is not null)
            this.CompileOpcode (expression.FirstArgument, writer);
        // if there's a value also write the string to the buffer
        if (expression.ExpressionValue is not null)
            writer.Write (expression.ExpressionValue);
        // if there's a attributeID also write it to the buffer
        if (expression.AttributeID is not null)
            writer.Write ((int) expression.AttributeID);
        // finally write the second argument
        if (expression.SecondArgument is not null)
            this.CompileOpcode (expression.SecondArgument, writer);
    }
}