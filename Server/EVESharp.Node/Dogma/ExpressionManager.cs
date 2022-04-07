using System.Collections.Generic;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Dogma;
using Serilog;

namespace EVESharp.Node.Dogma;

public class ExpressionManager
{
    private ILogger                     Log         { get; }
    private DogmaDB                     DB          { get; }
    private Dictionary<int, Expression> Expressions { get; }
        
    public ExpressionManager(DogmaDB db, ILogger logger)
    {
        this.DB          = db;
        this.Log         = logger;
        this.Expressions = this.DB.LoadDogmaExpressions();

        Log.Debug($"Loaded {this.Expressions.Count} expressions for Dogma");
    }

    public Expression this[int index] => this.Expressions[index];
}