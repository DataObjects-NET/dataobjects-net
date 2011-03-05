// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Providers;
using M1 = Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug2.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug2.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.Issue_0841_HintGeneratorBug2
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.UpdateFrom);
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var a = new M1.Base();
          var b = new M1.Derived() { Text = "B" };
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
          var a = session.Query.All<M2.Base>().SingleOrDefault(e => e.Text==null);
          var b = session.Query.All<M2.Base>().SingleOrDefault(e => e.Text!=null);
          Assert.IsNotNull(a);
          Assert.IsNotNull(b);
          Assert.AreEqual("B", b.Text);
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