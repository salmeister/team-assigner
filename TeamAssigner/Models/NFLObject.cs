
namespace TeamAssigner.Models
{
    public sealed class NFLObject
    {
        public int count { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int pageCount { get; set; }
        public List<NFLRef> items { get; set; }
    }
}
