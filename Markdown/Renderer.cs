using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    public class Renderer
    {
        private readonly Dictionary<Tag, string> htmlTags = new Dictionary<Tag, string>();

        public Renderer AddTag(Tag tag, string htmlTag)
        {
            if (htmlTag == null)
                throw new ArgumentNullException(nameof(htmlTag));
            if (htmlTags.ContainsKey(tag))
                throw new ArgumentException("Tag already exists", nameof(tag));
            htmlTags.Add(tag, htmlTag);
            return this;
        }

        public string Render(List<INode> nodes)
        {
            var builder = new StringBuilder();
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case StringNode n:
                        builder.Append(n.Text);
                        break;
                    case TagNode n:
                        builder.Append(RenderNode(n));
                        break;
                }
            }
            return builder.ToString();
        }

        private string RenderNode(TagNode node)
        {
            if (!htmlTags.ContainsKey(node.Tag))
                throw new ArgumentException("Tag does not exist", nameof(node));
            if (node.IsBanned || !node.HasPair)
                return node.Tag.Sequence;
            return GetHtmlTag(htmlTags[node.Tag], node.IsOpeningTag);
        }

        private static string GetHtmlTag(string text, bool isOpeningTag)
        {
            return $"<{(isOpeningTag ? "" : "/")}{text}>";
        }
    }
}