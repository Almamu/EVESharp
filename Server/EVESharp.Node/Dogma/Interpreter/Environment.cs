﻿using EVESharp.EVE.Sessions;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;

namespace EVESharp.Node.Dogma.Interpreter
{
    public class Environment
    {
        public ItemEntity Self { get; init; }
        public Character Character { get; init; }
        public Ship Ship { get; init; }
        public ItemEntity Target { get; init; }
        public Session Session { get; init; }
        public ItemFactory ItemFactory { get; init; }
    }
}