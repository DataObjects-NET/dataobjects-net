using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Localization.Tests.Model.Upgrader;
using Xtensive.Orm.Tests;
using English = Xtensive.Orm.Localization.Tests.WellKnownCultures.English;
using Spanish = Xtensive.Orm.Localization.Tests.WellKnownCultures.Spanish;

namespace Xtensive.Orm.Localization.Tests.Model.Upgrader
{
  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnUpgrade()
    {
      _ = Query.All<Page>().FirstOrDefault(x => x.Title == "Welcome!");
    }
  }
}

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public class AccessToLocalizationOnUpgrade
  {
    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(Page).Assembly, typeof(Page).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        // populating database
        var welcomePage = new Page(session);
        welcomePage.Localizations[English.Culture].Title = English.Title;
        welcomePage.Localizations[English.Culture].Content = English.Content;
        welcomePage.Localizations[Spanish.Culture].Title = Spanish.Title;
        welcomePage.Localizations[Spanish.Culture].Content = Spanish.Content;

        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ILocalizable<>).Assembly);
      configuration.Types.Register(typeof (Page).Assembly, typeof (Page).Namespace);
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<Page>().FirstOrDefault(x => x.Title==English.Title);
      }
    }
  }
}
