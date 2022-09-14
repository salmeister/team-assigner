namespace TeamAssigner.Models
{
    public sealed class AppSettings
    {
        public string Token { get; set; }
        public string ADOBaseURL { get; set; }
        public string VariableGroupIDToModify { get; set; }
        public int WeekOffset { get; set; }
    }
}
