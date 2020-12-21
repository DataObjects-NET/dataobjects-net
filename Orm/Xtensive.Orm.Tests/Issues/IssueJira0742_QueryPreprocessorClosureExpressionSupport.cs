// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xtensive.IoC;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0742_QueryPreprocessorClosureExpressionSupport
  {
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MainTest(bool useClosurePreprocessor)
    {
      using (var domain = BuildDomain(useClosurePreprocessor))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new TestEntity(session) { String = "test" };

        var simpleQuery = session.Query.All<TestEntity>().Count(e => e.Id == TestMethod());
        Assert.That(simpleQuery, Is.EqualTo(1));
        session.Extensions.Clear();

        var simpleJoin = session.Query.All<TestEntity>().Where(e => e.Id == TestMethod())
          .Join(session.Query.All<TestEntity>().Where(e => e.Id == TestMethod()),
            o => o.Id, i => i.Id, (o, i) => o)
          .Count();
        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetSuccess());
        Assert.That(simpleJoin, Is.EqualTo(1));
        session.Extensions.Clear();

        var query = session.Query.All<TestEntity>().Where(e => e.Id == TestMethod());

        var variableJoin = session.Query.All<TestEntity>().Where(e => e.Id == TestMethod())
          .Join(query, o => o.Id, i => i.Id, (o, i) => o)
          .Count();

        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetSuccess());
        Assert.That(variableJoin, Is.EqualTo(1));
        session.Extensions.Clear();

        var anyCount = session.Query.All<TestEntity>()
          .Count(e => e.Id == TestMethod() && session.Query.All<TestEntity>().Where(i => i.Id == TestMethod()).Any(z => z.Id == e.Id));

        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetSuccess());
        Assert.That(anyCount, Is.EqualTo(1));
        session.Extensions.Clear();

        var linqCount = (from a in session.Query.All<TestEntity>().Where(e => e.Id == TestMethod())
                         from b in session.Query.All<TestEntity>().Where(e => e.Id == TestMethod())
                         where b.Id == a.Id
                         select a).Count();

        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetSuccess());
        Assert.That(linqCount, Is.EqualTo(1));
        session.Extensions.Clear();

        var anyCountFail = session.Query.All<TestEntity>()
          .Count(e => e.Id == TestMethod() && query.Any(z => z.Id == e.Id));

        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetFail(useClosurePreprocessor));
        Assert.That(anyCountFail, Is.EqualTo(Convert.ToInt32(useClosurePreprocessor)));
        session.Extensions.Clear();

        var linqCountFail = (from a in session.Query.All<TestEntity>().Where(e => e.Id == TestMethod())
                             from b in query
                             where b.Id == a.Id
                             select a).Count();

        Assert.That(session.Extensions.Get(typeof(ClosureMarker)), GetFail(useClosurePreprocessor));
        Assert.That(linqCountFail, Is.EqualTo(Convert.ToInt32(useClosurePreprocessor)));
      }
    }

    private Domain BuildDomain(bool useClosurePreprocessor)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity));
      if (useClosurePreprocessor) {
        configuration.Types.Register(typeof(ClosureQueryPreprocessor));
      }
      configuration.Types.Register(typeof(Preprocessor));
      return Domain.Build(configuration);
    }

    private IResolveConstraint GetSuccess() => Is.Null;

    private IResolveConstraint GetFail(bool useClosurePreprocessor) => useClosurePreprocessor ? Is.Null : Is.Not.Null;

    private static int TestMethod() => 2;

    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string String { get; set; }

      public TestEntity(Session session)
        : base(session)
      {
      }
    }

    public class ClosureMarker
    {
    }

    [Service(typeof(IQueryPreprocessor))]
    public class Preprocessor : IQueryPreprocessor2
    {
      public Expression Apply(Session session, Expression query)
      {
        var visitor = new TestVisitor();
        var result = visitor.Visit(query);
        if (visitor.ClosureRegistered()) {
          session.Extensions.Set(new ClosureMarker());
        }
        return result;
      }

      public Expression Apply(Expression query)
      {
        throw new NotImplementedException();
      }

      public bool IsDependentOn(IQueryPreprocessor other)
      {
        return other is ClosureQueryPreprocessor;
      }
    }

    public class TestVisitor : ExpressionVisitor
    {
      private bool anyClosure = false;

      public bool ClosureRegistered() => anyClosure;

      protected override Expression VisitMember(MemberExpression node)
      {
        if (!anyClosure) {
          anyClosure = node.Type != typeof(Session) && node.Member.DeclaringType.IsClosure();
        }
        return base.VisitMember(node);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node) =>
        node.Method.Name == "TestMethod" ? Expression.Constant(1, typeof(int)) : base.VisitMethodCall(node);
    }
  }
}