using System;
using EVESharp.Common.Configuration.Attributes;
using EVESharp.EVE.Data.Account;

namespace EVESharp.EVE.Configuration.Attributes;

public class RoleListConfigValue : TransformConfigValue
{
    public RoleListConfigValue (string name, bool optional = false) : base (name, optional, (value) =>
    {
        long      result   = 0;
        string [] rolelist = value.Split (",");

        foreach (string role in rolelist)
        {
            string trimedRole = role.Trim ();

            // ignore empty roles
            if (trimedRole == "")
                continue;

            if (Enum.TryParse (trimedRole, out Roles roleValue) == false)
                throw new Exception ($"Unknown role value {role.Trim ()}");

            result |= (long) roleValue;
        }

        return result;
    })
    {
        
    }
}