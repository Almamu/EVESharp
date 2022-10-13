using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Configuration;
using EVESharp.Database.Extensions;

namespace EVESharp.EVE.Configuration;

/// <summary>
/// List of constants for the EVE Server
/// </summary>
public class Constants : IConstants
{
    private readonly Dictionary <string, Constant> mConstants;

    public Constant this [string name] => this.mConstants [name];

    public Constants (IDatabase Database)
    {
        this.mConstants = Database.EveLoadConstants ();
    }
}