using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public MediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "media", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Archivo no encontrado.");

            var mimeType = MimeMapping.MimeUtility.GetMimeMapping(fileName);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, mimeType, fileName);
        }
    }
}
