using EVESharp.Common.Configuration;
using EVESharp.Database;
using EVESharp.Orchestrator.Providers;
using EVESharp.Orchestrator.Repositories;
using Serilog;
using ILogger = Serilog.ILogger;

WebApplicationBuilder builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
                     .ReadFrom.Configuration (builder.Configuration)
                     .Enrich.FromLogContext ()
                     .CreateLogger ();

builder.Host.UseSerilog ();

IConfigurationSection databaseSettings = builder.Configuration.GetSection ("Database");

if (databaseSettings is null)
    throw new InvalidDataException ("Please specify database connection settings. Check your appsettings.json and ASP.NET documentation");
    
// create a database configuration instance for the DatabaseConnection class
Database config = new Database ()
{
    Hostname = databaseSettings ["hostname"],
    Name = databaseSettings ["database"],
    Username = databaseSettings ["username"],
    Password = databaseSettings ["password"],
    Port = uint.Parse (databaseSettings ["port"])
};

// add controllers
builder.Services.AddControllers ();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();
builder.Services.AddSingleton <ILogger> (Log.Logger);
builder.Services.AddSingleton <IStartupInfoProvider> (new StartupInfoProvider () {Time = DateTime.Now});
builder.Services.AddSingleton (config);
builder.Services.AddSingleton <IDatabaseConnection, DatabaseConnection> ();
builder.Services.AddSingleton <IClusterRepository, ClusterRepository> ();

WebApplication app = builder.Build ();

app.UseSerilogRequestLogging ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ())
{
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();
app.UseAuthorization ();
app.MapControllers ();

// forces initialization of the cluster repository (cleanup of data if required)
app.Services.GetService <IClusterRepository> ();

app.Run ();