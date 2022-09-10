using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace EVESharp.Database;

/// <summary>
/// Custom DbDataReader implementation used to properly dispose of data readers and connections
/// in a single statement
/// </summary>
public class WrappedDbDataReader : DbDataReader
{
    private readonly DbDataReader  mReader;
    private readonly IDbConnection mConnection;

    public WrappedDbDataReader (DbDataReader reader, IDbConnection connection)
    {
        this.mConnection = connection;
        this.mReader     = reader;
    }

    public override void Close ()
    {
        this.mReader.Close ();
    }

    protected override void Dispose (bool disposing)
    {
        // dispose the reader
        base.Dispose (disposing);
        
        this.mConnection.Dispose ();
    }

    public override bool     GetBoolean (int      ordinal)
    {
        return this.mReader.GetBoolean (ordinal);
    }

    public override byte     GetByte (int         ordinal)
    {
        return this.mReader.GetByte (ordinal);
    }

    public override long     GetBytes (int        ordinal, long dataOffset, byte [] buffer, int bufferOffset, int length)
    {
        return this.mReader.GetBytes (ordinal, dataOffset, buffer, bufferOffset, length);
    }

    public override char     GetChar (int         ordinal)
    {
        return this.mReader.GetChar (ordinal);
    }

    public override long     GetChars (int        ordinal, long dataOffset, char [] buffer, int bufferOffset, int length)
    {
        return this.mReader.GetChars (ordinal, dataOffset, buffer, bufferOffset, length);
    }

    public override string   GetDataTypeName (int ordinal)
    {
        return this.mReader.GetDataTypeName (ordinal);
    }

    public override DateTime GetDateTime (int     ordinal)
    {
        return this.mReader.GetDateTime (ordinal);
    }

    public override decimal GetDecimal (int      ordinal)
    {
        return this.mReader.GetDecimal (ordinal);
    }

    public override double GetDouble (int       ordinal)
    {
        return this.mReader.GetDouble (ordinal);
    }

    public override Type   GetFieldType (int    ordinal)
    {
        return this.mReader.GetFieldType (ordinal);
    }

    public override float  GetFloat (int        ordinal)
    {
        return this.mReader.GetFloat (ordinal);
    }

    public override Guid   GetGuid (int         ordinal)
    {
        return this.mReader.GetGuid (ordinal);
    }

    public override short  GetInt16 (int        ordinal)
    {
        return this.mReader.GetInt16 (ordinal);
    }

    public override int    GetInt32 (int        ordinal)
    {
        return this.mReader.GetInt32 (ordinal);
    }

    public override long   GetInt64 (int        ordinal)
    {
        return this.mReader.GetInt64 (ordinal);
    }

    public override string GetName (int         ordinal)
    {
        return this.mReader.GetName (ordinal);
    }

    public override int    GetOrdinal (string   name)
    {
        return this.mReader.GetOrdinal (name);
    }

    public override string GetString (int       ordinal)
    {
        return this.mReader.GetString (ordinal);
    }

    public override object GetValue (int        ordinal)
    {
        return this.mReader.GetValue (ordinal);
    }

    public override int  GetValues (object [] values)
    {
        return this.mReader.GetValues (values);
    }

    public override bool IsDBNull (int ordinal)
    {
        return this.mReader.IsDBNull (ordinal);
    }

    public override int FieldCount => this.mReader.FieldCount;
    public override object this [int    ordinal] => this.mReader [ordinal];
    public override object this [string name] => this.mReader [name];
    public override int  RecordsAffected => this.mReader.RecordsAffected;
    public override bool HasRows         => this.mReader.HasRows;
    public override bool IsClosed        => this.mReader.IsClosed;

    public override bool NextResult ()
    {
        return this.mReader.NextResult ();
    }

    public override bool Read ()
    {
        return this.mReader.Read ();
    }

    public override int Depth => this.mReader.Depth;

    public override IEnumerator GetEnumerator ()
    {
        return this.mReader.GetEnumerator ();
    }
}