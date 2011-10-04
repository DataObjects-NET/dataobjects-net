// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.02

using System;
using System.Linq;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueA413_KeyNotFoundException_Model;

namespace Xtensive.Storage.Tests.Issues
{
  namespace IssueA413_KeyNotFoundException_Model
  {
    [HierarchyRoot]
    public class A : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public B B { get; set; }

      [Field]
      public string Tag { get; set; }
    }

    [HierarchyRoot]
    public class B : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public IItem Item { get; set; }
    }

    public interface IItem : IEntity
    {
      [Field]
      string Name { get; set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public long Id { get; private set; }

      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class My : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyStatus Status { get; set; }  // Enum
    }

    public enum MyStatus
    {
      None,
      Open,
      Closed
    }

    public class MyEntityViewModel
    {
      public string Status { get; set; }

      public MyEntityViewModel(MyStatus status)
      {
        Status = status.ToString();
      }
    }

    /*public class MyEntity : Entity {
      [Field]
      public MyStatus Status { get; set; }  // Enum
    }

    

    ...
    var x = from e in Session.Query.All<MyEntity>() select new MyEntitiyViewModel(e.Status)*/
  }

  [Serializable]
  public class IssueA413_KeyNotFoundException : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IItem).Assembly, typeof (IItem).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new A {Tag = "Alpha", B = new B {Item = new Item {Name = "Item name"}}};
        t.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) { 
        var query = 
          from a in Query.All<A>()
          where a.Tag == "Alpha"
          group a by a.B into g
          select new { g.Key.Item.Name, Count = g.Count() };
        var list = query.ToList();
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("Item name", list.Single().Name);
        Assert.AreEqual(1, list.Single().Count);

        t.Complete();
      }
    }

    [Test]
    public void EnumTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new My {Status = MyStatus.Closed};
        t.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var x = from e in Query.All<My>()
                select new MyEntityViewModel(e.Status);
        var list = x.ToList();
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual("Closed", list.Single().Status);
        t.Complete();
      }
      
    }
  }
}