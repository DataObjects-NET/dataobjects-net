// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.15

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0017_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0017_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Master:Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field, Association(PairTo = "Master1")]
    public Slave Slave { get; set; }

    [Field, Association(PairTo = "Master2")]
    public EntitySet<Slave> Slaves { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Slave:Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public Master Master1 { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public Master Master2 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0017_ReferentialActionCascade : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Master).Assembly, typeof (Master).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var m1 = new Master();
          var s1 = new Slave();
          m1.Slave = s1;
          Assert.AreEqual(m1, s1.Master1);
          m1.Remove();
          Assert.AreEqual(PersistenceState.Removed, m1.PersistenceState);
          Assert.AreEqual(PersistenceState.Removed, s1.PersistenceState);
          Session.Current.Persist();
          // Rollback
        }
      }
    }
  }
}