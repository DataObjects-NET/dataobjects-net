using NUnit.Framework;
using TestCommon;
using TestCommon.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Reprocessing.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest : CommonModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (IExecuteActionStrategy).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly);
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      if (domain.StorageProviderInfo.Supports(ProviderFeatures.ExclusiveWriterConnection))
        Assert.Ignore("This storage does not support multiple sessions writing to the same database, ignoring");
      return domain;
    }
  }
}