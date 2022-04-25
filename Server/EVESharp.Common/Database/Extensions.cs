using System.Data.Common;
using MySql.Data.MySqlClient;

namespace EVESharp.Common.Database;

public static class Extensions
{
    public static int? GetInt32OrNull (this DbDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetInt32 (columnIndex);
    }

    public static long? GetInt64OrNull (this DbDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetInt64 (columnIndex);
    }

    public static double? GetDoubleOrNull (this DbDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetDouble (columnIndex);
    }

    public static string GetStringOrNull (this DbDataReader reader, int columnIndex)
    {
        return reader.IsDBNull (columnIndex) ? null : reader.GetString (columnIndex);
    }

    public static int GetInt32OrDefault (this DbDataReader reader, int columnIndex, int defaultValue = 0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetInt32 (columnIndex);
    }

    public static long GetInt64OrDefault (this DbDataReader reader, int columnIndex, long defaultValue = 0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetInt64 (columnIndex);
    }

    public static double GetDoubleOrDefault (this DbDataReader reader, int columnIndex, double defaultValue = 0.0)
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetDouble (columnIndex);
    }

    public static string GetStringOrDefault (this DbDataReader reader, int columnIndex, string defaultValue = "")
    {
        return reader.IsDBNull (columnIndex) ? defaultValue : reader.GetString (columnIndex);
    }
}