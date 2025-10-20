namespace TabTogetherApi.Models
{
    public class ReceiptItemDto
{
    public string Description { get; set; }
    public double? Quantity { get; set; }
    public double? TotalPrice { get; set; }
    public double Confidence { get; set; }
    }
}
