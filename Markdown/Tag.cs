using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class Tag
    {
        public string Sequence { get; }
        public ICollection<Tag> BannedTags { get; }

        public Tag(string sequence, IEnumerable<Tag> bannedTags)
        {
            Sequence = sequence;
            BannedTags = new HashSet<Tag>(bannedTags ?? Enumerable.Empty<Tag>());
        }
    }
}