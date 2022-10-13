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

using EVESharp.Database.Types;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Types;
using EVESharp.Node.Configuration;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Authentication;

public class authentication : Service
{
    private readonly Configuration.Authentication mConfiguration;
    public override  AccessLevel                  AccessLevel => AccessLevel.None;

    public authentication (Configuration.Authentication configuration)
    {
        this.mConfiguration = configuration;
    }

    public PyDataType GetPostAuthenticationMessage (ServiceCall call)
    {
        return this.mConfiguration.MessageType switch
        {
            AuthenticationMessageType.NoMessage => null,
            AuthenticationMessageType.Message   => KeyVal.FromDictionary (new PyDictionary {["message"] = this.mConfiguration.Message}),
            _                                   => null
        };
    }
}