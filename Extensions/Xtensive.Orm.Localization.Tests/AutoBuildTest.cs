using System.Globalization;
using NUnit.Framework;
using TestCommon;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    public static CultureInfo EnglishCulture = new CultureInfo("en-US");
    public static string EnglishTitle = "Welcome!";
    public static string EnglishContent = "My dear guests, welcome to my birthday party!";

    public static CultureInfo SpanishCulture = new CultureInfo("es-ES");
    public static string SpanishTitle = "Bienvenido!";
    public static string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    protected Domain Domain { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ILocalizable<>).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly, typeof (AutoBuildTest).Namespace);
      Domain = Domain.Build(configuration);
      PopulateDatabase();
    }

    protected virtual void PopulateDatabase()
    {
    }
  }
}