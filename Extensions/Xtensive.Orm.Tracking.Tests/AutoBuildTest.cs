using NUnit.Framework;
using System;
using TestCommon;
using Xtensive.Core;
using Xtensive.Orm.Tracking.Tests.Model;
using Xtensive.Reflection;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    protected Domain Domain { get; private set; }

    [SetUp]
    public virtual void TestSetUp()
    {
      
    }

    [TearDown]
    public virtual void TestTearDown()
    {
      
    }

    [OneTimeSetUp]
    public virtual void TestFixtureSetUp()
    {
      var config = BuildConfiguration();
      Domain = BuildDomain(config);
    }

    [OneTimeTearDown]
    public virtual void TestFixtureTearDown()
    {
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ITrackingMonitor).Assembly);
      configuration.Types.Register(typeof (AutoBuildTest).Assembly);
      return configuration;
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        Console.WriteLine(GetType().GetFullName());
        Console.WriteLine(e);
        throw;
      }
    }
  }
}
