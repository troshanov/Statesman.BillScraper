using Statesman.BillScraper;
using Statesman.BillScraper.Data;
using Statesman.BillScraper.Mappings;
using Statesman.BillScraper.Utilities.ImageScanner;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient("parliamentClient", client =>
{
    client.BaseAddress = new Uri("https://www.parliament.bg");
});

var googleCredentials = builder.Configuration["Paths:GoogleCredentials"];

string credentialPath = Path.Combine(AppContext.BaseDirectory, googleCredentials!);
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

builder.Services.AddAutoMapper(typeof(MappingProfile));

await builder.Services.AddDataLayer(builder.Configuration, true);

builder.Services.AddSingleton<IGoogleCloudVisionService, GoogleCloudVisionService>();

var host = builder.Build();

await host.RunNeo4jMigrations();

host.Run();