using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Orm.Configuration;
using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0742_QueryPreprocessorClosureExpressionSupport : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestEntity));
      config.Types.Register(typeof(Preprocessor));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        new TestEntity(session) {String = "test"};

        var query = Query.All<TestEntity>().Where(x => x.Id == TestMethod());
        var anyCountFail = Query.All<TestEntity>()
          .Count(e => e.Id == TestMethod() && query.Any(z => z.Id == e.Id));
        Assert.AreEqual(1, anyCountFail);

        var linqCountFail = (from a in Query.All<TestEntity>().Where(e => e.Id == TestMethod())
          from b in query
          where b.Id == a.Id
          select a).Count();

        Assert.AreEqual(1, linqCountFail);
      }
    }

    private static int TestMethod()
    {
      return 2;
    }

    [HierarchyRoot]
    public class TestEntity : Entity
    {
      /// <summary>Initializes a new instance of this class.</summary>
      /// <param name="session">The session.</param>
      public TestEntity(Session session)
        : base(session)
      {
      }

      [Key]
      [Field(Nullable = false)]
      public int Id { get; private set; }

      [Field]
      public string String { get; set; }
    }

    [Service(typeof(IQueryPreprocessor))]
    public class Preprocessor : IQueryPreprocessor
    {
      public Expression Apply(Expression query)
      {
        return new TestVisitor().Visit(query);
      }

      public bool IsDependentOn(IQueryPreprocessor other)
      {
        return false;
      }
    }

    public class TestVisitor : ExpressionVisitor
    {
      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
        return node.Method.Name=="TestMethod" ? Expression.Constant(1, typeof(int)) : base.VisitMethodCall(node);
      }
    }
  }
}
