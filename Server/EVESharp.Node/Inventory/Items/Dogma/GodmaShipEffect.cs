using System;
using System.Collections.Generic;
using EVESharp.EVE.Data.Dogma;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Dogma;

public class GodmaShipEffect
{
    public ShipModule AffectedItem { get; init; }
    public Effect     Effect       { get; init; }
    public bool       ShouldStart  { get; set; }
    public long       StartTime    { get; set; }
    public PyDataType Duration     { get; set; }

    public static implicit operator PyDataType (GodmaShipEffect effect)
    {
        return new PyList
        {
            effect.AffectedItem.ID,
            effect.Effect.EffectID,
            DateTime.UtcNow.ToFileTimeUtc (),
            effect.ShouldStart, // should this change?
            effect.ShouldStart, // should active change based on any condition?
            new PyTuple (7)
            {
                [0] = effect.AffectedItem.ID,
                [1] = effect.AffectedItem.OwnerID,
                [2] = effect.AffectedItem.LocationID,
                [3] = null, // target
                [4] = null,
                [5] = null,
                [6] = effect.Effect.EffectID
            },
            effect.StartTime,
            effect.Duration,
            effect.Effect.DisallowAutoRepeat == false,
            0, // random seed, doesn't seem to be used by the client anymore
            null // error (if any)
        };
    }

    public static implicit operator List <PyDataType> (GodmaShipEffect effect)
    {
        return new List <PyDataType>
        {
            effect.AffectedItem.ID,
            effect.Effect.EffectID,
            DateTime.UtcNow.ToFileTimeUtc (),
            effect.ShouldStart, // should this change?
            effect.ShouldStart, // should active change based on any condition?
            new PyTuple (7)
            {
                [0] = effect.AffectedItem.ID,
                [1] = effect.AffectedItem.OwnerID,
                [2] = effect.AffectedItem.LocationID,
                [3] = null, // target
                [4] = null,
                [5] = null,
                [6] = effect.Effect.EffectID
            },
            effect.StartTime,
            effect.Duration,
            effect.Effect.DisallowAutoRepeat == false,
            0, // random seed, doesn't seem to be used by the client anymore
            null // error (if any)
        };
    }
}