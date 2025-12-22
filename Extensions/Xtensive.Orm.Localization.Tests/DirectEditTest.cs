using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests
{
  public class DirectEditTest : LocalizationBaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        var welcomePage = new Page(session);

        // Editing localizations directly
        welcomePage.Localizations[English.Culture].Title = English.Title;
        welcomePage.Localizations[English.Culture].Content = English.Content;
        welcomePage.Localizations[Spanish.Culture].Title = Spanish.Title;
        welcomePage.Localizations[Spanish.Culture].Content = Spanish.Content;

        ts.Complete();
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        Assert.That(session.Query.All<Page>().Count(), Is.EqualTo(1));
        Assert.That(session.Query.All<PageLocalization>().Count(), Is.EqualTo(2));

        var page = session.Query.All<Page>().First();
        Assert.That(page.Localizations[English.Culture].Title, Is.EqualTo(English.Title));
        Assert.That(page.Localizations[English.Culture].Content, Is.EqualTo(English.Content));

        Assert.That(page.Localizations[Spanish.Culture].Title, Is.EqualTo(Spanish.Title));
        Assert.That(page.Localizations[Spanish.Culture].Content, Is.EqualTo(Spanish.Content));

        ts.Complete();
      }
      
    }
  }
}