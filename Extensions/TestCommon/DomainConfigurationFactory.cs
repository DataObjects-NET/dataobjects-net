using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace TestCommon
{
  public static class DomainConfigurationFactory
  {
    public static DomainConfiguration Create(string name = null)
    {
      var testConfiguration = TestConfiguration.Instance;
      var storageName = name ?? testConfiguration.Storage;

      var configuration = typeof(DomainConfigurationFactory).Assembly.GetAssemblyConfiguration();
      var domainConfiguration = DomainConfiguration.Load(configuration, storageName);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      var customConnectionInfo = testConfiguration.GetConnectionInfo(storageName);
      if (customConnectionInfo != null)
        domainConfiguration.ConnectionInfo = customConnectionInfo;
      return domainConfiguration;
    }
  }
}