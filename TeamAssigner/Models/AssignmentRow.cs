namespace TeamAssigner.Models
{
    public sealed class AssignmentRow
    {
        public required string PlayerName { get; init; }
        public required string Team1 { get; init; }
        public required string Team2 { get; init; }
        public bool Team1IsBye { get; init; }
    }
}
