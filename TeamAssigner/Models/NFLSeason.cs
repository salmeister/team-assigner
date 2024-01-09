namespace TeamAssigner.Models
{
    public sealed class NFLSeason
    {
        public string id { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string abbreviation { get; set; }
        public int year { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public bool hasGroups { get; set; }
        public bool hasStandings { get; set; }
        public bool hasLegs { get; set; }
        public string slug { get; set; }
    }
}
