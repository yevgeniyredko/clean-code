using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class Parser
    {
        private readonly SortedSet<Tag> tags;

        public Parser()
        {
            tags = new SortedSet<Tag>(Comparer<Tag>.Create((t1, t2) =>
                t1.Sequence.Length.CompareTo(t2.Sequence.Length)));
        }

        public Parser AddTag(Tag tag)
        {
            if (tags.Any(t => t.Sequence == tag.Sequence))
                throw new ArgumentException(
                    $"A tag with the same sequence {tag.Sequence} already exists", nameof(tag));
            tags.Add(tag);
            return this;
        }

        public List<INode> Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return InternalParse(text);
        }

        private List<INode> InternalParse(string text)
        {
            var currentPtr = 0;
            var openingTags = new LinkedList<TagNode>();
            var parsed = new List<INode>();

            while (currentPtr < text.Length)
            {
                var (newPtr, tag) = GetClosestTag(text, currentPtr);

                if (newPtr < 0)
                {
                    parsed.Add(new StringNode(text.Substring(currentPtr)));
                    break;
                }

                if (IsEscapedCharacter(text, newPtr))
                {
                    parsed.Add(new StringNode(text.Substring(currentPtr, newPtr - currentPtr - 1) + text[newPtr]));
                    currentPtr = newPtr + 1;
                }
                else
                {
                    parsed.Add(new StringNode(text.Substring(currentPtr, newPtr - currentPtr)));
                    parsed.Add(CreateTagNode(openingTags, tag, GetTagType(text, newPtr, tag)));
                    currentPtr = newPtr + tag.Sequence.Length;
                }
            }
            return parsed;
        }

        private (int position, Tag tag) GetClosestTag(string text, int startIndex)
        {
            while (startIndex + tags.Min.Sequence.Length <= text.Length)
            {
                foreach (var tag in tags.Reverse())
                {
                    var sequence = tag.Sequence;
                    if (startIndex + sequence.Length > text.Length) continue;
                    var pos = text.IndexOf(sequence, startIndex, sequence.Length, StringComparison.Ordinal);
                    if (pos >= 0) return (pos, tag);
                }
                startIndex++;
            }
            return (-1, null);
        }

        private static bool IsEscapedCharacter(string s, int pos)
        {
            return pos > 0 && s[pos - 1] == '\\';
        }

        private static TagType GetTagType(string text, int startIndex, Tag tag)
        {
            var previousCharIndex = startIndex - 1;
            var previousChar = previousCharIndex >= 0 ? text[previousCharIndex] : 'a';

            var nextCharIndex = startIndex + tag.Sequence.Length;
            var nextChar = nextCharIndex < text.Length ? text[nextCharIndex] : 'a';

            if (char.IsDigit(previousChar) && !char.IsWhiteSpace(nextChar) 
                || char.IsDigit(nextChar) && !char.IsWhiteSpace(previousChar)
                || char.IsWhiteSpace(previousChar) && char.IsWhiteSpace(nextChar))
                return TagType.NonTag;
            if (char.IsWhiteSpace(previousChar))
                return TagType.Opening;
            if (char.IsWhiteSpace(nextChar))
                return TagType.Closing;
            return TagType.Universal;
        }

        private static INode CreateTagNode(LinkedList<TagNode> openingTags, Tag tag, TagType type)
        {
            switch (type)
            {
                case TagType.Opening:
                    return CreateOpeningTagNode(openingTags, tag);
                case TagType.Closing:
                    return TryCreateClosingTagNode(openingTags, tag, out var tagNode)
                        ? tagNode
                        : TagNode.Closing(tag);
                case TagType.Universal:
                    return TryCreateClosingTagNode(openingTags, tag, out tagNode) 
                        ? tagNode 
                        : CreateOpeningTagNode(openingTags, tag);
                case TagType.NonTag:
                    return new StringNode(tag.Sequence);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static TagNode CreateOpeningTagNode(LinkedList<TagNode> openingTags, Tag tag)
        {
            var tagNode = TagNode.Opening(tag);
            if (openingTags.Last != null)
                tagNode.Parent = openingTags.Last.Value;
            openingTags.AddLast(tagNode);
            return tagNode;
        }

        private static bool TryCreateClosingTagNode(
            LinkedList<TagNode> openingTags, Tag tag, out TagNode result)
        {
            if (TryFindPair(openingTags, tag, out var listNode))
            {
                var tagNode = TagNode.Closing(tag);
                TagNode.Connect(listNode.Value, tagNode);
                openingTags.Remove(listNode);
                result = tagNode;
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryFindPair(
            LinkedList<TagNode> openedTags, Tag tag, out LinkedListNode<TagNode> result)
        {
            var curreListNode = openedTags.Last;
            while (curreListNode != null)
            {
                if (curreListNode.Value.Tag == tag)
                {
                    result = curreListNode;
                    return true;
                }
                curreListNode = curreListNode.Previous;
            }
            result = null;
            return false;
        }
    }
}