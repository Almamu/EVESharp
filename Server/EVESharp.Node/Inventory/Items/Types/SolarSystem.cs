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

using System;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types;

public class SolarSystem : ItemInventory
{
    public Information.SolarSystem SolarSystemInformation { get; }
    public SolarSystem(Information.SolarSystem info) : base(info.Information)
    {
        this.SolarSystemInformation = info;
        this.BelongsToUs            = false;
    }
        
    public int    RegionId        => this.SolarSystemInformation.RegionId;
    public int    ConstellationId => this.SolarSystemInformation.ConstellationId;
    public double MapX            => this.SolarSystemInformation.MapX;
    public double MapY            => this.SolarSystemInformation.MapY;
    public double MapZ            => this.SolarSystemInformation.MapZ;
    public double MapXMin         => this.SolarSystemInformation.MapXMin;
    public double MapYMin         => this.SolarSystemInformation.MapYMin;
    public double MapZMin         => this.SolarSystemInformation.MapZMin;
    public double MapXMax         => this.SolarSystemInformation.MapXMax;
    public double MapYMax         => this.SolarSystemInformation.MapYMax;
    public double MapZMax         => this.SolarSystemInformation.MapZMax;
    public double Luminosity      => this.SolarSystemInformation.Luminosity;
    public bool   Border          => this.SolarSystemInformation.Border;
    public bool   Fringe          => this.SolarSystemInformation.Fringe;
    public bool   Corridor        => this.SolarSystemInformation.Corridor;
    public bool   Hub             => this.SolarSystemInformation.Hub;
    public bool   International   => this.SolarSystemInformation.International;
    public bool   Regional        => this.SolarSystemInformation.Regional;
    public bool   Constellation   => this.SolarSystemInformation.Constellation;
    public double Security        => this.SolarSystemInformation.Security;
    public int?   FactionId       => this.SolarSystemInformation.FactionId;
    public double Radius          => this.SolarSystemInformation.Radius;
    public int    SunTypeId       => this.SolarSystemInformation.SunTypeId;
    public string SecurityClass   => this.SolarSystemInformation.SecurityClass;
    public bool   BelongsToUs     { get; set; }

        
    protected override void LoadContents(Flags ignoreFlags = Flags.None)
    {
        throw new NotImplementedException();
    }

    public override void Persist()
    {
        // solar systems cannot be updated
        throw new NotImplementedException();
    }

    public override void Destroy()
    {
        throw new NotImplementedException("Stations cannot be destroyed as they're regarded as static data!");
    }

    public PyDataType GetSolarSystemInfo()
    {
        // TODO: CHECK WHERE WE CAN FETCH allianceID, sovereigntyLevel and constellationSovereignty
        // TODO: AS THESE SEEM TO BE DYNAMIC VALUES
        return new Row(
            new PyList<PyString>(14)
            {
                [0]  = "solarSystemID",
                [1]  = "solarSystemName",
                [2]  = "x",
                [3]  = "y",
                [4]  = "z",
                [5]  = "radius",
                [6]  = "security",
                [7]  = "constellationID",
                [8]  = "factionID",
                [9]  = "sunTypeID",
                [10] = "regionID",
                [11] = "allianceID",
                [12] = "sovereigntyLevel",
                [13] = "constellationSovereignty"
            },
            new PyList(14)
            {
                [0]  = this.ID,
                [1]  = this.Name,
                [2]  = this.X,
                [3]  = this.Y,
                [4]  = this.Z,
                [5]  = this.Radius,
                [6]  = this.Security,
                [7]  = this.ConstellationId,
                [8]  = this.FactionId,
                [9]  = this.SunTypeId,
                [10] = this.RegionId,
                [11] = null,
                [12] = 0,
                [13] = 0
            }
        );
    }
}