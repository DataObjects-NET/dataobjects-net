using NUnit.Framework;
using TestCommon;
using Xtensive.Core;
using Xtensive.Orm.Tracking.Tests.Model;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public abstract class TrackingTestBase : AutoBuildTest
  {
    [SetUp]
    public virtual void TestSetUp()
    {
    }

    [TearDown]
    public virtual void TestTearDown()
    {
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(ITrackingMonitor).Assembly);
      configuration.Types.Register(typeof(TrackingTestBase).Assembly);
      return configuration;
    }
  }
}
