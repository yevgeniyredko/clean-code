namespace Markdown
{
    public class Md
    {
        private readonly Parser parser;
        private readonly Renderer renderer;

        public Md()
        {
            var strong = new Tag("__");
            var em = new Tag("_")
                .AddBannedTag(strong);
            parser = new Parser()
                .AddTag(em)
                .AddTag(strong);
            renderer = new Renderer()
                .AddTag(em, "em")
                .AddTag(strong, "strong");
        }

        public string RenderToHtml(string markdown)
        {
            var nodes = parser.Parse(markdown);
            return renderer.Render(nodes);
        }
    }
}