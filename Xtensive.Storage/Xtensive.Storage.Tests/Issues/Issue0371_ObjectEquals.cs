// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.02

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Tests.Issues.Issue0371_ObjectEquals_Model;
using System.Linq;
using Xtensive.Storage.Tests.Linq;

namespace Xtensive.Storage.Tests.Issues.Issue0371_ObjectEquals_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Item : Entity
  {
    [Field][Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; private set; }

    public static bool Equals()
    {
      return true;
    }

    public new static bool Equals(object a, object b)
    {
      return true;
    }

    public new static bool Equals(Item a, Item b)
    {
      return true;
    }

    public Item()
    {
      Name = Guid.NewGuid().ToString();
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0371_ObjectEquals : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Item).Assembly, typeof (Item).Namespace);
      return config;
    }

    [Test]
    public void ObjectEqualsTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var item1 = new Item();
          var item2 = new Item();
          var result = Query.All<Item>().Where(item => Equals(item, item1));
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }

    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void ItemEquals1Test()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var item1 = new Item();
          var item2 = new Item();
          var result = Query.All<Item>().Where(item => Item.Equals((object)item, (object)item1));
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }

    [Test]
    public void ItemEquals2Test()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var item1 = new Item();
          var item2 = new Item();
          var result = Query.All<Item>().Where(item => Item.Equals(item, item1));
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }

    [Test]
    public void ClassEqualsTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var item1 = new Item();
          var item2 = new Item();
          var result = Query.All<Item>().Where(item => String.Equals(item.Name, item1.Name));
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }

    [Test]
    public void StringEqualsTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var item1 = new Item();
          var item2 = new Item();
          var result = Query.All<Item>().Where(item => item.Equals(item1));
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }
  }
}