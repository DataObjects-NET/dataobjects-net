// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0082_ReferentialManagerProblem_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0082_ReferentialManagerProblem_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Ancestor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  public class Descendant : Ancestor
  {
    [Field]
    public Descendant Ref1 { get; set; }

    [Field]
    public EntitySet<Descendant> Set1 { get; private set; }

    [Field, Association(PairTo = "Set3")]
    public EntitySet<Descendant> Set2 { get; private set; }

    [Field]
    public EntitySet<Descendant> Set3 { get; private set; }

    [Field]
    public String StringField { get; set; }

    [Field]
    public Descendant Ref2 { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0082_ReferentialManagerProblem : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Ancestor).Assembly, typeof (Ancestor).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new Descendant {StringField = "1",};
          new Descendant {StringField = "2",};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var allD = from d in session.Query.All<Descendant>() select d;
          foreach (var d in allD)
            d.Remove();
          t.Complete();
        }
      }
    }
  }
}