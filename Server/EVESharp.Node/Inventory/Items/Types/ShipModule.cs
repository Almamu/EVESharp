using System;
using System.Linq;
using System.Runtime.InteropServices;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma;
using EVESharp.Node.Dogma.Interpreter;
using EVESharp.Node.Dogma.Interpreter.Opcodes;
using EVESharp.Node.Exceptions.dogma;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Dogma;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types
{
    public class ShipModule : ItemEntity
    {
        public GodmaShipEffectsList Effects { get; }
        public ItemEffects ItemEffects { get; set; }
        
        public ShipModule(Information.Item info) : base(info)
        {
            this.Effects = new GodmaShipEffectsList();
        }
        
        public override PyDictionary GetEffects()
        {
            return this.Effects;
        }

        public bool IsHighSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.HighPower) == true;
        }

        public bool IsMediumSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.MedPower) == true;
        }

        public bool IsLowSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.LowPower) == true;
        }

        public bool IsRigSlot()
        {
            return this.Effects.ContainsKey((int) EffectsEnum.RigSlot) == true;
        }
    }
}