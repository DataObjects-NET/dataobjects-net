// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: a
// Created:    a

using System;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0298_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues.Issue0298_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association("Master", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Deny)]
    public EntitySet<MasterTrack> Tracks { get; private set; }
  }

  [Serializable]
  public class AudioMaster : Master
  {
  }

  [Serializable]
  [HierarchyRoot]
  public class MasterTrack : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Master Master { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0298_InheritedAssociationIsIgnored : AutoBuildTest
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var am = new AudioMaster();
          am.Tracks.Add(new MasterTrack());
          am.Tracks.Add(new MasterTrack());

          Assert.AreEqual(1, session.Query.All<AudioMaster>().Count());
          Assert.AreEqual(2, session.Query.All<MasterTrack>().Count());

          AssertEx.Throws<ReferentialIntegrityException>(() => am.Tracks.First().Remove());

          am.Remove();

          Assert.AreEqual(0, session.Query.All<AudioMaster>().Count());
          Assert.AreEqual(0, session.Query.All<MasterTrack>().Count());
          // Rollback
        }
      }
    }
  }
}