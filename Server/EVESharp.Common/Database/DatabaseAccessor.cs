namespace EVESharp.Common.Database;

public abstract class DatabaseAccessor
{
    protected DatabaseConnection Database { get; init; }

    protected DatabaseAccessor(DatabaseConnection db)
    {
        this.Database = db;
    }
}