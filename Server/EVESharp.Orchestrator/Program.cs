using EVESharp.Orchestator.Models;
using EVESharp.Orchestator.Providers;
using MySql.Data.MySqlClient;

WebApplicationBuilder builder = WebApplication.CreateBuilder (args);

// Add services to the container.

Database db = new Database (builder.Configuration.GetConnectionString ("DefaultConnection"));

builder.Services.AddControllers ();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();
builder.Services.AddSingleton (db);
builder.Services.AddSingleton (new StartupInfoProvider () {Time = DateTime.Now});

// check for the settings and restart things if needed
bool restartOnStartup = bool.Parse (builder.Configuration.GetSection ("Cluster") ["ResetOnStartup"]);

if (restartOnStartup)
    // set things to zero
    using (MySqlConnection connection = db.Get ())
    {
        MySqlCommand items    = new MySqlCommand ("UPDATE invItems SET nodeID = 0;",     connection);
        MySqlCommand nodes    = new MySqlCommand ("DELETE FROM cluster;",                connection);
        MySqlCommand accounts = new MySqlCommand ("UPDATE account SET proxyNodeID = 0;", connection);

        nodes.ExecuteNonQuery ();
        items.ExecuteNonQuery ();
        accounts.ExecuteNonQuery ();
    }

WebApplication app = builder.Build ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ())
{
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();

app.UseAuthorization ();

app.MapControllers ();

app.Run ();