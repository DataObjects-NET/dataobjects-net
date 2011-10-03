// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.19

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using M1 = Xtensive.Storage.Tests.Issues.Issue_Jira0180_ChangeNullabilityViaUpgradeHints.Model.Version1;
using M2 = Xtensive.Storage.Tests.Issues.Issue_Jira0180_ChangeNullabilityViaUpgradeHints.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Issues.Issue_Jira0180_ChangeNullabilityViaUpgradeHints
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

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (Session.Open(domain)) {
        using (var tx = Transaction.Open()) {
          var person = new M1.Person {
            Name = "Vasya",
            Weight = 80,
            Phone = new M1.Phone(),
          };
          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.PerformSafely);
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          var vasya = Query.All<M2.Person>().Single();
          Assert.AreEqual("Vasya", vasya.Name);
          Assert.AreEqual(80, vasya.Weight);
          Assert.IsNotNull(vasya.Phone);
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

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}