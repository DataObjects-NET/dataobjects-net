// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.01.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0759_UnableToTranslateOfTypeSelectExpressionModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0759_UnableToTranslateOfTypeSelectExpression : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var a = new TestEntity1 {CreationDate = DateTime.UtcNow.AddDays(-10)};
        var b = new TestEntity1 {CreationDate = DateTime.UtcNow.AddDays(-9)};
        var c = new TestEntity1 {CreationDate = DateTime.UtcNow.AddDays(-8)};

        var d = new TestEntity2 {CreationDate = DateTime.UtcNow.AddDays(-6), DirectlyDeclaredField = 10, Value = 100, Comment = "100"};
        var e = new TestEntity2 {CreationDate = DateTime.UtcNow.AddDays(-5), DirectlyDeclaredField = 11, Value = 101, Comment = "101"};
        var f = new TestEntity2 {CreationDate = DateTime.UtcNow.AddDays(-4), DirectlyDeclaredField = 12, Value = 102, Comment = "102"};

        var g = new TestEntity3 {CreationDate = DateTime.UtcNow.AddDays(-3), DirectlyDeclaredField = 20, Value = 200, Comment = "200", Field4 = 1.1f};
        var h = new TestEntity3 {CreationDate = DateTime.UtcNow.AddDays(-2), DirectlyDeclaredField = 21, Value = 201, Comment = "201", Field4 = 1.2f};
        var i = new TestEntity3 {CreationDate = DateTime.UtcNow.AddDays(-1), DirectlyDeclaredField = 22, Value = 202, Comment = "202", Field4 = 1.3f};
        var j = new TestEntity3 {CreationDate = DateTime.UtcNow.AddDays(-1), DirectlyDeclaredField = 22, Value = 202, Comment = "202", Field4 = 1.4f};

        g.Field5.Add(d);
        g.Field5.Add(h);
        h.Field5.Add(e);
        h.Field5.Add(i);
        i.Field5.Add(j);

        var status = new Status {Name = "test"};
        new TestEntity4 {TestField = "Test", Status = status};

        transaction.Complete();
      }
    }

    [Test]
    public void Test1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var serverSide = session.Query.All<TestEntity3>()
          .OfType<IHasValue>()
          .Select(x => x.Value + x.Comment).ToArray();
        var clientSide = session.Query.All<TestEntity3>()
          .OfType<IHasValue>().ToArray();

        Assert.That(serverSide.Length, Is.EqualTo(4));

        foreach (var item in clientSide)
          Assert.That(serverSide.Contains(item.Value + item.Comment));

        serverSide = session.Query.All<TestEntity2>()
          .OfType<IHasValue>()
          .Select(x => x.Value + x.Comment).ToArray();
        clientSide = session.Query.All<TestEntity2>()
          .OfType<IHasValue>().ToArray();
        Assert.That(serverSide.Length, Is.EqualTo(7));

        foreach (var item in clientSide)
          Assert.That(serverSide.Contains(item.Value + item.Comment));

        serverSide = session.Query.All<TestEntity1>()
          .OfType<IHasValue>()
          .Select(x => x.Value + x.Comment).ToArray();
        clientSide = session.Query.All<TestEntity3>()
          .OfType<IHasValue>().ToArray();

        Assert.That(serverSide.Length, Is.EqualTo(7));
        foreach (var item in clientSide) 
          Assert.That(serverSide.Contains(item.Value + item.Comment));
      }
    }

    [Test]
    public void Test2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var anyQuery = session.Query.All<TestEntity4>()
          .OfType<IHasStatus>()
          .Select(e => e.Status)
          .Any();
        Assert.That(anyQuery, Is.True);
      }
    }

    [Test]
    public void Test3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var results = session.Query.All<TestEntity4>()
          .OfType<IHasStatus>()
          .Select(e => e.Status)
          .ToArray();

        Assert.That(results.Length, Is.EqualTo(1));
        Assert.That(results[0].Name, Is.EqualTo("test"));
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0759_UnableToTranslateOfTypeSelectExpressionModel
{
  public interface IHasValue : IEntity
  {
    [Field]
    long Value { get; set; }

    [Field]
    string Comment { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime CreationDate { get; set; }
  }

  public class TestEntity2 : TestEntity1, IHasValue
  {
    [Field]
    public int DirectlyDeclaredField { get; set; } 

    public long Value { get; set; }

    public string Comment { get; set; }
  }

  public class TestEntity3 : TestEntity2
  {
    [Field]
    public double Field4 { get; set; }

    [Field]
    public EntitySet<TestEntity2> Field5 { get; set; }
  }

  public interface IHasStatus : IEntity
  {
    [Field]
    Status Status { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class Status : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public bool IsDeletable { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class TestEntity4 : Entity, IHasStatus
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public string TestField { get; set; }

    public Status Status { get; set; }
  }
}
