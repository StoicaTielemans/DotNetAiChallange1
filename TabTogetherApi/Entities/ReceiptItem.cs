namespace TabTogetherApi.Entities
{
    public class ReceiptItem
{
    public string Description { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public double? TotalPrice { get; set; }

    public double Confidence { get; set; }
}
}
