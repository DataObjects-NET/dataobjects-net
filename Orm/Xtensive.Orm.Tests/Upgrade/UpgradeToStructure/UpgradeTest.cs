// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using M1 = Xtensive.Orm.Tests.Upgrade.UpgradeToStructure.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.UpgradeToStructure.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeToStructure
{
  [TestFixture]
  [Explicit("This functionality was requested internally + there is a workaround with custom upgrade handler.")]
  public class UpgradeTest
  {
    [SetUp]
    public void SetUp()
    {
      using (var domain = BuildDomain("1", DomainUpgradeMode.Recreate))
      using (domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {
        var person = new M1.Person { Title = "Alex" };
        tx.Complete();
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person = session.Query.All<M2.Person>().SingleOrDefault();
        Assert.NotNull(person);
        Assert.AreEqual("Alex", person.Info.Title);
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var ns = typeof(M1.Person).Namespace;
      var nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        return Domain.Build(configuration);
      }
    }
  }
}