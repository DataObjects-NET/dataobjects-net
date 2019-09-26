using NUnit.Framework;
using TestCommon;
using TestCommon.Model;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.BulkOperations.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest : CommonModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (IUpdatable<>).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly);
      return configuration;
    }
  }
}