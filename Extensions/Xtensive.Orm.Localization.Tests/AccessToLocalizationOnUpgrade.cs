using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Localization.Tests.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Localization.Tests.Model.Upgrader;
using Xtensive.Orm.Tests;

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
    public static CultureInfo EnglishCulture = new CultureInfo("en-US");
    public static string EnglishTitle = "Welcome!";
    public static string EnglishContent = "My dear guests, welcome to my birthday party!";

    public static CultureInfo SpanishCulture = new CultureInfo("es-ES");
    public static string SpanishTitle = "Bienvenido!";
    public static string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(LocalizationBaseTest).Assembly, typeof(LocalizationBaseTest).Namespace);

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        // populating database
        var welcomePage = new Page(session);
        welcomePage.Localizations[EnglishCulture].Title = EnglishTitle;
        welcomePage.Localizations[EnglishCulture].Content = EnglishContent;
        welcomePage.Localizations[SpanishCulture].Title = SpanishTitle;
        welcomePage.Localizations[SpanishCulture].Content = SpanishContent;

        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ILocalizable<>).Assembly);
      configuration.Types.Register(typeof (LocalizationBaseTest).Assembly, typeof (LocalizationBaseTest).Namespace);
      configuration.Types.Register(typeof (CustomUpgradeHandler));
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<Page>().FirstOrDefault(x => x.Title==EnglishTitle);
      }
    }
  }
}
