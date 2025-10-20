namespace TabTogetherApi.Models
{
    public class ReceiptDto
{
    public DateTimeOffset? TransactionDate { get; set; }
    public List<ReceiptItemDto> Items { get; set; }
    public double Total { get; set; }
}
}
