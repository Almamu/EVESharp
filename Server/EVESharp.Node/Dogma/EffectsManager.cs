using EVESharp.EVE.Sessions;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;

namespace EVESharp.Node.Dogma;

public class EffectsManager
{
    private DogmaUtils  DogmaUtils  { get; }
    private ItemFactory ItemFactory { get; }

    public EffectsManager (ItemFactory itemFactory, DogmaUtils dogmaUtils)
    {
        ItemFactory = itemFactory;
        DogmaUtils  = dogmaUtils;
    }

    /// <summary>
    /// Provides access to the effect manager for the given item (creates a new one if the item doesn't have any assigned)
    /// </summary>
    /// <param name="module">The ship module to apply effects to</param>
    /// <param name="session">The session attached as owner of the effects</param>
    /// <returns></returns>
    public ItemEffects GetForItem (ShipModule module, Session session)
    {
        if (module is null)
            return null;

        return module.ItemEffects ??= new ItemEffects (module, ItemFactory, session);
    }
}