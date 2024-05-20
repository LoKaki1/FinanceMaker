namespace FinanceMaker.Common.Models.Filters
{
    public record ChartFiltersResult
    {
        public IEnumerable<double> SupportLevel { get; set; }
        public IEnumerable<double> ResistanceLevel { get;}
        // Etc
    }
}
