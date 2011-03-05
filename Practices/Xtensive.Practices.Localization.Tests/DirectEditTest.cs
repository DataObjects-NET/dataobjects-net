using System.Linq;
using NUnit.Framework;
using Xtensive.Practices.Localization.Tests.Model;

namespace Xtensive.Practices.Localization.Tests
{
  public class DirectEditTest : TestBase
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var welcomePage = new Page(session);

          // Editing localizations directly
          welcomePage.Localizations[English].Title = EnglishTitle;
          welcomePage.Localizations[English].Content = EnglishContent;
          welcomePage.Localizations[Spanish].Title = SpanishTitle;
          welcomePage.Localizations[Spanish].Content = SpanishContent;

          ts.Complete();
        }
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          Assert.AreEqual(1, session.Query.All<Page>().Count());
          Assert.AreEqual(2, session.Query.All<PageLocalization>().Count());

          var page = session.Query.All<Page>().First();
          Assert.AreEqual(EnglishTitle, page.Localizations[English].Title);
          Assert.AreEqual(EnglishContent, page.Localizations[English].Content);

          Assert.AreEqual(SpanishTitle, page.Localizations[Spanish].Title);
          Assert.AreEqual(SpanishContent, page.Localizations[Spanish].Content);

          ts.Complete();
        }
      }
    }
  }
}