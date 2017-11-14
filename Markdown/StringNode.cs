namespace Markdown
{
    public class StringNode : INode
    {
        public string Text { get; }

        public StringNode(string text)
        {
            Text = text;
        }
    }
}