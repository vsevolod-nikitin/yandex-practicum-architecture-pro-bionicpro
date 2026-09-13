using System.Text.Json.Serialization;

namespace BionicPro.Reports.Models
{
    public class CustomerSummaryReport
    {
        [JsonPropertyName("customer_id")]
        public long CustomerId { get; set; }

        [JsonPropertyName("total_orders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("total_spent")]
        public decimal TotalSpent { get; set; }

        [JsonPropertyName("total_discount")]
        public decimal TotalDiscount { get; set; }

        [JsonPropertyName("avg_sensor_value")]
        public decimal AvgSensorValue { get; set; }

        [JsonPropertyName("max_power")]
        public decimal MaxPower { get; set; }
    }
}
