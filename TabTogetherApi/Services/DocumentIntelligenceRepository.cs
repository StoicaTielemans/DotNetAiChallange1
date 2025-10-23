namespace TabTogetherApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.AI.DocumentIntelligence;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Http;
    using TabTogetherApi.Configuration;
    using TabTogetherApi.Entities;

    public class DocumentItelligenceRepository : IDocumentItelligenceRepository
    {
        private readonly DocumentIntelligenceClient _client;

        // Accept IOptions<T> so DI will pass configured settings from Program.cs
        public DocumentItelligenceRepository(IOptions<AzureDocumentIntelligenceSettings> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var settings = options.Value ?? throw new InvalidOperationException("AzureDocumentIntelligence settings are not configured.");

            if (string.IsNullOrWhiteSpace(settings.Endpoint) || string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException("AzureDocumentIntelligence settings must contain Endpoint and ApiKey.");
            }

            var credential = new AzureKeyCredential(settings.ApiKey);
            _client = new DocumentIntelligenceClient(new Uri(settings.Endpoint), credential);
        }

        // Raw analysis - returns full Azure result
        public async Task<AnalyzeResult> AnalyzeReceiptAsync(string imagePath)
        {
            using var stream = File.OpenRead(imagePath);
            var content = BinaryData.FromStream(stream);
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-receipt", content);
            return operation.Value;
        }

        public async Task<AnalyzeResult> AnalyzeReceiptFromUrlAsync(Uri imageUrl)
        {
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-receipt", imageUrl);
            return operation.Value;
        }

        // Get structured receipt data
        public async Task<Receipt> GetReceiptDataAsync(string imagePath)
        {
            var result = await AnalyzeReceiptAsync(imagePath);
            return ParseReceiptData(result);
        }

        public async Task<Receipt> GetReceiptDataFromUrlAsync(Uri imageUrl)
        {
            var result = await AnalyzeReceiptFromUrlAsync(imageUrl);
            return ParseReceiptData(result);
        }

        // Get just items
        public async Task<List<ReceiptItem>> GetItemsAsync(string imagePath)
        {
            var result = await AnalyzeReceiptAsync(imagePath);
            return ExtractItems(result.Documents.FirstOrDefault());
        }

        // Get just items from uploaded file
        public async Task<List<ReceiptItem>> GetItemsFromUploadAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            using var stream = file.OpenReadStream();
            var content = BinaryData.FromStream(stream);
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-receipt", content);
            var result = operation.Value;
            return ExtractItems(result.Documents.FirstOrDefault());
        }

        // Get just total
        public async Task<double> GetTotalAsync(string imagePath)
        {
            var result = await AnalyzeReceiptAsync(imagePath);
            var receipt = result.Documents.FirstOrDefault();

            if (receipt?.Fields.TryGetValue("Total", out DocumentField field) == true)
            {
                if (field.FieldType == DocumentFieldType.Currency)
                {
                    return field.ValueCurrency?.Amount ?? 0;
                }
            }
            return 0;
        }

        // Helper methods - These remain private as they are implementation details
        private Receipt ParseReceiptData(AnalyzeResult result)
        {
            var data = new Receipt();
            var receipt = result.Documents.FirstOrDefault();

            if (receipt == null) return data;

            if (receipt.Fields.TryGetValue("TransactionDate", out DocumentField dateField))
            {
                data.TransactionDate = dateField.ValueDate;
            }

            if (receipt.Fields.TryGetValue("Total", out DocumentField totalField))
            {
                if (totalField.FieldType == DocumentFieldType.Currency)
                {
                    data.Total = totalField.ValueCurrency?.Amount ?? 0;
                }
            }

            data.Items = ExtractItems(receipt);
            return data;
        }

        private List<ReceiptItem> ExtractItems(AnalyzedDocument? receipt)
        {
            var items = new List<ReceiptItem>();

            if (receipt?.Fields.TryGetValue("Items", out DocumentField itemsField) == true)
            {
                if (itemsField.FieldType == DocumentFieldType.List)
                {
                    foreach (DocumentField itemField in itemsField.ValueList)
                    {
                        if (itemField.FieldType == DocumentFieldType.Dictionary)
                        {
                            var itemData = new ReceiptItem();
                            var itemFields = itemField.ValueDictionary;

                            if (itemFields.TryGetValue("Description", out DocumentField descField))
                            {
                                itemData.Description = descField.ValueString ?? string.Empty;
                                itemData.Confidence = descField.Confidence ?? 0;
                            }

                            if (itemFields.TryGetValue("Quantity", out DocumentField quantityField))
                            {
                                itemData.Quantity = quantityField.ValueDouble;
                            }

                            if (itemFields.TryGetValue("TotalPrice", out DocumentField priceField))
                            {
                                if (priceField.FieldType == DocumentFieldType.Currency)
                                {
                                    itemData.TotalPrice = priceField.ValueCurrency?.Amount;
                                }
                            }

                            items.Add(itemData);
                        }
                    }
                }
            }

            return items;
        }
    }
}