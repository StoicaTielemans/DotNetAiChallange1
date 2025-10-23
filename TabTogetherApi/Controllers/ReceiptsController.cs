using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.DocumentIntelligence;
using TabTogetherApi.Models;
using TabTogetherApi.Services;
using TabTogetherApi.Entities;
using AutoMapper;

namespace TabTogetherApi.Controllers
{
    public class AnalyzePathRequest
    {
        public string ImagePath { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IDocumentItelligenceRepository _documentService;
        private readonly IMapper _mapper;

        public ReceiptsController(IMapper mapper, IDocumentItelligenceRepository documentService)
        {
            _documentService = documentService;
            _mapper = mapper;
        }

        [HttpGet("items-from-path")]
        [ProducesResponseType(typeof(List<ReceiptItemDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetItemsFromPath([FromQuery] string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) 
                return BadRequest("imagePath query parameter is required.");

            try
            {
                var items = await _documentService.GetItemsAsync(imagePath);
                var dtoItems = _mapper.Map<List<ReceiptItemDto>>(items ?? new List<ReceiptItem>());
                return Ok(dtoItems);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, $"Azure Document Intelligence API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        // to test
        //curl.exe -v -k -X POST "https://localhost:7122/api/Receipts/upload" -H "accept: application/json" -F "file=@C:.\r.jpg;type=image/jpeg"

        [ProducesResponseType(typeof(List<ReceiptItemDto>), 200)]
        [ProducesResponseType(400)]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GetItemsFromUpload( IFormFile file)
        {
            if (file == null || file.Length == 0) {

            return BadRequest("No file uploaded.");
        }

            try
            {
                var items = await _documentService.GetItemsFromUploadAsync(file);
                var dtoItems = _mapper.Map<List<ReceiptItemDto>>(items ?? new List<ReceiptItem>());
                return Ok(dtoItems);
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status, $"Azure Document Intelligence API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}