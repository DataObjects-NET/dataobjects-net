// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.28

using System.Linq;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Upgrade.TypeIdUpgrade
{
  [TestFixture]
  public class Single2ManyClassesTest
  {
    [Test]
    public void CombinedTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(Model1.Person));
      var domain = Domain.Build(configuration);

      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        var person = new Model1.Person() {
                       FirstName = "Alex", LastName = "Kochetov"
                     };
        t.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof(Model2.Person));
      configuration.Types.Register(typeof(Model2.Employee));
      domain = Domain.Build(configuration);

      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        var person = new Model2.Person() {
                       FirstName = "Alex", LastName = "Gamzov"
                     };
        var employee = new Model2.Employee() {
                         FirstName = "Dmitri", LastName = "Maximov"
                       };
        var count = Query.All<Model2.Person>().Count();
        Assert.AreEqual(3, count);
        t.Complete();
      }
    }
  }
}