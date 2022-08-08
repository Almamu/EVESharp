using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Data.Inventory;

namespace EVESharp.Node.Dogma;

public class EffectsManager
{
    private IDogmaNotifications  DogmaNotifications  { get; }
    private IItems Items { get; }

    public EffectsManager (IItems items, IDogmaNotifications dogmaNotifications)
    {
        this.Items             = items;
        this.DogmaNotifications = dogmaNotifications;
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

        return module.ItemEffects ??= new ItemEffects (module, this.Items, this.DogmaNotifications, session);
    }
}