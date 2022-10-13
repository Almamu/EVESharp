using System.Data;
using EVESharp.Database;
using Moq;

namespace EVESharp.Node.Unit.Utils;

public static class Database
{
    delegate DbLock GetLockCallback (string name);
    
    /// <summary>
    /// Sets up a mock for database locking
    /// </summary>
    /// <returns></returns>
    public static Mock <IDatabase> DatabaseLockMocked ()
    {
        Mock <IDatabase> databaseMock = new Mock <IDatabase> ();
        
        databaseMock
            .Setup (
                x => x.GetLock (
                    It.IsAny <string> ()
                )
            )
            .Returns ((string name) => new DbLock () { Connection = DbConnectionMocked ().Object, Creator = databaseMock.Object, Name = name})
            .Verifiable();

        databaseMock
            .Setup (
                x => x.ReleaseLock (
                    It.IsAny <DbLock> ()
                )
            )
            .Verifiable();

        return databaseMock;
    }

    private static Mock <IDbConnection> DbConnectionMocked ()
    {
        Mock <IDbConnection> connectionMock = new Mock <IDbConnection> ();
        
        // setup connection state
        connectionMock
            .SetupGet (x => x.State)
            .Returns (ConnectionState.Open);
        
        return connectionMock;
    }
}