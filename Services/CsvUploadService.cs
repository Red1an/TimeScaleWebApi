using TimeScaleWebApi.Data;
using TimeScaleWebApi.Models;

namespace TimeScaleWebApi.Services
{
    public class CsvUploadService
    {
        private readonly ApplicationContext _context;
        public CsvUploadService(ApplicationContext context) => _context = context;

        public async Task UploadCsvAsync(IFormFile file)
        {
            var filename = Path.GetFileName(file.FileName);
            using var sr = new StreamReader(file.OpenReadStream());
            string line;
            
            var values = new List<TimeValues>();
            while ((line = await sr.ReadLineAsync()) != null)
            {
                var row = line.Split(',');
                var dateStr = row[0];
                var execStr = row[1];
                var valStr = row[2];
                                
                values.Add(new TimeValues
                {   
                    Filename = filename,
                    Date = DateTimeOffset.Parse(dateStr).ToUniversalTime(),
                    ExecutionTime = double.Parse(execStr),
                    Value = double.Parse(valStr)
                });               
                
            }
            await _context.Values.AddRangeAsync(values);
            await _context.SaveChangesAsync();
        }
    }
}
