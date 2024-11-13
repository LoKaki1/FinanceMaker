namespace FinanceMaker.Common.Models.Trades.Enums;

[Flags]
public enum TraderType
{
    EntryExit = 0,
    ProfitStopLoss = 1,
    Market = 2,
    Dynamic = 4
}
