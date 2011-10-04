// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.12

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0737_PersistentFieldState_Model;
using System.Linq;

namespace Xtensive.Storage.Tests.Issues
{
  namespace Issue0737_PersistentFieldState_Model
  {
    [HierarchyRoot]
    public class Derived : Entity
    {
      [Key,Field]
      public int Id { get; private set; }

      [Field]
      public string State { get; set; }
    }

    [HierarchyRoot, Serializable]
    [TableMapping("States")]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class State : Entity
    {
      [Key]
      [Field]
      public string ID { get; private set; }

      [Field]
      [Association(PairTo = "AbiturAd.State")]
      public EntitySet<AbiturAd> AbiturAds { get; set; }
    }

    [HierarchyRoot, Serializable]
    [TableMapping("AbiturAds")]
    public class AbiturAd : Entity
    {
      [Key]
      [Field]
      public int ID { get; private set; }

      [Field]
      [FieldMapping("StateID")]
      public State State { get; set; }
    }
  }

  [Serializable]
  public class Issue0737_PersistentFieldState : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Derived).Assembly, typeof(Derived).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var derived = new Derived() {State = "new"};

        var list = Query.All<Derived>()
          .Where(d => d.State == "new")
          .ToList();
      }
    }
  }
}