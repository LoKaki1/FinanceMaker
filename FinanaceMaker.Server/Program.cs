using FinanaceMaker.Server.Middlewares;
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
services.AddSingleton<FinvizTickersPuller>();
services.AddSingleton(sp => new IParamtizedTickersPuller[]
{
    sp.GetService<FinvizTickersPuller>()
});
services.AddSingleton(sp => Array.Empty<ITickerPuller>());
services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
services.AddSingleton<MainTickersPuller>();

services.AddSingleton<YahooPricesPuller>();
services.AddSingleton<YahooInterdayPricesPuller>();
services.AddSingleton(sp => new IPricesPuller[]
{
    sp.GetService<YahooPricesPuller>(),
    sp.GetService<YahooInterdayPricesPuller>(),

});
services.AddSingleton<MainPricesPuller>();
services.AddSingleton<GoogleNewsPuller>();
services.AddSingleton(sp => new INewsPuller[]
{
    sp.GetService<GoogleNewsPuller>()
});
services.AddSingleton<MainNewsPuller>();
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
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

