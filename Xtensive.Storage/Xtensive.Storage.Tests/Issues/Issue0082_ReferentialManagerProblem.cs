// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0082_ReferentialManagerProblem_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0082_ReferentialManagerProblem_Model
{
  [HierarchyRoot]
  public class Ancestor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

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

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          new Descendant {StringField = "1",};
          new Descendant {StringField = "2",};
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var allD = from d in Query.All<Descendant>() select d;
          foreach (var d in allD)
            d.Remove();
          t.Complete();
        }
      }
    }
  }
}