namespace TeamAssigner.Models
{

    public sealed class NFLTeam
    {
        public string id { get; set; }
        public string guid { get; set; }
        public string uid { get; set; }
        public string slug { get; set; }
        public string location { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string abbreviation { get; set; }
        public string displayName { get; set; }
        public string shortDisplayName { get; set; }
        public string color { get; set; }
        public string alternateColor { get; set; }
        public bool isActive { get; set; }
        public bool isAllStar { get; set; }
    }
}
