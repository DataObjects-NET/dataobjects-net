using System;
using NUnit.Framework;

namespace Xtensive.PluginManager.Tests
{
  [TestFixture]
  public class AppDomainManagerTests: MarshalByRefObject
  {
    [Test]
    public void RecreationTest()
    {
      using (AppDomainManager apm = new AppDomainManager()) {
        apm.DomainRecreate += new EventHandler<EventArgs>(apm_DomainRecreate);
        apm.DomainUnload += new EventHandler<EventArgs>(apm_DomainUnload);
        AppDomain.Unload(apm.Domain);
      }
    }

    private static void apm_DomainRecreate(object sender, EventArgs e)
    {
      AppDomainManager apm = sender as AppDomainManager;
      Assert.IsNotNull(apm.Domain);
      apm.Domain.DoCallBack(delegate { Console.WriteLine("test"); });
    }

    private static void apm_DomainUnload(object sender, EventArgs e)
    {
      try {
        AppDomainManager apm = sender as AppDomainManager;
        Assert.IsNotNull(apm.Domain);
        apm.Domain.DoCallBack(delegate { Console.WriteLine("test"); });
      }
      catch (AppDomainUnloadedException) {
      }
    }
  }
}