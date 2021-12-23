// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest.Model.Version1;
using M1 = Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest.Model.Version2;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest
{
  [TestFixture]
  public class UpgradeTest
  {
    [SetUp]
    public void SetUp()
    {
      using (var domain = BuildDomain("1", DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = new Person();
        var address = new Address() {
          Person = person
        };
        // person.Addresses.Add(address);
        tx.Complete();
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        Assert.AreEqual(1, session.Query.All<Model.Version2.Person>().Count());
      }
    }

    [Test]
    public async Task UpgradeToVersion2AsyncTest()
    {
      using (var domain = await BuildDomainAsync("2", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        Assert.AreEqual(1, session.Query.All<Model.Version2.Person>().Count());
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var ns = typeof(Person).Namespace;
      var nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        return Domain.Build(configuration);
      }
    }

    private async Task<Domain> BuildDomainAsync(string version, DomainUpgradeMode upgradeMode)
    {
      var ns = typeof(Person).Namespace;
      var nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        var domain = await Domain.BuildAsync(configuration);
        return domain;
      }
    }
  }
}