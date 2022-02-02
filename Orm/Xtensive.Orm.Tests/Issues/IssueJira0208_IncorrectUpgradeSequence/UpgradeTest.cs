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
using M1 = Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence
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
        var toRemove1 = new M1.EntityToRemove1();
        var toKeep1 = new M1.EntityToKeep1(toRemove1);
        var toRemove2 = new M1.EntityToRemove2();
        var toKeep2 = new M1.EntityToKeep2(toRemove2);
        _ = new M1.VeryUniqueEntity();
        _ = new M1.VeryUniqueEntity();
        tx.Complete();
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      using (var domain = BuildDomain("2", DomainUpgradeMode.PerformSafely))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var toKeep1 = session.Query.All<M2.EntityToKeep1>().Single();
        var toKeep2 = session.Query.All<M2.EntityToKeep2>().Single();
        Assert.AreEqual(2, session.Query.All<M2.VeryUniqueEntity>().Count());
      }
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var ns = typeof(M1.EntityToKeep1).Namespace;
      var nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        return Domain.Build(configuration);
      }
    }
  }
}