using System;

namespace FinanceMaker.Common.Models.Ideas.IdeaOutputs;

public class GeneralOutputIdea
{
    public string Description { get; set; }

    public GeneralOutputIdea(string description)
    {
        Description = description;
    }
}
