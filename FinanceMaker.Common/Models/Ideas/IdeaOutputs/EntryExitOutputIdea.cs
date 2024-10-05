using System;
using FinanceMaker.Common.Models.Ideas.Enums;

namespace FinanceMaker.Common.Models.Ideas.IdeaOutputs;

public class EntryExitOutputIdea : GeneralOutputIdea
{
    public float Entry { get; set; }
    public float Exit { get; set; }
    public float Stoploss { get; set; }

    public IdeaTradeType Tragde => Exit > Entry ? IdeaTradeType.Long : IdeaTradeType.Short;
    public EntryExitOutputIdea(string description,
                               string ticker,
                               float entry,
                               float exit,
                               float stoploss) : base(description, ticker)
    {
        Entry = entry;
        Exit = exit;
        Stoploss = stoploss;
    }

    
}
