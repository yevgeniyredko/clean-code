using System;
using System.Collections.Generic;

namespace Markdown
{
    public class Tag
    {
        public string Sequence { get; }
        public ICollection<Tag> BannedTags => bannedTags; 
        private readonly HashSet<Tag> bannedTags = new HashSet<Tag>();

        public Tag(string sequence)
        {
            Sequence = sequence;
        }

        public Tag AddBannedTag(Tag tag)
        {
            if (!bannedTags.Add(tag))
                throw new ArgumentException("Tag already exists", nameof(tag));
            return this;
        }
    }
}