namespace TimeScaleWebApi.Models
{
    public class Values
    {
        int Id { get; set; }
        public string Filename { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public double ExecutionTime { get; set; }
        public double Value { get; set; }
    }
}
