using MySql.Data.MySqlClient;

namespace EVESharp.Orchestrator.Models;

public class Database
{
    public string ConnectionString { get; init; }

    public Database (string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public MySqlConnection Get ()
    {
        MySqlConnection connection = new MySqlConnection (this.ConnectionString);
        connection.Open ();

        return connection;
    }
}