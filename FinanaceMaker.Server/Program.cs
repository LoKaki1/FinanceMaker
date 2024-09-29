using System.Security.Policy;
using FinanaceMaker.Server;
using FinanaceMaker.Server.Middlewares;
using FinanceMaker.Algorithms;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Finance;
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

services.AddSingleton<IEnumerable<IAlgorithmRunner<RangeAlgorithmInput>>>(
    sp => 
    {
        TickerRangeAlgorithmRunnerBase<EMACandleStick> runner1 = sp.AddAndGetService<EMARunner>(services);
        var runner2 = sp.AddAndGetService<BreakOutDetectionRunner>(services);
        var runner3 = sp.AddAndGetService<KeyLevelsRunner>(services);
        
        
        return [runner1, runner2, runner3];
    }
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

