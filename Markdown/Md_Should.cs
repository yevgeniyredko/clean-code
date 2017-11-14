using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_Should
    {
        private Md md;

        [SetUp]
        public void SetUp()
        {
            md = new Md();
        }

        [TestCase(@"lorem ipsum dolor", ExpectedResult = @"lorem ipsum dolor")]
        [TestCase(@"_lorem ipsum dolor_", ExpectedResult = @"<em>lorem ipsum dolor</em>")]
        [TestCase(@"__lorem ipsum dolor__", ExpectedResult = @"<strong>lorem ipsum dolor</strong>")]
        [TestCase(@"\_lorem ipsum dolor\_", ExpectedResult = @"_lorem ipsum dolor_")]
        [TestCase(@"__lorem _ipsum_ dolor__", ExpectedResult = @"<strong>lorem <em>ipsum</em> dolor</strong>")]
        [TestCase(@"_lorem __ipsum__ dolor_", ExpectedResult = @"<em>lorem __ipsum__ dolor</em>")]
        [TestCase(@"lorem_12_3", ExpectedResult = @"lorem_12_3")]
        [TestCase(@"__lorem _ipsum dolor", ExpectedResult = @"__lorem _ipsum dolor")]
        [TestCase(@"lorem_ ipsum_ dolor_", ExpectedResult = @"lorem_ ipsum_ dolor_")]
        [TestCase(@"_lorem _ipsum _dolor", ExpectedResult = @"_lorem _ipsum _dolor")]
        public string RenderToHtml(string markdown)
        {
            return md.RenderToHtml(markdown);
        }
    }
}