using Statesman.BillScraper;
using Statesman.BillScraper.Data;
using Statesman.BillScraper.Mappings;
using Statesman.BillScraper.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient("parliamentClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Statesman:ParliamentBaseUrl")!);
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSingleton<BillClientService>();

await builder.Services.AddDataLayer(builder.Configuration, true);

var host = builder.Build();

await host.RunNeo4jMigrations();

host.Run();