using System.Xml;
using EVESharp.API.Formatters;
using EVESharp.ASP.Common.Providers;
using EVESharp.Database;
using EVESharp.Database.Configuration;
using Serilog;
using Constants = EVESharp.EVE.Configuration.Constants;
using Database = EVESharp.Database.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder (args);

Log.Logger = new LoggerConfiguration ()
             .ReadFrom.Configuration (builder.Configuration)
             .Enrich.FromLogContext ()
             .CreateLogger ();

builder.Host.UseSerilog ();

// add controllers
builder.Services.AddControllers (options => {
    // ensure only XML serializer is available
    options.OutputFormatters.Clear ();
    
    // make sure the xml formatters include the proper headers on the answers, otherwise <?xml won't be there
    options.OutputFormatters.Insert (0, new CustomXmlFormatter(new XmlWriterSettings ()
    {
        OmitXmlDeclaration = false,
        ConformanceLevel = ConformanceLevel.Document,
    }));
});
builder.Services.AddSingleton (Log.Logger);
builder.Services.AddSingleton <EVESharp.Common.Configuration.Database, DatabaseConfiguration> ();
builder.Services.AddSingleton <IDatabase, Database> ();
builder.Services.AddSingleton <IConstants, Constants> ();

WebApplication app = builder.Build ();

app.UseSerilogRequestLogging ();
app.MapControllers ();

app.Run ();