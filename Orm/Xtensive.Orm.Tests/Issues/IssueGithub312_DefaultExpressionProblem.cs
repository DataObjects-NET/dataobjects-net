// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub312_DefaultExpressionProblemModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueGithub312_DefaultExpressionProblemModel
  {
    [HierarchyRoot]
    public sealed class TestEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public int Value { get; set; }

      public TestEntity(Session session)
        : base(session)
      {
      }
    }
  }

  public class IssueGithub312_DefaultExpressionProblem : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity(session) { Name = null, Value = 0 };
        _ = new TestEntity(session) { Name = string.Empty, Value = 0 };
        _ = new TestEntity(session) { Name = "-2", Value = -2 };
        _ = new TestEntity(session) { Name = "-1", Value = -1 };
        _ = new TestEntity(session) { Name = "1", Value = 1 };
        _ = new TestEntity(session) { Name = "2", Value = 2 };
        tx.Complete();
      }
    }

    [Test]
    public void DefaultOfObject()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Default(typeof(object))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(1));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Default(typeof(object))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(5));
      }
    }

    [Test]
    public void DefaultOfValueType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Value)),
            Expression.Default(typeof(int))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(2));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Value)),
            Expression.Default(typeof(int))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(4));
      }
    }

    [Test]
    public void DefaultOfReferenceType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Default(typeof(string))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(1));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Default(typeof(string))),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(5));
      }
    }

    [Test]
    public void ConstantNullObjectValue()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Constant((object) null)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(1));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Constant((object) null)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(5));
      }
    }

    [Test]
    public void ConstantValueTypeDefalut()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Value)),
            Expression.Constant(0)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(2));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Value)),
            Expression.Constant(0)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(4));
      }
    }

    [Test]
    public void ConstantNullStringValue()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var param = Expression.Parameter(typeof(TestEntity), "o");
        var lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.Equal(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Constant((string) null)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(1));

        param = Expression.Parameter(typeof(TestEntity), "o");
        lambda = Expression.Lambda<Func<TestEntity, bool>>(
          Expression.NotEqual(
            Expression.Property(param, nameof(TestEntity.Name)),
            Expression.Constant((string) null)),
          new[] { param });

        Assert.That(session.Query.All<TestEntity>().Where(lambda).Count(), Is.EqualTo(5));
      }
    }
  }
}
