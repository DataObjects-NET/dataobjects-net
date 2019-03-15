
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
    public void WithoutClosurePreprocessor()
    {
      RunTest(false);
    }

    [Test]
    public void WithClosurePreprocessor()
    {
      RunTest(true);
    }

    private void RunTest(bool useClosurePreprocessor)
    {
      using (var domain = BuildDomain(useClosurePreprocessor))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new TestEntity(session) { String = "test" };

        var simpleQuery = Query.All<TestEntity>().Count(e => e.Id==TestMethod());
        Assert.That(simpleQuery, Is.EqualTo(1));

        var simpleJoin = Query.All<TestEntity>().Where(e => e.Id==TestMethod())
          .Join(Query.All<TestEntity>().Where(e => e.Id==TestMethod()),
            o => o.Id,
            i => i.Id,
            (o, i) => o)
          .Count();
        Assert.That(session.Extensions.Get(typeof (ClosureMarker)), GetSuccess());
        Assert.That(simpleJoin, Is.EqualTo(1));

        var query = Query.All<TestEntity>().Where(e => e.Id==TestMethod());

        var variableJoin = Query.All<TestEntity>().Where(e => e.Id==TestMethod())
          .Join(query,
            o => o.Id,
            i => i.Id,
            (o, i) => o)
          .Count();

        Assert.That(session.Extensions.Get(typeof (ClosureMarker)), GetSuccess());
        Assert.That(variableJoin, Is.EqualTo(1));

        var anyCount = Query.All<TestEntity>()
            .Count(e => e.Id==TestMethod() && Query.All<TestEntity>().Where(i => i.Id==TestMethod()).Any(z => z.Id==e.Id));
        Assert.That(anyCount, Is.EqualTo(1));

        var linqCount = (from a in Query.All<TestEntity>().Where(e => e.Id==TestMethod())
                         from b in Query.All<TestEntity>().Where(e => e.Id==TestMethod())
                         where b.Id == a.Id
                         select a).Count();

        Assert.That(session.Extensions.Get(typeof (ClosureMarker)), GetSuccess());
        Assert.That(linqCount, Is.EqualTo(1));

        var anyCountFail = Query.All<TestEntity>()
            .Count(e => e.Id==TestMethod() && query.Any(z => z.Id==e.Id));

        Assert.That(session.Extensions.Get(typeof (ClosureMarker)), GetFail(useClosurePreprocessor));
        Assert.That(anyCountFail, Is.EqualTo(Convert.ToInt32(useClosurePreprocessor)));
        session.Extensions.Clear();

        var linqCountFail = (from a in Query.All<TestEntity>().Where(e => e.Id==TestMethod())
                             from b in query
                             where b.Id==a.Id
                             select a).Count();

        Assert.That(session.Extensions.Get(typeof (ClosureMarker)), GetFail(useClosurePreprocessor));
        Assert.That(linqCountFail, Is.EqualTo(Convert.ToInt32(useClosurePreprocessor)));
      }
    }

    private Domain BuildDomain(bool useClosurePreprocessor)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (TestEntity));
      if (useClosurePreprocessor)
        configuration.Types.Register(typeof (ClosureQueryPreprocessor));
      configuration.Types.Register(typeof (Preprocessor));
      return Domain.Build(configuration);
    }

    private IResolveConstraint GetSuccess()
    {
      return Is.Null;
    }

    private IResolveConstraint GetFail(bool useClosurePreprocessor)
    {
      return useClosurePreprocessor ? Is.Null : Is.Not.Null;
    }

    private static int TestMethod()
    {
      return 2;
    }

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

    [Service(typeof (IQueryPreprocessor))]
    public class Preprocessor : IQueryPreprocessor2
    {
      public Expression Apply(Session session , Expression query)
      {
        var visitor = new TestVisitor();
        var result = visitor.Visit(query);
        if(visitor.ClosureRegistered())
          session.Extensions.Set(new ClosureMarker());
        return result;
      }

      public Expression  Apply(Expression query)
      {
        throw new System.NotImplementedException();
      }

      public bool IsDependentOn(IQueryPreprocessor other)
      {
        if (other is ClosureQueryPreprocessor)
          return true;
        return false;
      }
    }

    public class TestVisitor : ExpressionVisitor
    {
      private bool anyClosure = false;

      public bool ClosureRegistered()
      {
        return anyClosure;
      }

      protected override Expression VisitMember(MemberExpression node)
      {
        if (!anyClosure)
          anyClosure = node.Member.DeclaringType.IsClosure();
        return base.VisitMember(node);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
        return node.Method.Name=="TestMethod" ? Expression.Constant(1, typeof(int)) : base.VisitMethodCall(node);
      }
    }
  }
}
