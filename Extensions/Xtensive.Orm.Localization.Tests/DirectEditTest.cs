using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  public class DirectEditTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var welcomePage = new Page(session);

          // Editing localizations directly
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          ts.Complete();
        }
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          Assert.AreEqual(1, session.Query.All<Page>().Count());
          Assert.AreEqual(2, session.Query.All<PageLocalization>().Count());

          var page = session.Query.All<Page>().First();
          Assert.AreEqual(EnglishTitle, page.Localizations[EnglishCulture].Title);
          Assert.AreEqual(EnglishContent, page.Localizations[EnglishCulture].Content);

          Assert.AreEqual(SpanishTitle, page.Localizations[SpanishCulture].Title);
          Assert.AreEqual(SpanishContent, page.Localizations[SpanishCulture].Content);

          ts.Complete();
        }
      }
    }
  }
}