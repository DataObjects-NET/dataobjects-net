using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Practices.Localization.Tests.Model;

namespace Xtensive.Practices.Localization.Tests
{
  [TestFixture]
  public class QueryTests : TestBase
  {
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          // populating database
          var welcomePage = new Page(session);
          welcomePage.Localizations[English].Title = EnglishTitle;
          welcomePage.Localizations[English].Content = EnglishContent;
          welcomePage.Localizations[Spanish].Title = SpanishTitle;
          welcomePage.Localizations[Spanish].Content = SpanishContent;

          ts.Complete();
        }
      }
    }

    [Test]
    public void ImplicitJoinViaPreprocessorTest()
    {
      Thread.CurrentThread.CurrentCulture = English;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          string title = EnglishTitle;
          var query = from p in session.Query.All<Page>()
          where p.Title==title
          select p;
          Assert.AreEqual(1, query.Count());

          ts.Complete();
        }
      }
    }

    [Test]
    public void ExplicitJoinTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          using (new LocalizationScope(Spanish)) {
            var query = from p in session.Query.All<Page>()
                        join pl in session.Query.All<PageLocalization>()
                          on p equals pl.Target
                        where pl.CultureName == LocalizationContext.Current.CultureName && pl.Title == SpanishTitle
                        select p;
            Assert.AreEqual(1, query.Count());
          }

          ts.Complete();
        }
      }
    }

    [Test]
    public void QueryForLocalizationPairTest()
    {
      Thread.CurrentThread.CurrentCulture = English;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var pairs = from pair in session.Query.All<Page, PageLocalization>()
          where pair.Localization.Title==EnglishTitle
          select pair.Target;
          Assert.AreEqual(1, pairs.Count());

          ts.Complete();
        }
      }
    }

  }
}