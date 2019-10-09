using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  public class CurrentThreadTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var welcomePage = new Page(session);

          // Editing localizable properties through CurrentThread.CurrentCulture
          Thread.CurrentThread.CurrentCulture = EnglishCulture;
          welcomePage.Title = EnglishTitle;
          welcomePage.Content = EnglishContent;

          // The same entity, the same properties, but another culture
          Thread.CurrentThread.CurrentCulture = SpanishCulture;
          welcomePage.Title = SpanishTitle;
          welcomePage.Content = SpanishContent;

          ts.Complete();
        }
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          Assert.AreEqual(1, session.Query.All<Page>().Count());
          Assert.AreEqual(2, session.Query.All<PageLocalization>().Count());

          var page = session.Query.All<Page>().First();
          Thread.CurrentThread.CurrentCulture = EnglishCulture;
          Assert.AreEqual(EnglishTitle, page.Title);
          Assert.AreEqual(EnglishContent, page.Content);

          Thread.CurrentThread.CurrentCulture = SpanishCulture;
          Assert.AreEqual(SpanishTitle, page.Title);
          Assert.AreEqual(SpanishContent, page.Content);

          ts.Complete();
        }
      }
    }
  }
}