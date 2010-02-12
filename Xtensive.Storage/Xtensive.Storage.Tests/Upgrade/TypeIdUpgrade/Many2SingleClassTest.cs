// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.29

using System.Linq;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Upgrade.TypeIdUpgrade
{
  [TestFixture]
  public class Many2SingleClassTest
  {
    [TestFixtureSetUp]
    public void SetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [Test]
    public void CombinedTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      var domain = Domain.Build(configuration);

      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
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

      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        var count = Query.All<Model.Person>().Count();
        Assert.AreEqual(2, count);
        var list = Query.All<Model.Person>().ToList();
        Assert.AreEqual(2, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }
  }
}