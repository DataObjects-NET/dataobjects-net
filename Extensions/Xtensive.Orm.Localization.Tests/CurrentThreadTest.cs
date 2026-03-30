using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests
{
  public class CurrentThreadTest : LocalizationBaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          var welcomePage = new Page(session);

          // Editing localizable properties through CurrentThread.CurrentCulture
          Thread.CurrentThread.CurrentCulture = English.Culture;
          welcomePage.Title = English.Title;
          welcomePage.Content = English.Content;

          // The same entity, the same properties, but another culture
          Thread.CurrentThread.CurrentCulture = Spanish.Culture;
          welcomePage.Title = Spanish.Title;
          welcomePage.Content = Spanish.Content;

          ts.Complete();
        }
      }

      // Checking the presence of localizable & localization entities
      using (var session = Domain.OpenSession()) {
        using (var ts = session.OpenTransaction()) {

          Assert.That(session.Query.All<Page>().Count(), Is.EqualTo(1));
          Assert.That(session.Query.All<PageLocalization>().Count(), Is.EqualTo(2));

          var page = session.Query.All<Page>().First();
          Thread.CurrentThread.CurrentCulture = English.Culture;
          Assert.That(page.Title, Is.EqualTo(English.Title));
          Assert.That(page.Content, Is.EqualTo(English.Content));

          Thread.CurrentThread.CurrentCulture = Spanish.Culture;
          Assert.That(page.Title, Is.EqualTo(Spanish.Title));
          Assert.That(page.Content, Is.EqualTo(Spanish.Content));

          ts.Complete();
        }
      }
    }
  }
}