// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using M1 = Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version1;
using M2 = Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade.DataUpgrade
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
      BuildDomain(DomainUpgradeMode.Perform, typeof (M1.A));
      using (var s = Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(1, Query.All<M1.A>().Count());
        }
      }
    }

    [Test]
    public void ClearDataTest2()
    {
      BuildDomain(DomainUpgradeMode.Perform, typeof (M1.A), typeof (M1.B));
      using (var s = Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(2, Query.All<M1.A>().Count());
        }
      }
    }
    
    [Test]
    public void ClearDataTest3()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var s = Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(4, Query.All<M2.A>().Count());
          var firstD = Query.All<M2.D>().First();
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
        "Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version" + version);
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
      using (Upgrader.Enable("1")) {
        domain = Domain.Build(configuration);
      }
    }

    private void FillData()
    {
      using (var s = Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          var a1 = new M1.A();
          var b1 = new M1.B();
          var c1 = new M1.C();
          var c2 = new M1.C();
          var d1 = new M1.D();
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