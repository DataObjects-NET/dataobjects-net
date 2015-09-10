using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.ExtractedSchemaCaching
{
  [TestFixture]
  public abstract class NodeBuildingTest
  {
    [Test]
    public void RecreateTest()
    {
      RunTest(DomainUpgradeMode.Recreate);
    }

    [Test]
    public void PerformTest()
    {
      RunTest(DomainUpgradeMode.Perform);
    }

    [Test]
    public void PerformSafelyTest()
    {
      RunTest(DomainUpgradeMode.PerformSafely);
    }

    [Test]
    public void ValidateTest()
    {
      RunTest(DomainUpgradeMode.Validate);
    }

    [Test]
    public void SkipTest()
    {
      RunTest(DomainUpgradeMode.Skip);
    }

    [Test]
    public void LegacySkipTest()
    {
      RunTest(DomainUpgradeMode.LegacySkip);
    }

    [Test]
    public void LegacyValidateTest()
    {
      RunTest(DomainUpgradeMode.LegacyValidate);
    }

    private void RunTest(DomainUpgradeMode upgradeMode)
    {
      var domainConfiguration = GetDomainConfiguration(upgradeMode);
      var firstNodeConfiguration = GetFirstNodeConfiguration(upgradeMode);
      var secondNodeConfiguration = GetSecondNodeConfiguration(upgradeMode);

      if (ShouldBuildInitialDomain(upgradeMode)) {
        var initialDomainConfiguration = domainConfiguration.Clone();
        var firstInitialNodeConfiguration = (NodeConfiguration)firstNodeConfiguration.Clone();
        var secondInitialNodeConfiguration = (secondNodeConfiguration!=null)? (NodeConfiguration)secondNodeConfiguration.Clone() : null;

        initialDomainConfiguration.UpgradeMode =
          firstInitialNodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
        if(secondInitialNodeConfiguration!=null)
          secondInitialNodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

        using (var initialDomain = BuildDomain(initialDomainConfiguration)) {
          initialDomain.StorageNodeManager.AddNode(firstInitialNodeConfiguration);
          if (secondInitialNodeConfiguration!=null)
            initialDomain.StorageNodeManager.AddNode(secondInitialNodeConfiguration);
        }
      }
      
      using (var domain = BuildDomain(domainConfiguration)) {
        domain.StorageNodeManager.AddNode(firstNodeConfiguration);
        if(secondNodeConfiguration!=null)
          domain.StorageNodeManager.AddNode(secondNodeConfiguration);
        RunQueries(domain);
      }
    }

    protected abstract DomainConfiguration GetDomainConfiguration(DomainUpgradeMode upgradeMode);
    protected abstract NodeConfiguration GetFirstNodeConfiguration(DomainUpgradeMode upgradeMode);
    protected abstract NodeConfiguration GetSecondNodeConfiguration(DomainUpgradeMode upgradeMode);
    

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      CheckRequirements();
    }

    protected virtual void CheckRequirements()
    {
    }

    protected virtual void RunQueries(Domain domain)
    {
      
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    protected void EnsureSchemasExist(DomainConfiguration configuration, params string[] schemas)
    {
      StorageTestHelper.DemandSchemas(configuration.ConnectionInfo, schemas);
    }

    protected bool ShouldBuildInitialDomain(DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Recreate:
          return false;
        default:
          return true;
      }
    }
  }
}