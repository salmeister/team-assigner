namespace TeamAssigner.Models
{
    public sealed class PlayerInfo
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Filled { get; set; }
    }
}
