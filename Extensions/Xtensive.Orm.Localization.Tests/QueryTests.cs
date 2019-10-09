using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class QueryTests : AutoBuildTest
  {
    protected override void PopulateDatabase()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          // populating database
          var welcomePage = new Page(session);
          welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
          welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
          welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
          welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

          ts.Complete();
        }
      }
    }

    [Test]
    public void ImplicitJoinViaPreprocessorTest()
    {
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
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
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          using (new LocalizationScope(SpanishCulture)) {
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
      Thread.CurrentThread.CurrentCulture = EnglishCulture;
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

    [Test]
    public void UnknownCultureTest()
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var query = from p in session.Query.All<Page>()
          select p.Title;
          Console.Write(query.First());
          Assert.AreEqual(1, query.Count());

          ts.Complete();
        }
      }
    }
  }
}