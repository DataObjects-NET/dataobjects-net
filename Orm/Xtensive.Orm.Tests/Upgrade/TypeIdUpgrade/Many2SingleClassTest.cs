// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public void CombinedTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Model.Person() {
          FirstName = "Alex", 
          LastName = "Kochetov"
        };
        new Model.Person() {
          FirstName = "Alex",
          LastName = "Gamzov"
        };
        new Model.Employee() {
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