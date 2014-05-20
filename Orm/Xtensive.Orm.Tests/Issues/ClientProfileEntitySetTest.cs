// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.04

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.ClientProfileEntitySetTestModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace ClientProfileEntitySetTestModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public EntitySet<Item> Items { get; private set; }

      public Owner(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Item : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      public Item(Session session)
        : base(session)
      {
      }
    }
  }

  public class ClientProfileEntitySetTest : AutoBuildTest
  {
    private Key ownerKey;
    private Key itemKey;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      configuration.Sessions.Default.Options = SessionOptions.ClientProfile1;
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      CheckRequirements();
    }

    public override void TestFixtureTearDown()
    {
    }

    [SetUp]
    public void SetUp()
    {
      RebuildDomain();

      using (var session = Domain.OpenSession()) {
        var owner = new Owner(session);
        var item = new Item(session);

        session.SaveChanges();

        ownerKey = owner.Key;
        itemKey = item.Key;
      }
    }

    [TearDown]
    public void TearDown()
    {
      if (Domain==null)
        return;
      Domain.Dispose();
      Domain = null;
    }

    [Test]
    public void ClearAndAddWithPreviouslyUsedItemTest()
    {
      using (var session = Domain.OpenSession()) {
        var owner = GetOwner(session);
        var item = GetItem(session);
        owner.Items.Add(item);
        session.SaveChanges();
      }

      using (var session = Domain.OpenSession()) {
        var owner = GetOwner(session);
        var item = GetItem(session);

        owner.Items.Clear();
        owner.Items.Add(item);

        session.SaveChanges();
      }
    }

    [Test]
    public void AddClearAddClearWithPreviouslyNonUsedItem()
    {
      using (var session = Domain.OpenSession()) {
        var owner = GetOwner(session);
        var item = GetItem(session);

        owner.Items.Add(item);
        owner.Items.Clear();
        owner.Items.Add(item);
        owner.Items.Clear();

        session.SaveChanges();
      }
    }

    private Owner GetOwner(Session session)
    {
      return session.Query.Single<Owner>(ownerKey);
    }

    private Item GetItem(Session session)
    {
      return session.Query.Single<Item>(itemKey);
    }
  }
}