namespace TabTogetherApi.Entities
{
    public class Receipt
{
    public DateTimeOffset? TransactionDate { get; set; }
    public List<ReceiptItem> Items { get; set; } = new();
    public double Total { get; set; }
}
}
