// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using System;

namespace Xtensive.Storage.Tests.Storage.DomainBuild
{

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class A : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Col1 { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class B : Entity
  {
    [Field]
    public Guid Id { get; private set; }

    [Field]
    public string Col1 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  using DomainBuild;

  [TestFixture]
  public class DomainBuildTest
  {
    private Domain Domain { get; set; }
    
    private void BuildDomain(DomainUpgradeMode upgradeMode)
    {
      if (Domain != null)
        Domain.DisposeSafely();

      var config = DomainConfigurationFactory.Create("mssql2005");
      config.UpgradeMode = upgradeMode;
      config.Types.Register(typeof (A).Assembly, typeof (A).Namespace);
      
      Domain = Domain.Build(config);
    }
      
    [Test]
    public void DomainRecreateTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          for (var i = 0; i < 10; i++) {
            var a = new A {Col1 = i.ToString()};
            var b = new B {Col1 = i.ToString()};
          }
          transaction.Complete();
        }
      }
    }

    [Test]
    public void DomainBlockUpgradeTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          for (var i = 0; i < 129; i++) {
            var a = new A {Col1 = i.ToString()};
          }
          transaction.Complete();
        }
      }
      BuildDomain(DomainUpgradeMode.Validate);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          var products = Query<A>.All;
          var result =
            from a in products
            select a;
          Assert.AreEqual(129, result.Count());
          for (var i = 0; i < 10; i++) {
            var a = new A {Col1 = i.ToString()};  
          }
          Assert.AreEqual(139, result.Count());
          transaction.Complete();
        }
      }
    }
  }
}