
namespace EVESharp.Database;

public abstract class DatabaseAccessor
{
    protected IDatabase Database { get; init; }

    protected DatabaseAccessor (IDatabase db)
    {
        Database = db;
    }
}