using Azure.AI.DocumentIntelligence;
using TabTogetherApi.Entities; // Import the namespace where ReceiptData and ReceiptItem now live

namespace TabTogetherApi.Services
{
    public interface IDocumentItelligenceRepository
    {
        // Methods returning custom structured data
        Task<Receipt> GetReceiptDataAsync(string imagePath);
        Task<Receipt> GetReceiptDataFromUrlAsync(Uri imageUrl);
        Task<List<ReceiptItem>> GetItemsAsync(string imagePath);
        Task<double> GetTotalAsync(string imagePath);

        // Raw analysis methods
        Task<AnalyzeResult> AnalyzeReceiptAsync(string imagePath);
        Task<AnalyzeResult> AnalyzeReceiptFromUrlAsync(Uri imageUrl);
    }
}
