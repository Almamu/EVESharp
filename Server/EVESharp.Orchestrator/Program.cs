using EVESharp.ASP.Common.Providers;
using EVESharp.Common.Configuration;
using EVESharp.Database;
using EVESharp.Orchestrator.Providers;
using EVESharp.Orchestrator.Repositories;
using Serilog;
using Database = EVESharp.Database.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
                     .ReadFrom.Configuration (builder.Configuration)
                     .Enrich.FromLogContext ()
                     .CreateLogger ();

builder.Host.UseSerilog ();

// add controllers
builder.Services.AddControllers ();
builder.Services.AddSingleton (Log.Logger);
builder.Services.AddSingleton <EVESharp.Common.Configuration.Database, DatabaseConfiguration> ();
builder.Services.AddSingleton <IStartupInfoProvider, StartupInfoProvider> ();
builder.Services.AddSingleton <IDatabase, Database> ();
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