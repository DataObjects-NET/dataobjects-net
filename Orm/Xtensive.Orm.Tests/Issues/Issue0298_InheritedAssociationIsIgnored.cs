// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: a
// Created:    a

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Tests;
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
      config.Types.RegisterCaching(typeof (Master).Assembly, typeof (Master).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var am = new AudioMaster();
          _ = am.Tracks.Add(new MasterTrack());
          _ = am.Tracks.Add(new MasterTrack());

          Assert.That(session.Query.All<AudioMaster>().Count(), Is.EqualTo(1));
          Assert.That(session.Query.All<MasterTrack>().Count(), Is.EqualTo(2));

          AssertEx.Throws<ReferentialIntegrityException>(() => am.Tracks.First().Remove());

          am.Remove();

          Assert.That(session.Query.All<AudioMaster>().Count(), Is.EqualTo(0));
          Assert.That(session.Query.All<MasterTrack>().Count(), Is.EqualTo(0));
          // Rollback
        }
      }
    }

    [Test]
    public void ClientPrifileTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()){

        var am = new AudioMaster();
        _ = am.Tracks.Add(new MasterTrack());
        _ = am.Tracks.Add(new MasterTrack());

        AssertEx.Throws<ReferentialIntegrityException>(() => ((IEnumerable<MasterTrack>) am.Tracks).First().Remove());

        am.Remove();
        // Rollback
      }
    }
  }
}