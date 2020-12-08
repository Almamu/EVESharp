/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
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

namespace Node.Inventory
{
    public class ItemFactory
    {
        public NodeContainer Container { get; }
        public AttributeManager AttributeManager { get; }
        public ItemManager ItemManager { get; }
        public CategoryManager CategoryManager { get; }
        public GroupManager GroupManager { get; }
        public TypeManager TypeManager { get; }
        public StationManager StationManager { get; }
        public ItemDB ItemDB { get; }
        public SkillDB SkillDB { get; }
        public CharacterDB CharacterDB { get; }
        public StationDB StationDB { get; }
        
        public ItemFactory(NodeContainer container)
        {
            this.Container = container;
            this.ItemDB = new ItemDB(container.Database, this);
            this.SkillDB = new SkillDB(container.Database, this);
            this.CharacterDB = new CharacterDB(container.Database, this);
            this.StationDB = new StationDB(container.Database);
            
            // station manager goes first
            this.StationManager = new StationManager(this);
            // attribute manager goes first
            this.AttributeManager = new AttributeManager(this);
            // category manager goes first
            this.CategoryManager = new CategoryManager(this);
            // then groups
            this.GroupManager = new GroupManager(this);
            // then the type manager
            this.TypeManager = new TypeManager(this);
            // finally the item manager
            this.ItemManager = new ItemManager(this);
        }

        public void Init()
        {
            this.AttributeManager.Load();
            this.CategoryManager.Load();
            this.GroupManager.Load();
            this.TypeManager.Load();
            this.ItemManager.Load();
        }
    }
}