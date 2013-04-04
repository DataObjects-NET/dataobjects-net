// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.13

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0433_CompiledQueryOverEntitySetModel;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0433_CompiledQueryOverEntitySetModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }
  }

  public class IssueJira0433_CompiledQueryOverEntitySet : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    [Test]
    public void CompiledQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx =session.OpenTransaction()) {
        var owner1 = new Owner();
        var owner2 = new Owner();
        owner1.Items.Add(new Item());
        owner1.Items.Add(new Item());

        Assert.That(HasItems(session, owner1));
        Assert.That(HasItems(session, owner2), Is.False);
      }
    }

    [Test]
    public void InvalidCompiledQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var owner1 = new Owner();
        AssertEx.Throws<QueryTranslationException>(() => HasItemsInvalid(session, owner1));
      }
    }

    [Test]
    public void RegularQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx =session.OpenTransaction()) {
        var owner1 = new Owner();
        var owner2 = new Owner();
        owner1.Items.Add(new Item());
        owner1.Items.Add(new Item());

        Assert.That(session.Query.Items(() => owner1.Items).Any());
        Assert.That(session.Query.Items(() => owner2.Items).Any(), Is.False);
      }
    }

    [Test]
    public void SubqueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var owner1 = new Owner();
        var owner2 = new Owner();
        owner1.Items.Add(new Item());
        owner1.Items.Add(new Item());

        Assert.That(GetOwnersWithItemsDirect(session).Single(), Is.EqualTo(owner1));
        Assert.That(GetOwnersWithItemsWrapped(session).Single(), Is.EqualTo(owner1));

        var ownersWithItemsDirect = session.Query
          .All<Owner>()
          .Where(o => o.Items.Any())
          .AsEnumerable();
        Assert.That(ownersWithItemsDirect.Single(), Is.EqualTo(owner1));

        var ownersWithItemsWrapped = session.Query
          .All<Owner>()
          .Where(o => session.Query.Items(() => o.Items).Any())
          .AsEnumerable();
        Assert.That(ownersWithItemsWrapped.Single(), Is.EqualTo(owner1));
      }
    }

    public IEnumerable<Owner> GetOwnersWithItemsDirect(Session session)
    {
      return session.Query.Execute(q => q.All<Owner>().Where(o => o.Items.Any()));
    }

    public IEnumerable<Owner> GetOwnersWithItemsWrapped(Session session)
    {
      return session.Query.Execute(q => q.All<Owner>().Where(o => q.Items(() => o.Items).Any()));
    }

    public bool HasItems(Session session, Owner owner)
    {
      return session.Query.Execute(q => q.Items(() => owner.Items).Any());
    }

    public bool HasItemsInvalid(Session session, Owner owner)
    {
      return session.Query.Execute(q => owner.Items.Any());
    }
  }
}