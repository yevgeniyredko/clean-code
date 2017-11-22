namespace Markdown
{
    public class TagNode : INode
    {
        public static TagNode Opening(Tag tag) => new TagNode(tag, TagNodeType.Opening);
        public static TagNode Closing(Tag tag) => new TagNode(tag, TagNodeType.Closing);

        public Tag Tag { get; }
        public TagNodeType NodeType { get; }

        public TagNode Pair { get; set; }
        public TagNode Parent { get; set; }

        public bool HasPair => Pair != null;
        public bool IsBanned => (Parent?.HasPair ?? false) && Parent.Tag.BannedTags.Contains(Tag);

        private TagNode(Tag tag, TagNodeType nodeType)
        {
            Tag = tag;
            NodeType = nodeType;
        }
    }
}