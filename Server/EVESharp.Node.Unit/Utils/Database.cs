using System.Data;
using EVESharp.Database;
using Moq;

namespace EVESharp.Node.Unit.Utils;

public static class Database
{
    delegate void GetLockCallback (ref IDbConnection connection, string _);
    
    /// <summary>
    /// Sets up a mock for database locking
    /// </summary>
    /// <returns></returns>
    public static Mock <IDatabaseConnection> DatabaseLockMocked ()
    {
        Mock <IDatabaseConnection> databaseMock   = new Mock <IDatabaseConnection> ();
        Mock <IDbConnection>       connectionMock = new Mock <IDbConnection> ();
        
        databaseMock
            .Setup (
                x => x.GetLock (
                    ref It.Ref <IDbConnection>.IsAny,
                    It.IsAny <string> ()
                )
            )
            .Callback (new GetLockCallback((ref IDbConnection db, string _) => db = connectionMock.Object))
            .Verifiable();

        databaseMock
            .Setup (
                x => x.ReleaseLock (
                    It.IsAny <IDbConnection> (),
                    It.IsAny <string> ()
                )
            )
            .Verifiable();

        return databaseMock;
    }
}