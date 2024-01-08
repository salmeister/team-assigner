namespace TeamAssigner.Models
{
    public sealed class AppSettings
    {
        public string Token { get; set; }
        public string ADOBaseURL { get; set; }
        public string VariableGroupIDToModify { get; set; }
        public string NFLSeason { get; set; }
        public string WeekOverride { get; set; }
    }
}
