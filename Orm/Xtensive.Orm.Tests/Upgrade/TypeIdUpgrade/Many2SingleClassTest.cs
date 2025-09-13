// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.12.29

using System.Linq;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.TypeIdUpgrade
{
  [TestFixture]
  public class Many2SingleClassTest
  {
    [Test]
    [IgnoreOnGithubActionsIfFailed(StorageProvider.Firebird)]
    public void CombinedTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new Model.Person() {
          FirstName = "Alex", 
          LastName = "Kochetov"
        };
        _ = new Model.Person() {
          FirstName = "Alex",
          LastName = "Gamzov"
        };
        _ = new Model.Employee() {
          FirstName = "Dmitri",
          LastName = "Maximov"
        };
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable())
        domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Model.Person>().Count();
        Assert.AreEqual(2, count);
        var list = session.Query.All<Model.Person>().ToList();
        Assert.AreEqual(2, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }
  }
}