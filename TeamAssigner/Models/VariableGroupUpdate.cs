
namespace TeamAssigner.Models
{
    public sealed class VariableGroupUpdate
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public Variables variables { get; set; }
    }
}
