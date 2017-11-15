// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.28

using System.Linq;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.TypeIdUpgrade
{
  [TestFixture]
  public class Single2ManyClassesTest
  {
    [Test]
    public void CombinedTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model.Person));
      var domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var person = new Model.Person() {
                       FirstName = "Alex", LastName = "Kochetov"
                     };
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof(Model.Person));
      configuration.Types.Register(typeof(Model.Employee));
      domain = Domain.Build(configuration);

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var person = new Model.Person() {
                       FirstName = "Alex", LastName = "Gamzov"
                     };
        var employee = new Model.Employee() {
                         FirstName = "Dmitri", LastName = "Maximov"
                       };
        var count = session.Query.All<Model.Person>().Count();
        Assert.AreEqual(3, count);
        var list = session.Query.All<Model.Person>().ToList();
        Assert.AreEqual(3, list.Count);
        foreach (var item in list)
          Assert.IsNotNull(item);
        t.Complete();
      }
    }
  }
}