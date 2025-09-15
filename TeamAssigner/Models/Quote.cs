namespace TeamAssigner.Models
{
    public sealed class Quote
    {
        public string text { get; set; } = string.Empty;
        public string author { get; set; } = string.Empty;
        public string? source { get; set; }
        public string? tags { get; set; }
    }
}
