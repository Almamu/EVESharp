/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using Node.Database;
using Node.Dogma;
using Node.Dogma.Interpreter;
using Node.Inventory.Items;
using Node.Inventory.Items.Dogma;
using SimpleInjector;
using Container = SimpleInjector.Container;
using Environment = Node.Dogma.Interpreter.Environment;

namespace Node.Inventory
{
    public class ItemFactory
    {
        public NodeContainer Container { get; }
        public AttributeManager AttributeManager { get; private set; }
        public ItemManager ItemManager { get; private set; }
        public CategoryManager CategoryManager { get; private set; }
        public GroupManager GroupManager { get; private set; }
        public TypeManager TypeManager { get; private set; }
        public StationManager StationManager { get; private set; }
        public SystemManager SystemManager { get; private set; }
        public ItemDB ItemDB { get; private set; }
        public CharacterDB CharacterDB { get; private set; }
        public InsuranceDB InsuranceDB { get; private set; }
        private SimpleInjector.Container DependencyInjection { get; }
        
        public ItemFactory(NodeContainer container, SimpleInjector.Container dependencyInjection)
        {
            this.DependencyInjection = dependencyInjection;
            this.Container = container;
        }

        public void Init()
        {
            this.ItemDB = this.DependencyInjection.GetInstance<ItemDB>();
            this.CharacterDB = this.DependencyInjection.GetInstance<CharacterDB>();
            this.InsuranceDB = this.DependencyInjection.GetInstance<InsuranceDB>();

            this.SystemManager = this.DependencyInjection.GetInstance<SystemManager>();
            // station manager goes first
            this.StationManager = this.DependencyInjection.GetInstance<StationManager>();
            // attribute manager goes first
            this.AttributeManager = this.DependencyInjection.GetInstance<AttributeManager>();
            // category manager goes first
            this.CategoryManager = this.DependencyInjection.GetInstance<CategoryManager>();
            // then groups
            this.GroupManager = this.DependencyInjection.GetInstance<GroupManager>();
            // then the type manager
            this.TypeManager = this.DependencyInjection.GetInstance<TypeManager>();
            // finally the item manager
            this.ItemManager = this.DependencyInjection.GetInstance<ItemManager>();

            this.AttributeManager.Load();
            this.CategoryManager.Load();
            this.GroupManager.Load();
            this.TypeManager.Load();
            this.StationManager.Load();

            this.ItemManager.Load();
        }
    }
}