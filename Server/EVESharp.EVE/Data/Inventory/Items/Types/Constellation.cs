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

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Constellation : ItemInventory
{
    public     Information.Constellation ConstellationInformation { get; }
    public     int                       RegionId                 => this.ConstellationInformation.RegionId;
    public new double                    X                        => this.ConstellationInformation.X;
    public new double                    Y                        => this.ConstellationInformation.Y;
    public new double                    Z                        => this.ConstellationInformation.Z;
    public     double                    XMin                     => this.ConstellationInformation.XMin;
    public     double                    YMin                     => this.ConstellationInformation.YMin;
    public     double                    ZMin                     => this.ConstellationInformation.ZMin;
    public     double                    XMax                     => this.ConstellationInformation.XMax;
    public     double                    YMax                     => this.ConstellationInformation.YMax;
    public     double                    ZMax                     => this.ConstellationInformation.ZMax;
    public     int?                      FactionId                => this.ConstellationInformation.FactionId;
    public     double                    Radius                   => this.ConstellationInformation.Radius;

    public Constellation (Information.Constellation constellation) : base (constellation.Information)
    {
        this.ConstellationInformation = constellation;
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