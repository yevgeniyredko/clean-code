namespace Markdown
{
    public class TagNode : INode
    {
        public static void Connect(TagNode opening, TagNode closing)
        {
            opening.Pair = closing;
            closing.Pair = opening;
            closing.Parent = opening.Parent;
        }

        public static TagNode Opening(Tag tag) => new TagNode(tag, true);
        public static TagNode Closing(Tag tag) => new TagNode(tag, false);

        public Tag Tag { get; }
        public bool IsOpeningTag { get; }

        public TagNode Pair { get; set; }
        public TagNode Parent { get; set; }

        public bool HasPair => Pair != null;
        public bool IsBanned => (Parent?.HasPair ?? false) && Parent.Tag.BannedTags.Contains(Tag);

        private TagNode(Tag tag, bool isOpeningTag)
        {
            Tag = tag;
            IsOpeningTag = isOpeningTag;
        }
    }
}