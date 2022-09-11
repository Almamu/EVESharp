using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Database;
using Serilog;

namespace EVESharp.Database.Inventory;

public static class DogmaDB
{
    public static Dictionary <int, Expression> InvDgmLoadExpressions (this IDatabaseConnection Database, ILogger Log)
    {
        IDataReader reader = Database.Select (
            "SELECT expressionID, operandID, arg1, arg2, expressionValue, expressionName, expressionAttributeID FROM dgmExpressions ORDER BY arg1, arg2, expressionID"
        );

        Dictionary <int, Expression>            expressions  = new Dictionary <int, Expression> ();
        Dictionary <int, DogmaExpressionStruct> databaseLoad = new Dictionary <int, DogmaExpressionStruct> ();

        using (reader)
        {
            while (reader.Read ())
                databaseLoad [reader.GetInt32 (0)] = new DogmaExpressionStruct
                {
                    ExpressionID    = reader.GetInt32 (0),
                    Operand         = (EffectOperand) reader.GetInt32 (1),
                    ExpressionName  = reader.GetStringOrNull (5),
                    ExpressionValue = reader.GetStringOrNull (4),
                    FirstArgument   = reader.GetInt32OrNull (2),
                    SecondArgument  = reader.GetInt32OrNull (3),
                    AttributeID     = reader.GetInt32OrNull (6)
                };

            // now that there's some kind of cache of dogma expressions start loading things one by one
            foreach ((int _, DogmaExpressionStruct expression) in databaseLoad)
                Database.LoadExpression (expressions, databaseLoad, expression, Log);

            // compile all the expressions
            foreach ((int _, Expression expression) in expressions)
                expression.Compile ();

            return expressions;
        }
    }

    private static Expression LoadExpression (
        this IDatabaseConnection                Database,
        Dictionary <int, Expression>            dogmaExpressions,
        Dictionary <int, DogmaExpressionStruct> expressionList,
        DogmaExpressionStruct                   expressionToLoad,
        ILogger                                 Log
    )
    {
        // ignore loaded expressions
        if (dogmaExpressions.TryGetValue (expressionToLoad.ExpressionID, out Expression expression))
            return expression;

        Expression firstArgument  = null;
        Expression secondArgument = null;

        // this expression is not loaded, ensure that args are properly loaded first
        if (expressionToLoad.FirstArgument is not null && dogmaExpressions.TryGetValue ((int) expressionToLoad.FirstArgument, out firstArgument) == false)
        {
            if (expressionList.ContainsKey ((int) expressionToLoad.FirstArgument) == false)
            {
                Log.Warning ($"Referenced expression {expressionToLoad.FirstArgument} cannot be found in the list... Ignoring");
                expressionToLoad.FirstArgument = null;
            }
            else
            {
                firstArgument = Database.LoadExpression (dogmaExpressions, expressionList, expressionList [(int) expressionToLoad.FirstArgument], Log);
            }
        }

        if (expressionToLoad.SecondArgument is not null && dogmaExpressions.TryGetValue ((int) expressionToLoad.SecondArgument, out secondArgument) == false)
        {
            if (expressionList.ContainsKey ((int) expressionToLoad.SecondArgument) == false)
            {
                Log.Warning ($"Referenced expression {expressionToLoad.SecondArgument} cannot be found in the list... Ignoring");
                expressionToLoad.SecondArgument = null;
            }
            else
            {
                secondArgument = Database.LoadExpression (dogmaExpressions, expressionList, expressionList [(int) expressionToLoad.SecondArgument], Log);
            }
        }

        // now that the "dependencies" are loaded mark the current expression as loaded
        return dogmaExpressions [expressionToLoad.ExpressionID] = new Expression
        {
            ID              = expressionToLoad.ExpressionID,
            Operand         = expressionToLoad.Operand,
            ExpressionName  = expressionToLoad.ExpressionName,
            ExpressionValue = expressionToLoad.ExpressionValue,
            FirstArgument   = firstArgument,
            SecondArgument  = secondArgument,
            AttributeID     = expressionToLoad.AttributeID is null ? null : (AttributeTypes) expressionToLoad.AttributeID
        };
    }

    private struct DogmaExpressionStruct
    {
        public int           ExpressionID;
        public EffectOperand Operand;
        public string        ExpressionName;
        public string        ExpressionValue;
        public int?          FirstArgument;
        public int?          SecondArgument;
        public int?          AttributeID;
    }
}