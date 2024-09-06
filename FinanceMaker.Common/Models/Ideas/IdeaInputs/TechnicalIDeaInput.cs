using System;
using FinanceMaker.Common.Models.Pullers;

namespace FinanceMaker.Common.Models.Ideas.IdeaInputs;

public class TechnicalIDeaInput: GeneralInputIdea
{
    public required TickersPullerParameters  TechnicalParams {get; set;}
}
