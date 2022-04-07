using MySql.Data.MySqlClient;

namespace EVESharp.Orchestator.Models;

public class Database
{
    public string ConnectionString { get; init; }

    public Database (string connectionString)
    {
        ConnectionString = connectionString;
    }

    public MySqlConnection Get ()
    {
        MySqlConnection connection = new MySqlConnection (ConnectionString);
        connection.Open ();

        return connection;
    }
}