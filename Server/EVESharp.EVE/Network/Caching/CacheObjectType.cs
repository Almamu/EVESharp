namespace EVESharp.Node.Cache;

/// <summary>
/// Different types of cache objects, used on the Load mechanism that allow you to specify a SQL query
/// Simplifies the flow of fetching and storing the data
/// </summary>
public enum CacheObjectType
{
    TupleSet      = 0,
    Rowset        = 1,
    CRowset       = 2,
    PackedRowList = 3,
    IntIntDict    = 4,
    IndexRowset   = 5
}