
namespace TeamAssigner.Models
{
    public sealed class NFLWeeklyScores
    {
        public List<Event> events { get; set; }

        public class Event
        {
            public string id { get; set; }
            public string uid { get; set; }
            public string date { get; set; }
            public string name { get; set; }
            public string shortName { get; set; }
            public List<Competition> competitions { get; set; }
        }

        public class Competition
        {
            public string id { get; set; }
            public string uid { get; set; }
            public string date { get; set; }
            public int attendance { get; set; }
            public bool timeValid { get; set; }
            public bool neutralSite { get; set; }
            public bool conferenceCompetition { get; set; }
            public bool playByPlayAvailable { get; set; }
            public bool recent { get; set; }
            public List<Competitor> competitors { get; set; }
            public string startDate { get; set; }
            public string broadcast { get; set; }
            public List<Headline> headlines { get; set; }
        }

        public class Competitor
        {
            public string id { get; set; }
            public string uid { get; set; }
            public string type { get; set; }
            public int order { get; set; }
            public string homeAway { get; set; }
            public bool winner { get; set; }
            public Team team { get; set; }
            public string score { get; set; }
        }

        public class Headline
        {
            public string type { get; set; }
            public string description { get; set; }
            public string shortLinkText { get; set; }
        }

        public class Team
        {
            public string id { get; set; }
            public string uid { get; set; }
            public string location { get; set; }
            public string name { get; set; }
            public string abbreviation { get; set; }
            public string displayName { get; set; }
            public string shortDisplayName { get; set; }
            public string color { get; set; }
            public string alternateColor { get; set; }
            public bool isActive { get; set; }
            public string logo { get; set; }
        }

    }
}
