// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using M1 = Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRules.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRules.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRules
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (domain.OpenSession()) {
        using (var tx = Session.Current.OpenTransaction()) {
          var person = new M1.Person();
          person.Friend = person;
          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (Session.Current.OpenTransaction()) {
          var person = Query.All<M2.Person>().SingleOrDefault();
          Assert.NotNull(person);
          Assert.AreSame(person, person.NewFriend);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));
      configuration.NamingConvention.NamingRules = NamingRules.UnderscoreDots;

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}