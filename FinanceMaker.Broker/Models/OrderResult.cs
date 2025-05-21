using FinanceMaker.Broker.Models;

namespace FinanceMaker.Broker.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Filled,
        Rejected
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string? OrderId { get; set; }
        public string? ErrorMessage { get; set; }
        public OrderStatus Status { get; set; }
        public decimal? FilledPrice { get; set; }
        public int? FilledQuantity { get; set; }
        public DateTime? FilledTime { get; set; }
    }
}
