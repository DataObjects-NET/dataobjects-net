// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.27

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class DomainBuilderTest : UpgradeTestBase
  {
    [HierarchyRoot(typeof(KeyGenerator), "Id")]
    private class A : Entity
    {
      [Field]
      public int Id { get; private set; }
    }

    [HierarchyRoot(typeof(KeyGenerator), "Id")]
    private class B : Entity
    {
      [Field]
      public int Id { get; private set; }
    }

    [HierarchyRoot(typeof(KeyGenerator), "Id")]    
    private class C : Entity
    {
      [Field]
      public int Id { get; private set; }
    }
    
    private int GetTypeId(Type type)
    {
      return Domain.Model.Types[type].TypeId;
    }

    [Test]
    public void TypesTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof (B));
      int bId = GetTypeId(typeof (B));

      BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A), typeof (B));
      Assert.AreEqual(bId, GetTypeId(typeof (B)));
      int aId = GetTypeId(typeof (A));

      Assert.AreEqual(TypeInfo.MinTypeId, bId);
      Assert.AreEqual(TypeInfo.MinTypeId + 1, aId);

      BuildDomain(SchemaUpgradeMode.Validate, typeof (A));
      BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B));

      AssertEx.Throws<AggregateException>(() =>
        BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B), typeof (C)));

      BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A));

      Assert.AreEqual(aId, GetTypeId(typeof (A)));

      AssertEx.Throws<AggregateException>(() =>
        BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B)));

      BuildDomain(SchemaUpgradeMode.Recreate, typeof(A), typeof (B));
      AssertEx.Throws<AggregateException>(() =>
        BuildDomain(SchemaUpgradeMode.SafeUpgrade, typeof (A)));
    }

    [Test]
    public void InstancesTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof (A));

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          A a = new A();
          transactionScope.Complete();
        }
      }

      BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A));

      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          Assert.AreEqual(1, Query<A>.All.Count());

          foreach (A a in Query<A>.All) {
            Assert.IsNotNull(a);  
          }
        }
      }
    }
  }
}