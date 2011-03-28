// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using M1 = Xtensive.Orm.Tests.Issues.Issue_0834_HintGeneratorBug.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.Issue_0834_HintGeneratorBug.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.Issue_0834_HintGeneratorBug
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
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var a = new M1.Base() { BaseTitle = "a.Base"};
          var b = new M1.Derived() { BaseTitle = "b.Base", DerivedTitle = "b.Derived" };
          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var a = session.Query.All<M2.Base>().SingleOrDefault();
          Assert.IsNotNull(a);
          Assert.AreEqual("a.Base", a.BaseTitle);

          var b = session.Query.All<M2.Derived>().SingleOrDefault();
          Assert.IsNotNull(b);
          Assert.AreEqual("b.Base", b.BaseTitle);
          Assert.AreEqual("b.Derived", b.DerivedTitle);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.Base).Namespace;
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