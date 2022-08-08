using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.EVE.Data.Configuration;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Configuration;

/// <summary>
/// List of constants for the EVE Server
/// </summary>
public class Constants : IConstants
{
    private readonly Dictionary <string, Constant> mConstants;

    public Constant this [string name] => this.mConstants [name];

    public Constants (IDatabaseConnection Database)
    {
        this.mConstants = Database.EveLoadConstants ();
    }
}