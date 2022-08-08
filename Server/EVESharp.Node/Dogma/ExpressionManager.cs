using System.Collections.Generic;
using EVESharp.EVE.Data.Dogma;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Node.Database;
using Serilog;

namespace EVESharp.Node.Dogma;

public class ExpressionManager : IExpressions
{
    private ILogger                      Log         { get; }
    private DogmaDB                      DB          { get; }
    private Dictionary <int, Expression> Expressions { get; }

    public Expression this [int index] => Expressions [index];

    public ExpressionManager (DogmaDB db, ILogger logger)
    {
        DB          = db;
        Log         = logger;
        Expressions = DB.LoadDogmaExpressions ();

        Log.Debug ($"Loaded {Expressions.Count} expressions for Dogma");
    }
}