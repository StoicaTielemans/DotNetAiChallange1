using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TabTogetherApi.Services;

namespace TabTogetherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IStorageAccountBlobRepository _blobRepo;

        public ImageController(IStorageAccountBlobRepository blobRepo)
        {
            _blobRepo = blobRepo;
        }

        /// <summary>
        /// Upload an image to blob storage. Returns the public blob URL and generated fileName.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var ext = Path.GetExtension(file.FileName) ?? string.Empty;
                var fileName = $"{Guid.NewGuid()}{ext}";

                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                var url = await _blobRepo.UploadImageAsync(ms, fileName, file.ContentType ?? "application/octet-stream");
                return Ok(new { fileName, url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        /// <summary>
        /// Download an image from blob storage by fileName. Returns the file stream and content-type.
        /// </summary>
        [HttpGet("{fileName}")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Download([FromRoute] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("fileName is required.");

            try
            {
                var result = await _blobRepo.GetImageAsync(fileName);
                if (result == null) return NotFound();

                var (stream, contentType) = result.Value;
                stream.Position = 0;
                return File(stream, contentType ?? "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }
    }
}
