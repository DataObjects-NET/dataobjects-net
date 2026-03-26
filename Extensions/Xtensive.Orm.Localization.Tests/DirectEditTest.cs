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

        Assert.AreEqual(1, session.Query.All<Page>().Count());
        Assert.AreEqual(2, session.Query.All<PageLocalization>().Count());

        var page = session.Query.All<Page>().First();
        Assert.AreEqual(English.Title, page.Localizations[English.Culture].Title);
        Assert.AreEqual(English.Content, page.Localizations[English.Culture].Content);

        Assert.AreEqual(Spanish.Title, page.Localizations[Spanish.Culture].Title);
        Assert.AreEqual(Spanish.Content, page.Localizations[Spanish.Culture].Content);

        ts.Complete();
      }
      
    }
  }
}