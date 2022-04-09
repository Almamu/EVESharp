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
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types;

public class SolarSystem : ItemInventory
{
    public Information.SolarSystem SolarSystemInformation { get; }

    public int    RegionId        => SolarSystemInformation.RegionId;
    public int    ConstellationId => SolarSystemInformation.ConstellationId;
    public double MapX            => SolarSystemInformation.MapX;
    public double MapY            => SolarSystemInformation.MapY;
    public double MapZ            => SolarSystemInformation.MapZ;
    public double MapXMin         => SolarSystemInformation.MapXMin;
    public double MapYMin         => SolarSystemInformation.MapYMin;
    public double MapZMin         => SolarSystemInformation.MapZMin;
    public double MapXMax         => SolarSystemInformation.MapXMax;
    public double MapYMax         => SolarSystemInformation.MapYMax;
    public double MapZMax         => SolarSystemInformation.MapZMax;
    public double Luminosity      => SolarSystemInformation.Luminosity;
    public bool   Border          => SolarSystemInformation.Border;
    public bool   Fringe          => SolarSystemInformation.Fringe;
    public bool   Corridor        => SolarSystemInformation.Corridor;
    public bool   Hub             => SolarSystemInformation.Hub;
    public bool   International   => SolarSystemInformation.International;
    public bool   Regional        => SolarSystemInformation.Regional;
    public bool   Constellation   => SolarSystemInformation.Constellation;
    public double Security        => SolarSystemInformation.Security;
    public int?   FactionId       => SolarSystemInformation.FactionId;
    public double Radius          => SolarSystemInformation.Radius;
    public int    SunTypeId       => SolarSystemInformation.SunTypeId;
    public string SecurityClass   => SolarSystemInformation.SecurityClass;
    public bool   BelongsToUs     { get; set; }

    public SolarSystem (Information.SolarSystem info) : base (info.Information)
    {
        SolarSystemInformation = info;
        BelongsToUs            = false;
    }


    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        throw new NotImplementedException ();
    }

    public override void Persist ()
    {
        // solar systems cannot be updated
        throw new NotImplementedException ();
    }

    public override void Destroy ()
    {
        throw new NotImplementedException ("Stations cannot be destroyed as they're regarded as static data!");
    }

    public PyDataType GetSolarSystemInfo ()
    {
        // TODO: CHECK WHERE WE CAN FETCH allianceID, sovereigntyLevel and constellationSovereignty
        // TODO: AS THESE SEEM TO BE DYNAMIC VALUES
        return new Row (
            new PyList <PyString> (14)
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
            new PyList (14)
            {
                [0]  = ID,
                [1]  = Name,
                [2]  = X,
                [3]  = Y,
                [4]  = Z,
                [5]  = Radius,
                [6]  = Security,
                [7]  = ConstellationId,
                [8]  = FactionId,
                [9]  = SunTypeId,
                [10] = RegionId,
                [11] = null,
                [12] = 0,
                [13] = 0
            }
        );
    }
}