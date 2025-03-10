using System;
using System.Collections.Generic;
using FinanceMaker.Algorithms;
using FinanceMaker.Algorithms.News.Analyziers;
using FinanceMaker.Algorithms.News.Analyziers.Interfaces;
using FinanceMaker.Common;
using FinanceMaker.Common.Models.Ideas.IdeaInputs;
using FinanceMaker.Common.Models.Ideas.IdeaOutputs;
using FinanceMaker.Ideas.Ideas;
using FinanceMaker.Ideas.Ideas.Abstracts;
using FinanceMaker.Publisher.Orders.Trader;
using FinanceMaker.Publisher.Orders.Trader.Interfaces;
using FinanceMaker.Publisher.Traders;
using FinanceMaker.Publisher.Traders.Interfaces;
using FinanceMaker.Pullers;
using FinanceMaker.Pullers.NewsPullers;
using FinanceMaker.Pullers.NewsPullers.Interfaces;
using FinanceMaker.Pullers.PricesPullers;
using FinanceMaker.Pullers.PricesPullers.Interfaces;
using FinanceMaker.Pullers.TickerPullers;
using FinanceMaker.Pullers.TickerPullers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var app = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
                // Can't use the extension (the service collection becomes read only after the build)
                services.AddSingleton<MarketStatus>();
                services.AddSingleton<FinvizTickersPuller>();
                services.AddSingleton<TradingViewTickersPuller>();
                services.AddSingleton(sp => new IParamtizedTickersPuller[]
                {
                    sp.GetService<FinvizTickersPuller>(),
                    sp.GetService<TradingViewTickersPuller>()
                });
                services.AddSingleton(sp => Array.Empty<IRelatedTickersPuller>());
                services.AddSingleton(sp => Array.Empty<ITickerPuller>());
                services.AddSingleton<MainTickersPuller>();

                services.AddSingleton<YahooPricesPuller>();
                services.AddSingleton<YahooInterdayPricesPuller>();

                services.AddSingleton(sp => new IPricesPuller[]
                {
                    // sp.GetService<YahooPricesPuller>(),
                    sp.GetService<YahooInterdayPricesPuller>(),

                });
                services.AddSingleton<IPricesPuller, MainPricesPuller>();
                services.AddSingleton<GoogleNewsPuller>();
                services.AddSingleton<YahooFinanceNewsPuller>();
                services.AddSingleton(sp => new INewsPuller[]
                {
                    sp.GetService<GoogleNewsPuller>(),
                    sp.GetService<YahooFinanceNewsPuller>(),
                    sp.GetService<FinvizNewPuller>(),

                });

                services.AddSingleton<KeyLevelsRunner>();
                services.AddSingleton<EMARunner>();
                services.AddSingleton<BreakOutDetectionRunner>();

                services.AddSingleton<IEnumerable<IAlgorithmRunner<RangeAlgorithmInput>>>(
                    sp =>
                    {
                        var runner3 = sp.GetService<KeyLevelsRunner>();
                        var runner1 = sp.GetService<EMARunner>();
                        var runner2 = sp.GetService<BreakOutDetectionRunner>();


                        return [runner1, runner2, runner3];
                    }
                );

                services.AddSingleton<RangeAlgorithmsRunner>();
                services.AddSingleton<INewsPuller, MainNewsPuller>();
                services.AddSingleton<KeywordsDetectorAnalysed>();
                services.AddSingleton<INewsAnalyzer[]>(sp => [
                    sp.GetService<KeywordsDetectorAnalysed>()
                ]);
                services.AddSingleton<INewsAnalyzer, NewsAnalyzer>();
                services.AddSingleton<IdeaBase<TechnicalIdeaInput, EntryExitOutputIdea>, OverNightBreakout>();
                services.AddSingleton<OverNightBreakout>();
                services.AddSingleton<IBroker, AlpacaBroker>();
                services.AddSingleton<ITrader, QCTrader>();
                services.AddHostedService<Worker>();


            })
            .Build();
app.Run();
