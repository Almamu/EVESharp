using MySql.Data.MySqlClient;

namespace EVESharp.Common.Database;

public static class Extensions
{
    public static int? GetInt32OrNull(this MySqlDataReader reader, int columnIndex)
    {
        return reader.IsDBNull(columnIndex) ? null : reader.GetInt32(columnIndex);
    }
        
    public static long? GetInt64OrNull(this MySqlDataReader reader, int columnIndex)
    {
        return reader.IsDBNull(columnIndex) ? null : reader.GetInt64(columnIndex);
    }
        
    public static double? GetDoubleOrNull(this MySqlDataReader reader, int columnIndex)
    {
        return reader.IsDBNull(columnIndex) ? null : reader.GetDouble(columnIndex);
    }
        
    public static string GetStringOrNull(this MySqlDataReader reader, int columnIndex)
    {
        return reader.IsDBNull(columnIndex) ? null : reader.GetString(columnIndex);
    }
        
    public static int GetInt32OrDefault(this MySqlDataReader reader, int columnIndex, int defaultValue = 0)
    {
        return reader.IsDBNull(columnIndex) ? defaultValue : reader.GetInt32(columnIndex);
    }
        
    public static long GetInt64OrDefault(this MySqlDataReader reader, int columnIndex, long defaultValue = 0)
    {
        return reader.IsDBNull(columnIndex) ? defaultValue : reader.GetInt64(columnIndex);
    }
        
    public static double GetDoubleOrDefault(this MySqlDataReader reader, int columnIndex, double defaultValue = 0.0)
    {
        return reader.IsDBNull(columnIndex) ? defaultValue : reader.GetDouble(columnIndex);
    }
        
    public static string GetStringOrDefault(this MySqlDataReader reader, int columnIndex, string defaultValue = "")
    {
        return reader.IsDBNull(columnIndex) ? defaultValue : reader.GetString(columnIndex);
    }
}