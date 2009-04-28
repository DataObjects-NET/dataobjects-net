// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;

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


    private Domain BuildDomain(SchemaUpgradeMode schemaUpgradeMode, params Type[] persistentTypes)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (Type type in persistentTypes)
        configuration.Types.Register(type);

      return DomainBuilder.BuildDomain(configuration, schemaUpgradeMode);     
    }

    [Test]
    public void   TestBuilder()
    {
      Domain domain = BuildDomain(SchemaUpgradeMode.Recreate, typeof (B));
      using (var s = domain.OpenSession())
      using(var t = Transaction.Open()){
        new B();
        t.Complete();
      }
//      int bId = domain.Model.Types[typeof (B)].TypeId;

      domain = BuildDomain(SchemaUpgradeMode.Upgrade, typeof (A), typeof (B));
      using (var s = domain.OpenSession())
      using(var t = Transaction.Open()){
        new A();
        new B();
        t.Complete();
      }
      using (var s = domain.OpenSession())
      using(var t = Transaction.Open()){
        Assert.AreEqual(2, Query<B>.All.Count());
        Assert.AreEqual(1, Query<A>.All.Count());
      }
//      Assert.AreEqual(bId, domain.Model.Types[typeof (B)].TypeId);
    }


    


  }
}