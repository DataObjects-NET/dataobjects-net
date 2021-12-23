// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1;
using D = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2.D;
using M1 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.DataUpgrade
{
  [TestFixture, Category("Upgrade")]
  public class DataUpgradeTest
  {
    [SetUp]
    public void SetUp()
    {
      using var domain = BuildDomain("1", DomainUpgradeMode.Recreate);
      FillData(domain);
    }
    
    [Test]
    public void ClearDataTest1()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Perform, typeof(A)))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(1, s.Query.All<A>().Count());
      }
    }

    [Test]
    public async Task ClearDataAsyncTest1()
    {
      using (var domain = await BuildDomainAsync(DomainUpgradeMode.Perform, typeof(A)))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(1, s.Query.All<A>().Count());
      }
    }

    [Test]
    public void ClearDataTest2()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Perform, typeof(A), typeof(B)))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(2, s.Query.All<A>().Count());
      }
    }

    [Test]
    public async Task ClearDataAsyncTest2()
    {
      using (var domain = await BuildDomainAsync(DomainUpgradeMode.Perform, typeof(A), typeof(B)))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(2, s.Query.All<A>().Count());
      }
    }

    [Test]
    public void ClearDataTest3()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.Perform))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(4, s.Query.All<Model.Version2.A>().Count());
        var firstD = s.Query.All<D>().First();
        Assert.AreEqual(2, firstD.RefA.Count());
      }
    }

    [Test]
    public async Task ClearDataAsyncTest3()
    {
      using (var domain = await BuildDomainAsync("2", DomainUpgradeMode.Perform))
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        Assert.AreEqual(4, s.Query.All<Model.Version2.A>().Count());
        var firstD = s.Query.All<D>().First();
        Assert.AreEqual(2, firstD.RefA.Count());
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version" + version);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable(version)) {
        var domain = Domain.Build(configuration);
        return domain;
      }
    }

    private async Task<Domain> BuildDomainAsync(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version" + version);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable(version)) {
        var domain = await Domain.BuildAsync(configuration);
        return domain;
      }
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types) {
        configuration.Types.Register(type);
      }
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable("1")) {
        var domain = Domain.Build(configuration);
        return domain;
      }
    }

    private async Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types) {
        configuration.Types.Register(type);
      }
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable("1")) {
        var domain = await Domain.BuildAsync(configuration);
        return domain;
      }
    }

    private void FillData(Domain domain)
    {
      using (var s = domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        var a1 = new A();
        var b1 = new B();
        var c1 = new C();
        var c2 = new C();
        var d1 = new Model.Version1.D();
        c1.RefA = a1;
        c1.RefB = b1;
        c2.RefA = b1;
        _ = d1.RefA.Add(a1);
        _ = d1.RefA.Add(b1);
        _ = d1.RefA.Add(c1);
        t.Complete();
      }
    }
  }
}