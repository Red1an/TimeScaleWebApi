using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TimeScaleWebApi.Data;
using TimeScaleWebApi.Models;

namespace TimeScaleWebApi.Services
{
    public class CsvUploadService
    {
        private readonly ApplicationContext _context;
        public CsvUploadService(ApplicationContext context) => _context = context;
        private static readonly DateTimeOffset MinAllowedDate = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);     

        public async Task UploadCsvAsync(IFormFile file)
        {
            var filename = Path.GetFileName(file.FileName);
            string? line;
            int lineNumber = 1;
            var values = new List<TimeValues>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                using var sr = new StreamReader(file.OpenReadStream());
                var header = await sr.ReadLineAsync();
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    var row = line.Split(';');
                    if (row.Length != 3)
                    {
                        throw new Exception($"Строка {lineNumber}: неверное количество столбцов");
                    }

                    var dateStr = row[0];
                    var execStr = row[1];
                    var valStr = row[2];

                    if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(execStr) || string.IsNullOrWhiteSpace(valStr))
                    {
                        throw new Exception($"Строка {lineNumber}: отсутсвует одно из значений");
                    }

                    if (!DateTimeOffset.TryParse(dateStr,
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                    {
                        throw new Exception($"Строка {lineNumber}: неверный формат даты '{dateStr}'");
                    }

                    if (date < MinAllowedDate)
                    {
                        throw new Exception($"Строка {lineNumber}: дата должна быть позже 01.01.2000");
                    }

                    if (date > DateTimeOffset.UtcNow)
                    {
                        throw new Exception($"Строка {lineNumber}: дата не должна быть позже текущей");
                    }

                    if (!double.TryParse(execStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var exec))
                    {
                        throw new Exception($"Строка {lineNumber}: неверный формат времени выполнения '{execStr}'");
                    }

                    if (exec < 0)
                    {
                        throw new Exception($"Строка {lineNumber}: время выполнения меньше нуля");
                    }

                    if (!double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        throw new Exception($"Строка {lineNumber}: неверный формат показателя '{valStr}'");
                    }

                    if (value < 0)
                    {
                        throw new Exception($"Строка {lineNumber}: показатель не может быть меньше нуля");
                    }

                    values.Add(new TimeValues
                    {
                        Filename = filename,
                        Date = date,
                        ExecutionTime = exec,
                        Value = value
                    });

                }
                if (values.Count < 1)
                {
                    throw new Exception("Файл должен содержать минимум 1 запись");
                }

                if (values.Count > 10000)
                {
                    throw new Exception("Файл не может содержать более 10 000 записей");
                }

                await _context.Results.Where(r => r.Filename == filename).ExecuteDeleteAsync();
                await _context.Values.Where(v => v.Filename == filename).ExecuteDeleteAsync();
                await _context.Values.AddRangeAsync(values);
                await _context.SaveChangesAsync();                

                var result = CalculateResults(filename, values);
                _context.Results.Add(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private TimeResults CalculateResults(string filename, List<TimeValues> values)
        {
            var minDate = values.Min(v => v.Date);
            var maxDate = values.Max(v => v.Date);
            var delta = (maxDate - minDate).TotalSeconds;

            var avgExec = values.Average(v => v.ExecutionTime);
            var avgVal = values.Average(v => v.Value);
            var maxVal = values.Max(v => v.Value);
            var minVal = values.Min(v => v.Value);

            var allValues = values.Select(v => v.Value).OrderBy(x => x).ToList();
            var n = allValues.Count;
            var median = n % 2 == 1
                ? allValues[n / 2]
                : (allValues[n / 2 - 1] + allValues[n / 2]) / 2.0;

            return new TimeResults
            {
                Filename = filename,
                DeltaSeconds = delta,
                MinDate = minDate,
                AvgExecutionTime = avgExec,
                AvgValue = avgVal,
                MedianValue = median,
                MaxValue = maxVal,
                MinValue = minVal
            };
        }
    }
}
