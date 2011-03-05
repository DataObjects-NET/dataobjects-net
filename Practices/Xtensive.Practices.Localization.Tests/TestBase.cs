using System.Globalization;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Reflection;

namespace Xtensive.Practices.Localization.Tests
{
  [TestFixture]
  public class TestBase
  {
    public static CultureInfo English = new CultureInfo("en-US");
    public static CultureInfo Spanish = new CultureInfo("es-ES");

    public static string EnglishTitle = "Welcome!";
    public static string EnglishContent = "My dear guests, welcome to my birthday party!";

    public static string SpanishTitle = "Bienvenido!";
    public static string SpanishContent = "Mis amigos mejores! Bienvenido a mi cumpleanos!";

    protected Domain Domain { get; private set; }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      var domain = Domain.Build(DomainConfiguration.Load("Default"));

      var map = new TypeLocalizationMap();
      foreach (var localizableTypeInfo in domain.Model.Types.Entities) {
        var type = localizableTypeInfo.UnderlyingType;
        if (!type.IsOfGenericInterface(typeof (ILocalizable<>)))
          continue;
        var localizationType = type.GetInterface("ILocalizable`1").GetGenericArguments()[0];
        map.Register(type, localizationType.GetTypeInfo(domain));
      }
      domain.Extensions.Set(map);

      Domain = domain;
    }
  }
}