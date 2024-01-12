namespace TeamAssigner.Models
{
    public sealed class AppSettings
    {
        public string BaseAPIURL { get; set; }
        public string KeyFileName { get; set; }
        public string SheetID { get; set; }
        public string SheetRange { get; set; }
        public string WeekOverride { get; set; }
    }
}
