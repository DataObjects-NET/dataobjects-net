using System.Globalization;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public abstract class LocalizationBaseTest : AutoBuildTest
  {
    public static CultureInfo EnglishCulture = new CultureInfo("en-US");
    public static string EnglishTitle = "Welcome!";
    public static string EnglishContent = "My dear guests, welcome to my birthday party!";

    public static CultureInfo SpanishCulture = new CultureInfo("es-ES");
    public static string SpanishTitle = "Bienvenido!";
    public static string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(LocalizationBaseTest).Assembly, typeof(LocalizationBaseTest).Namespace);
      return configuration;
    }
  }
}