// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.08

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;

namespace Xtensive.Storage.Tests.Storage.DomainBuild
{

  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class A : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Col1 { get; set; }
  }

}

namespace Xtensive.Storage.Tests.Storage
{
  using DomainBuild;
  using Core.Testing;
  using Core;

  [TestFixture]
  public class DomainBuildTest
  {
    private Domain Domain { get; set; }
    
    private void BuildDomain(DomainBuildMode buildMode, bool clearSchema)
    {
      if (Domain != null)
        Domain.DisposeSafely();

      var config = DomainConfigurationFactory.Create("mssql2005");
      config.BuildMode = buildMode;
      if (!clearSchema)
        config.Types.Register(typeof(A).Assembly, typeof(A).Namespace);

      Domain = Domain.Build(config);
    }


    [Test]
    public void DomainRecreateTest()
    {
      BuildDomain(DomainBuildMode.Recreate, false);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          for (int i = 0; i < 10; i++) {
            var a = new A();
            a.Col1 = i.ToString();
            transaction.Complete();
          }
        }
      }
    }

    [Test]
    public void DomainBlockUpgradeTest()
    {
      BuildDomain(DomainBuildMode.Recreate, false);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          for (int i = 0; i < 129; i++) {
            var a = new A();
            a.Col1 = i.ToString();
            transaction.Complete();
          }
        }
      }
      BuildDomain(DomainBuildMode.BlockUpgrade, false);
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.Session.OpenTransaction()) {
          var products = Query<A>.All;
          var result =
            from a in products
            select a;
          Assert.AreEqual(129, result.Count());
          for (int i = 0; i < 10; i++) {
            var a = new A();
            a.Col1 = i.ToString();
            transaction.Complete();
          }
          Assert.AreEqual(139, result.Count());
        }
      }
    }

    [Test]
    public void DenyBlockUpgradeTest()
    {
      BuildDomain(DomainBuildMode.Recreate, true);
      AssertEx.Throws<AggregateException>(() => BuildDomain(DomainBuildMode.BlockUpgrade, false));
    }

  }

  
}