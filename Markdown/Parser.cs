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
            if (!tags.Add(tag))
                throw new ArgumentException(
                    $"A tag with the same sequence {tag.Sequence} already exists", nameof(tag));
            return this;
        }

        public List<INode> Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var currentPtr = 0;
            var openingTags = new Stack<TagNode>();
            var result = new List<INode>();

            while (currentPtr < text.Length)
            {
                var (newPtr, tag) = GetNextTag(text, currentPtr);

                if (newPtr < 0)
                {
                    result.Add(new StringNode(text.Substring(currentPtr)));
                    break;
                }

                if (IsEscapedCharacter(text, newPtr))
                {
                    result.Add(new StringNode(text.Substring(currentPtr, newPtr - currentPtr - 1) + text[newPtr]));
                    currentPtr = newPtr + 1;
                }
                else
                {
                    result.Add(new StringNode(text.Substring(currentPtr, newPtr - currentPtr)));
                    result.Add(CreateTagNode(openingTags, tag, GetTagType(text, newPtr, tag)));
                    currentPtr = newPtr + tag.Sequence.Length;
                }
            }
            ReplaceBannedAndNotPairedNodes(result);
            return result;
        }

        private (int position, Tag tag) GetNextTag(string text, int startIndex)
        {
            while (startIndex + tags.Min.Sequence.Length <= text.Length)
            {
                foreach (var tag in tags.Reverse())
                {
                    var sequence = tag.Sequence;
                    if (startIndex + sequence.Length > text.Length) 
                        continue;
                    var pos = text.IndexOf(sequence, startIndex, sequence.Length, StringComparison.Ordinal);
                    if (pos >= 0) 
                        return (pos, tag);
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
            var previousChar = previousCharIndex >= 0 ? text[previousCharIndex] : ' ';

            var nextCharIndex = startIndex + tag.Sequence.Length;
            var nextChar = nextCharIndex < text.Length ? text[nextCharIndex] : ' ';

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

        private static INode CreateTagNode(Stack<TagNode> openingTags, Tag tag, TagType type)
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

        private static TagNode CreateOpeningTagNode(Stack<TagNode> openingTags, Tag tag)
        {
            var tagNode = TagNode.Opening(tag);

            if (openingTags.Count > 0)
                tagNode.Parent = openingTags.Peek();

            openingTags.Push(tagNode);
            return tagNode;
        }

        private static bool TryCreateClosingTagNode(
            Stack<TagNode> openingTags, Tag tag, out TagNode result)
        {
            if (TryFindPair(openingTags, tag, out var openedTag))
            {
                var tagNode = TagNode.Closing(tag);
                ConnectTagNodes(openedTag, tagNode);
                result = tagNode;
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryFindPair(
            Stack<TagNode> openedTags, Tag tag, out TagNode result)
        {
            if (openedTags.Select(n => n.Tag).Contains(tag))
            {
                var node = openedTags.Pop();
                while (node.Tag != tag)
                {
                    node = openedTags.Pop();
                }
                result = node;
                return true;
            }
            result = null;
            return false;
        }

        private static void ConnectTagNodes(TagNode opening, TagNode closing)
        {
            opening.Pair = closing;
            closing.Pair = opening;
            closing.Parent = opening.Parent;
        }

        private static void ReplaceBannedAndNotPairedNodes(List<INode> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (!(nodes[i] is TagNode n)) 
                    continue;

                if (n.IsBanned || !n.HasPair)
                    nodes[i] = new StringNode(n.Tag.Sequence);
            }
        }
    }
}