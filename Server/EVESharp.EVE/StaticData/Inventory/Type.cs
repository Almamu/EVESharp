﻿/*
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

using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Inventory.Attributes;
using EVESharp.EVE.StaticData.Dogma;

namespace EVESharp.EVE.StaticData.Inventory;

public class Type
{
    public int                         ID                  { get; }
    public Group                       Group               { get; }
    public string                      Name                { get; }
    public string                      Description         { get; }
    public int                         GraphicID           { get; }
    public double                      Radius              { get; }
    public double                      Mass                { get; }
    public double                      Volume              { get; }
    public double                      Capacity            { get; }
    public int                         PortionSize         { get; }
    public int                         RaceID              { get; }
    public double                      BasePrice           { get; }
    public bool                        Published           { get; }
    public int                         MarketGroupID       { get; }
    public double                      ChanceOfDuplicating { get; }
    public Dictionary <int, Attribute> Attributes          { get; }
    public Dictionary <int, Effect>    Effects             { get; }
    public Dictionary <string, Effect> EffectsByName       { get; }

    public Type (
        int                      id,                  Group                       group,  string name,      string description,
        int                      graphicID,           double                      radius, double mass,      double volume,    double capacity,
        int                      portionSize,         int                         raceID, double basePrice, bool   published, int    marketGroupId,
        double                   chanceOfDuplicating, Dictionary <int, Attribute> defaultAttributes,
        Dictionary <int, Effect> effects
    )
    {
        ID                  = id;
        Group               = group;
        Name                = name;
        Description         = description;
        GraphicID           = graphicID;
        Radius              = radius;
        Mass                = mass;
        Volume              = volume;
        Capacity            = capacity;
        PortionSize         = portionSize;
        RaceID              = raceID;
        BasePrice           = basePrice;
        Published           = published;
        MarketGroupID       = marketGroupId;
        ChanceOfDuplicating = chanceOfDuplicating;
        Attributes          = defaultAttributes;
        Effects             = effects;
        EffectsByName       = Effects.ToDictionary (x => x.Value.EffectName, x => x.Value);
    }
}