// Copyright (C) 2010-2025 Xtensive LLC.
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

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var person = new M1.Person();
          var address = new M1.Address() {
            Person = person
          };
          // person.Addresses.Add(address);
          tx.Complete();
        }
      }
    }
    
    [Test]
    [IgnoreOnGithubActionsIfFailed(StorageProvider.Firebird)]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<M2.Person>().Count());
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}