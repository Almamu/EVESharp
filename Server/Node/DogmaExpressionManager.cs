using System.Collections.Generic;
using Common.Logging;
using Node.Database;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Dogma;
using Node.Inventory.Items.Types;

namespace Node
{
    public class DogmaExpressionManager
    {
        private Channel Log { get; }
        private DogmaDB DB { get; }
        private Dictionary<int, Expression> Expressions { get; }
        
        public DogmaExpressionManager(DogmaDB db, Logger logger)
        {
            this.DB = db;
            this.Log = logger.CreateLogChannel("DogmaExpressionManager");
            this.Expressions = this.DB.LoadDogmaExpressions();

            Log.Debug($"Loaded {this.Expressions.Count} expressions for Dogma");
        }

        public Expression this[int index] => this.Expressions[index];
        
        public void ApplyEffects(ShipModule item, Expression expression, Character character, Ship ship)
        {
            
        }
    }
}