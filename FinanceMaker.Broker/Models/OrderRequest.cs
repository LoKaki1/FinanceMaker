namespace FinanceMaker.Broker.Models
{
    public class OrderRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
