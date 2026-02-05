using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                await cvsService.UploadCsvAsync(file);
                return Ok(new { message = "Файл успешно загружен и обработан" });
            }
            catch (Exception ex)
            {                
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetResults(
          string? filename = null,
          DateTimeOffset? dateFrom = null,
          DateTimeOffset? dateTo = null,
          double? avgValueFrom = null,
          double? avgValueTo = null,
          double? avgExecutionFrom = null,
          double? avgExecutionTo = null)
        {
            var query = _context.Results.AsQueryable();

            if (!string.IsNullOrEmpty(filename))
                query = query.Where(r => r.Filename == filename);
            if (dateFrom.HasValue)
                query = query.Where(r => r.MinDate >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(r => r.MinDate <= dateTo.Value);
            if (avgValueFrom.HasValue)
                query = query.Where(r => r.AvgValue >= avgValueFrom.Value);
            if (avgValueTo.HasValue)
                query = query.Where(r => r.AvgValue <= avgValueTo.Value);
            if (avgExecutionFrom.HasValue)
                query = query.Where(r => r.AvgExecutionTime >= avgExecutionFrom.Value);
            if (avgExecutionTo.HasValue)
                query = query.Where(r => r.AvgExecutionTime <= avgExecutionTo.Value);

            var results = await query.OrderByDescending(r => r.MinDate).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{filename}/latest")]
        public async Task<IActionResult> GetLatestValues(string filename)
        {
            var latest = await _context.Values
                .Where(v => v.Filename == filename)
                .OrderByDescending(v => v.Date)
                .Take(10)
                .ToListAsync();

            if (!latest.Any())
            {
                return NotFound(new { error = $"Данные для файла '{filename}' не найдены" });
            }

            var sorted = latest.OrderBy(v => v.Date).ToList();
            return Ok(sorted);
        }
    }
}
