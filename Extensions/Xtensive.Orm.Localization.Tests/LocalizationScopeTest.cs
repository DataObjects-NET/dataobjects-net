using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests
{
  public class LocalizationScopeTest : LocalizationBaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        var welcomePage = new Page(session);

        // Editing localizable properties through localization scope
        using (new LocalizationScope(English.Culture)) {
          welcomePage.Title = English.Title;
          welcomePage.Content = English.Content;
        }

        // The same entity, the same properties, but another culture
        using (new LocalizationScope(Spanish.Culture)) {
          welcomePage.Title = Spanish.Title;
          welcomePage.Content = Spanish.Content;
        }

        ts.Complete();
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        Assert.That(session.Query.All<Page>().Count(), Is.EqualTo(1));
        Assert.That(session.Query.All<PageLocalization>().Count(), Is.EqualTo(2));

        var page = session.Query.All<Page>().First();
        using (new LocalizationScope(English.Culture)) {
          Assert.That(page.Title, Is.EqualTo(English.Title));
          Assert.That(page.Content, Is.EqualTo(English.Content));
        }

        using (new LocalizationScope(Spanish.Culture)) {
          Assert.That(page.Title, Is.EqualTo(Spanish.Title));
          Assert.That(page.Content, Is.EqualTo(Spanish.Content));
        }

        ts.Complete();
      }
    }
  }
}