using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Common.Database;

public abstract class DatabaseAccessor
{
    protected IDatabaseConnection Database { get; init; }

    protected DatabaseAccessor (IDatabaseConnection db)
    {
        Database = db;
    }
}