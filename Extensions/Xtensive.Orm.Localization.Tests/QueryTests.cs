using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class QueryTests : LocalizationBaseTest
  {
    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        // populating database
        var welcomePage = new Page(session);
        welcomePage.Localizations[English.Culture].Title = English.Title;
        welcomePage.Localizations[English.Culture].Content = English.Content;
        welcomePage.Localizations[Spanish.Culture].Title = Spanish.Title;
        welcomePage.Localizations[Spanish.Culture].Content = Spanish.Content;

        ts.Complete();
      }
    }

    [Test]
    public void ImplicitJoinViaPreprocessorTest()
    {
      Thread.CurrentThread.CurrentCulture = English.Culture;
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          string title = English.Title;
          var query = from p in session.Query.All<Page>()
          where p.Title == title
          select p;
          Assert.AreEqual(1, query.Count());

          ts.Complete();
        }
      }
    }

    [Test]
    public void ExplicitJoinTest()
    {
      Thread.CurrentThread.CurrentCulture = English.Culture;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        using (new LocalizationScope(Spanish.Culture)) {
          var query = from p in session.Query.All<Page>()
                      join pl in session.Query.All<PageLocalization>()
                        on p equals pl.Target
                      where pl.CultureName == LocalizationContext.Current.CultureName && pl.Title == Spanish.Title
                      select p;
          Assert.AreEqual(1, query.Count());
        }

        ts.Complete();
      }
    }

    [Test]
    public void QueryForLocalizationPairTest()
    {
      Thread.CurrentThread.CurrentCulture = English.Culture;
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        var pairs = from pair in session.Query.All<Page, PageLocalization>()
                    where pair.Localization.Title == English.Title
                    select pair.Target;
        Assert.AreEqual(1, pairs.Count());

        ts.Complete();
      }
    }

    [Test]
    public void UnknownCultureTest()
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
      using (var session = Domain.OpenSession())
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