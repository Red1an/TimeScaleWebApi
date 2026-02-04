using Microsoft.AspNetCore.Mvc;
using TimeScaleWebApi.Data;
using TimeScaleWebApi.Services;

namespace TimeScaleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly ApplicationContext _context;
        public DataController(ApplicationContext context) => _context = context;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен");

            try
            {
                var cvsService = new CsvUploadService(_context);
                await cvsService?.UploadCsvAsync(file);
                return Ok(new { message = "Файл успешно загружен и обработан" });
            }
            catch (Exception ex)
            {                
                return BadRequest(new { error = ex.Message });
            }
        }       

    }
}
