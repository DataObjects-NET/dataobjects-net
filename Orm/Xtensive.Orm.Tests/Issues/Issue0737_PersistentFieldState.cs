// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.12

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0737_PersistentFieldState_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0737_PersistentFieldState_Model
  {
    [HierarchyRoot]
    public class Derived : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public new string State { get; set; }
    }

    [HierarchyRoot]
    [TableMapping("States")]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class State : Entity
    {
      [Key]
      [Field(Length = 100)]
      public string ID { get; private set; }

      [Field]
      [Association(PairTo = "State")]
      public EntitySet<AbiturAd> AbiturAds { get; set; }
    }

    [HierarchyRoot]
    [TableMapping("AbiturAds")]
    public class AbiturAd : Entity
    {
      [Key]
      [Field]
      public int ID { get; private set; }

      [Field]
      [FieldMapping("StateID")]
      public new State State { get; set; }
    }
  }

  public class Issue0737_PersistentFieldState : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(Derived).Assembly, typeof(Derived).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var derived = new Derived() { State = "new" };

        var list = session.Query.All<Derived>()
          .Where(d => d.State == "new")
          .ToList();
      }
    }
  }
}
