using System.Globalization;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Localization.Tests
{
  [TestFixture]
  public abstract class LocalizationBaseTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(ILocalizable<>).Assembly);
      configuration.Types.Register(typeof(LocalizationBaseTest).Assembly, typeof(LocalizationBaseTest).Namespace);
      return configuration;
    }
  }
}