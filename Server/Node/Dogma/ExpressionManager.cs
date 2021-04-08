using System.Collections.Generic;
using Common.Logging;
using Node.Database;
using Node.Inventory.Items.Dogma;

namespace Node.Dogma
{
    public class ExpressionManager
    {
        private Channel Log { get; }
        private DogmaDB DB { get; }
        private Dictionary<int, Expression> Expressions { get; }
        
        public ExpressionManager(DogmaDB db, Logger logger)
        {
            this.DB = db;
            this.Log = logger.CreateLogChannel("DogmaExpressionManager");
            this.Expressions = this.DB.LoadDogmaExpressions();

            Log.Debug($"Loaded {this.Expressions.Count} expressions for Dogma");
        }

        public Expression this[int index] => this.Expressions[index];
    }
}