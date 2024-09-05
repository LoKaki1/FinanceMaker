using FinanaceMaker.Server;
using FinanaceMaker.Server.Middlewares;
using FinanceMaker.Algorithms;
using FinanceMaker.Common;
using FinanceMaker.Pullers;
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var services = builder.Services;
services.AddHttpClient();
services.AddSingleton(sp => new IParamtizedTickersPuller[]
{
    sp.AddAndGetService<FinvizTickersPuller>(services)
});
services.AddSingleton(sp => Array.Empty<ITickerPuller>());
services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
services.AddSingleton<MainTickersPuller>();

services.AddSingleton(sp => new IPricesPuller[]
{
    sp.AddAndGetService<YahooPricesPuller>(services),
    sp.AddAndGetService<YahooInterdayPricesPuller>(services),

});
services.AddSingleton<MainPricesPuller>();
services.AddSingleton(sp => new INewsPuller[]
{
    sp.AddAndGetService<GoogleNewsPuller>(services),
    sp.AddAndGetService<YahooFinanceNewsPuller>(services),

});

services.AddSingleton<IEnumerable<IAlgorithmRunner<RangeAlgorithmInput, object>>>(
    sp => 
    [
        sp.AddAndGetService<EMARunner>(services),
        sp.AddAndGetService<BreakOutDetectionRunner>(services)
    ]
);

services.AddSingleton<RangeAlgorithmsRunner>();
services.AddSingleton<MainNewsPuller>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                      });
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<CorsPolicyHandler>();
app.UseAuthorization();

app.MapControllers();

app.Run();

