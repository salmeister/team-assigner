namespace TeamAssigner.Models
{
    public sealed class PlayerInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public bool Filled { get; set; }
    }
}
