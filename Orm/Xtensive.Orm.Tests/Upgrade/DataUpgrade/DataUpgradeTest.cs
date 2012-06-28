// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1;
using D = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2.D;
using M1 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.DataUpgrade
{
  [TestFixture, Category("Upgrade")]
  public class DataUpgradeTest
  {
    private Domain domain;
    private StoredDomainModel storedModel;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      storedModel = domain.Model.ToStoredModel();
      storedModel.UpdateReferences();
      FillData();
    }
    
    [Test]
    public void ClearDataTest1()
    {
      BuildDomain(DomainUpgradeMode.Perform, typeof (A));
      using (var s = domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          Assert.AreEqual(1, s.Query.All<A>().Count());
        }
      }
    }

    [Test]
    public void ClearDataTest2()
    {
      BuildDomain(DomainUpgradeMode.Perform, typeof (A), typeof (B));
      using (var s = domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          Assert.AreEqual(2, s.Query.All<A>().Count());
        }
      }
    }
    
    [Test]
    public void ClearDataTest3()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var s = domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          Assert.AreEqual(4, s.Query.All<Model.Version2.A>().Count());
          var firstD = s.Query.All<D>().First();
          Assert.AreEqual(2, firstD.RefA.Count());
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version" + version);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    private void BuildDomain(DomainUpgradeMode upgradeMode, params Type[] types)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types)
        configuration.Types.Register(type);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable("1")) {
        domain = Domain.Build(configuration);
      }
    }

    private void FillData()
    {
      using (var s = domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          var a1 = new A();
          var b1 = new B();
          var c1 = new C();
          var c2 = new C();
          var d1 = new Model.Version1.D();
          c1.RefA = a1;
          c1.RefB = b1;
          c2.RefA = b1;
          d1.RefA.Add(a1);
          d1.RefA.Add(b1);
          d1.RefA.Add(c1);
          t.Complete();
        }
      }
    }
  }
}