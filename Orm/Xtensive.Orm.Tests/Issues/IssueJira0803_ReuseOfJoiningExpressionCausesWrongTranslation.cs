// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0803_ReuseOfJoiningExpressionCausesWrongTranslationModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0803_ReuseOfJoiningExpressionCausesWrongTranslationModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public string Text { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0803_ReuseOfJoiningExpressionCausesWrongTranslation : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity(session) { Name = "1", Description = "test", Text = "text" };
        _ = new TestEntity(session) { Name = "1", Description = "test", Text = "text" };
        _ = new TestEntity(session) { Name = "1", Description = null, Text = "text" };

        tx.Complete();
      }
    }

    [Test]
    public void LeftJoinOneVariableUsage()
    {
      Expression<Func<TestEntity, int>> key = it => it.Id;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var leftJoinWithExpression = session.Query.All<TestEntity>()
          .LeftJoin(session.Query.All<TestEntity>().Where(it => it.Description == null),
            o => o.Id,
            key,
            (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList();

        Assert.AreEqual(3, leftJoinWithExpression.Count);

        leftJoinWithExpression = session.Query.All<TestEntity>()
          .LeftJoin(session.Query.All<TestEntity>().Where(it => it.Description == null),
            key,
            i => i.Id,
            (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList();

        Assert.AreEqual(3, leftJoinWithExpression.Count);
      }
    }

    [Test]
    public void LeftJoinTwoVariableUsage()
    {
      Expression<Func<TestEntity, int>> key = it => it.Id;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var ex = Assert.Throws<QueryTranslationException>(() =>
          _ = session.Query.All<TestEntity>()
          .LeftJoin(session.Query.All<TestEntity>().Where(it => it.Description == null),
            key,
            key,
            (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList());

        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
      }
    }

    [Test]
    public void InnerJoinOneVariableUsage()
    {
      Expression<Func<TestEntity, int>> key = it => it.Id;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var leftJoinWithExpression = session.Query.All<TestEntity>()
          .Join(session.Query.All<TestEntity>().Where(it => it.Description == null),
            o => o.Id,
            key,
            (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList();

        Assert.AreEqual(1, leftJoinWithExpression.Count);

        leftJoinWithExpression = session.Query.All<TestEntity>()
          .Join(session.Query.All<TestEntity>().Where(it => it.Description == null),
            key,
            i => i.Id,
            (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList();

        Assert.AreEqual(1, leftJoinWithExpression.Count);
      }
    }

    [Test]
    public void InnerJoinTwoVariableUsage()
    {
      Expression<Func<TestEntity, int>> key = it => it.Id;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var ex = Assert.Throws<QueryTranslationException>(() =>
          _ = session.Query.All<TestEntity>()
            .Join(session.Query.All<TestEntity>().Where(it => it.Description == null),
              key,
              key,
              (o, i) => o)
          .Where(it => it.Text != null)
          .Select(it => it.Id)
          .Distinct()
          .ToList());

        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
      }
    }
  }
}
