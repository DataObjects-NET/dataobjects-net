// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Issues.Issue0624_EntitySetSubqueryError_Model;
using Xtensive.Storage.Tests.Issues.Issue0626_SequenceContainsNoElements_Model;

namespace Xtensive.Storage.Tests.Issues
{
  namespace Issue0626_SequenceContainsNoElements_Model
  {
    public abstract class EntityBase : Entity
    {
      [Field, Key]
      public Guid Id { get; private set; }

      protected EntityBase(Guid id)
        : base(id)
      {}
    }

    [HierarchyRoot]
    class Person : EntityBase
    {
      public Person(Guid id)
        : base(id)
      {}

      /// <summary>
      /// Gets or sets Rank.
      /// </summary>
      [Field]
      public int Rank { get; set; }
    }

    [HierarchyRoot]
    class Position : EntityBase
    {
      public Position(Guid id)
        : base(id)
      {}

      /// <summary>
      /// Gets or sets Rank.
      /// </summary>
      [Field]
      public int Rank { get; set; }
    }
  }

  [Serializable]
  public class Issue0626_SequenceContainsNoElements : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Position).Assembly, typeof(Position).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var person = new Person(Guid.NewGuid()) {Rank = 1};
        var position = new Position(Guid.NewGuid()) {Rank = 1};
//        session.Persist();

        var rank = Query.All<Position>().Where(r => r.Rank == 1).Single().Rank; // Ok here
        Assert.AreEqual(1, rank);
        var people = Query.All<Person>()
          .Where(p => p.Rank == Query.All<Position>().Where(r => r.Rank == 1).Single().Rank)
          .ToList(); //Exception
        Assert.AreEqual(1, people.Count);
        t.Complete();
      }  
    }
  }
}