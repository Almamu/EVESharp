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
using EVESharp.EVE.Data.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class Region : ItemInventory
{
    public Information.Region RegionInformation { get; }

    public double XMin      => RegionInformation.XMin;
    public double YMin      => RegionInformation.YMin;
    public double ZMin      => RegionInformation.ZMin;
    public double XMax      => RegionInformation.XMax;
    public double YMax      => RegionInformation.YMax;
    public double ZMax      => RegionInformation.ZMax;
    public int?   FactionID => RegionInformation.FactionID;
    public double Radius    => RegionInformation.Radius;

    public Region (Information.Region region) : base (region.Information)
    {
        RegionInformation = region;
    }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        throw new NotImplementedException ();
    }

    public override void Persist ()
    {
        // regions cannot be updated
        throw new NotImplementedException ();
    }

    public override void Destroy ()
    {
        throw new NotImplementedException ("Stations cannot be destroyed as they're regarded as static data!");
    }
}