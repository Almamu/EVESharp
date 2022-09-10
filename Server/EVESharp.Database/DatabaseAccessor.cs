
namespace EVESharp.Database;

public abstract class DatabaseAccessor
{
    protected IDatabaseConnection Database { get; init; }

    protected DatabaseAccessor (IDatabaseConnection db)
    {
        Database = db;
    }
}