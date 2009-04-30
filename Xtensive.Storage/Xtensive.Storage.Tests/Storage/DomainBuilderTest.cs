// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.27

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage
{  
  [TestFixture]
  public class DomainBuilderTest
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

    private Domain domain;

    private int GetTypeId(Type type)
    {
      return domain.Model.Types[type].TypeId;
    }

    private void BuildDomain(SchemaUpgradeMode schemaUpgradeMode, params Type[] persistentTypes)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (Type type in persistentTypes)
        configuration.Types.Register(type);

      domain = DomainBuilder.BuildDomain(configuration, schemaUpgradeMode);
    }

    [Test]
    public void TypesTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof (B));
      int bId = GetTypeId(typeof (B));
      
      BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A), typeof (B));
      Assert.AreEqual(bId, GetTypeId(typeof(B)));
      int aId = GetTypeId(typeof (A));

      Assert.AreEqual(TypeInfo.MinTypeId, bId);
      Assert.AreEqual(TypeInfo.MinTypeId+1, aId);

      // Temporary is not implemented
      AssertEx.Throws<NotImplementedException>(() => {

        BuildDomain(SchemaUpgradeMode.Validate, typeof (A));
        BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B));

        AssertEx.Throws<Exception>(() =>
          BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B), typeof (C)));

        BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A));

        Assert.AreEqual(aId, GetTypeId(typeof (A)));

        AssertEx.Throws<Exception>(() =>
          BuildDomain(SchemaUpgradeMode.Validate, typeof (A), typeof (B)));
      });
    }

    [Test]
    public void InstancesTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof (A));

      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          A a = new A();
          transactionScope.Complete();
        }
      }

      BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A));

      using (domain.OpenSession()) {
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