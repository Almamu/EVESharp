using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;

namespace EVESharp.Node.Dogma;

public class EffectsManager
{
    private Dogma Dogma { get; }
    private ItemFactory ItemFactory { get; }
    
    public EffectsManager(ItemFactory itemFactory, Dogma dogma)
    {
        this.ItemFactory = itemFactory;
        this.Dogma = dogma;
    }

    /// <summary>
    /// Provides access to the effect manager for the given item (creates a new one if the item doesn't have any assigned)
    /// </summary>
    /// <param name="module">The ship module to apply effects to</param>
    /// <returns></returns>
    public ItemEffects GetForItem(ShipModule module)
    {
        if (module is null)
            return null;
        
        return module.ItemEffects ??= new ItemEffects(module, this.ItemFactory);
    }
}