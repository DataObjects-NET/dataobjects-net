// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.19

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using M1 = Xtensive.Orm.Tests.Issues.IssueJira0180_ChangeNullabilityViaUpgradeHints.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.IssueJira0180_ChangeNullabilityViaUpgradeHints.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.IssueJira0180_ChangeNullabilityViaUpgradeHints
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [Test]
    public void NoneTest()
    {
      RunTest(NamingRules.None);
    }

    [Test]
    public void UnderscoreDotsTest()
    {
      RunTest(NamingRules.UnderscoreDots);
    }

    [Test]
    public void UnderscoreHyphensTest()
    {
      RunTest(NamingRules.UnderscoreHyphens);
    }

    [Test]
    public void UnderscoreDotsAndHyphensTest()
    {
      RunTest(NamingRules.UnderscoreDots | NamingRules.UnderscoreHyphens);
    }

    private void RunTest(NamingRules namingRules)
    {
      BuildDomain("1", DomainUpgradeMode.Recreate, namingRules);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var person = new M1.Person {
            Name = "Vasya",
            Weight = 80,
            Age = 20,
            Phone = new M1.Phone(),
          };
          tx.Complete();
        }
      }
      BuildDomain("2", DomainUpgradeMode.PerformSafely, namingRules);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var vasya = session.Query.All<Model.Version2.Person>().Single();
          Assert.AreEqual("Vasya", vasya.Name);
          Assert.AreEqual(80, vasya.Weight);
          Assert.IsNotNull(vasya.Phone);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode, NamingRules namingRules)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));
      configuration.NamingConvention.NamingRules = namingRules;

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}