// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA363_WrongInnerJoin_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA363_WrongInnerJoin_Model
  {
    [Serializable]
    [HierarchyRoot]
    public abstract class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class SomeEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    public class MyEntityWithLink : MyEntity
    {
      [Field(Nullable = false)]
      public SomeEntity Link { get; set; }
    }

    public class MyEntityWithText : MyEntity
    {
      [Field]
      public string SomeData { get; set; }
    }
  }

  [Serializable]
  public class IssueA363_WrongInnerJoin : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var se1 = new SomeEntity {Text = "se1"};
        var se2 = new SomeEntity {Text = "se2"};

        var mie1 = new MyEntityWithLink {Link = se1, Text = "MyEntityWithLink"};
        var mie2 = new MyEntityWithLink {Link = se2, Text = "MyEntityWithLink"};
        var mihe2 = new MyEntityWithText {SomeData = "ololo", Text = "MyEntityWithText"};

        // if MyEntityWithLink.SomeEntity - decorated with [Field(Nullable = false)] - there are no MyEntityWithText data
        // if MyEntityWithLink.SomeEntity - decorated with [Field(Nullable = true)] - ok
        var items = Query.All<MyEntity>().OrderBy(a => a.Id)
          .Select(q => (q as MyEntityWithLink!=null)
            ? new {d = (q as MyEntityWithLink).Link.Text}
            : new {d = (q as MyEntityWithText).SomeData});
        var result = items.ToList();
        t.Complete();

        Assert.AreEqual(3, result.Count);
      }
    }
  }
}