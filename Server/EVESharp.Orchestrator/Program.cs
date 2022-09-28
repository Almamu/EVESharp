using EVESharp.Common.Configuration;
using EVESharp.Database;
using EVESharp.Orchestrator.Providers;
using EVESharp.Orchestrator.Repositories;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
                     .ReadFrom.Configuration (builder.Configuration)
                     .Enrich.FromLogContext ()
                     .CreateLogger ();

builder.Host.UseSerilog ();

// add controllers
builder.Services.AddControllers ();
builder.Services.AddSingleton (Log.Logger);
builder.Services.AddSingleton <Database, DatabaseConfiguration> ();
builder.Services.AddSingleton <IStartupInfoProvider, StartupInfoProvider> ();
builder.Services.AddSingleton <IDatabaseConnection, DatabaseConnection> ();
builder.Services.AddSingleton <IClusterRepository, ClusterRepository> ();

WebApplication app = builder.Build ();

app.UseSerilogRequestLogging ();
app.UseHttpsRedirection ();
app.UseAuthorization ();
app.MapControllers ();

// force initialization of the base services/repositories
app.Services.GetService <IStartupInfoProvider> ();
app.Services.GetService <IClusterRepository> ();

app.Run ();