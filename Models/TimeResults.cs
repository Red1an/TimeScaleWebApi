namespace TimeScaleWebApi.Models
{
    public class TimeResults
    {
        public int Id { get; set; }
        public string Filename { get; set; } = null!;
        public double DeltaSeconds { get; set; }
        public DateTimeOffset MinDate { get; set; }
        public double AvgExecutionTime { get; set; }
        public double AvgValue { get; set; }
        public double MedianValue { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
    }
}
