using System.Linq;
using NUnit.Framework;
using Xtensive.Practices.Localization.Tests.Model;

namespace Xtensive.Practices.Localization.Tests
{
  public class LocalizationScopeTest : TestBase
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var welcomePage = new Page(session);

          // Editing localizable properties through localization scope
          using (new LocalizationScope(English)) {
            welcomePage.Title = EnglishTitle;
            welcomePage.Content = EnglishContent;
          }

          // The same entity, the same properties, but another culture
          using (new LocalizationScope(Spanish)) {
            welcomePage.Title = SpanishTitle;
            welcomePage.Content = SpanishContent;
          }

          ts.Complete();
        }
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          Assert.AreEqual(1, session.Query.All<Page>().Count());
          Assert.AreEqual(2, session.Query.All<PageLocalization>().Count());

          var page = session.Query.All<Page>().First();
          using (new LocalizationScope(English)) {
            Assert.AreEqual(EnglishTitle, page.Title);
            Assert.AreEqual(EnglishContent, page.Content);
          }

          using (new LocalizationScope(Spanish)) {
            Assert.AreEqual(SpanishTitle, page.Title);
            Assert.AreEqual(SpanishContent, page.Content);
          }

          ts.Complete();
        }
      }
    }
  }
}