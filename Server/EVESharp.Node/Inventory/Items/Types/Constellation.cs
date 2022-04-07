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
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class Constellation : ItemInventory
{
    public Information.Constellation ConstellationInformation { get; }
    public int                       RegionId                 => ConstellationInformation.RegionId;
    public double                    X                        => ConstellationInformation.X;
    public double                    Y                        => ConstellationInformation.Y;
    public double                    Z                        => ConstellationInformation.Z;
    public double                    XMin                     => ConstellationInformation.XMin;
    public double                    YMin                     => ConstellationInformation.YMin;
    public double                    ZMin                     => ConstellationInformation.ZMin;
    public double                    XMax                     => ConstellationInformation.XMax;
    public double                    YMax                     => ConstellationInformation.YMax;
    public double                    ZMax                     => ConstellationInformation.ZMax;
    public int?                      FactionId                => ConstellationInformation.FactionId;
    public double                    Radius                   => ConstellationInformation.Radius;

    public Constellation (Information.Constellation constellation) : base (constellation.Information)
    {
        ConstellationInformation = constellation;
    }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        throw new NotImplementedException ();
    }

    public override void Persist ()
    {
        // constellations cannot be updated
        throw new NotImplementedException ();
    }

    public override void Destroy ()
    {
        throw new NotImplementedException ("Stations cannot be destroyed as they're regarded as static data!");
    }
}