// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.13

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0776_OrderingCorrectorOfAggregateProvider_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0776_OrderingCorrectorOfAggregateProvider_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class Another : Entity
    {
      private static readonly Expression<Func<Another, string>> MyEntTextExpression =
        e => e.MyEnt == null ? null : e.MyEnt.Text;

      private static readonly Func<Another, string> MyEntTextExpressionCompiled = MyEntTextExpression.Compile();

      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }

      [Field]
      public MyEntity MyEnt { get; set; }

      public string MyEntText
      {
        get { return MyEntTextExpressionCompiled(this); }
      }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Another), "MyEntText", TargetKind.PropertyGet)]
        public static Expression MyEntText(Expression assignmentExpression)
        {
          return MyEntTextExpression.BindParameters(assignmentExpression);
        }
      }

      #endregion
    }
  }

  [Serializable]
  public class Issue0776_OrderingCorrectorOfAggregateProvider : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (TransactionScope transactionScope = session.OpenTransaction())
      {
        // Creating new persistent object
        var helloWorld = new MyEntity
                           {
                             Text = "Hello World!"
                           };
        // Committing transaction

        var hw2 = new Another {Text = "ololo", MyEnt = helloWorld};

        transactionScope.Complete();
      }

      // Reading all persisted objects from another Session
      using (var session = Domain.OpenSession())
      using (TransactionScope transactionScope = session.OpenTransaction())
      {
        foreach (MyEntity myEntity in session.Query.All<MyEntity>())
          Console.WriteLine(myEntity.Text);


        var query = session.Query.All<Another>().Where(a => a.MyEntText.Contains("e")).Select(a => new {a.Id, a.MyEntText});
        foreach (var item in query)
          // OK
          Console.WriteLine(item);


        var aggregateQuery = from q in query
                             select new {Item = q, FakeKey = 0}
                             into i
                             group i by i.FakeKey;
        var aggregate = aggregateQuery.Select(a => new {Count = a.Count()});

        foreach (var item in aggregate)
          // OK
          Console.WriteLine(item);

        // Exception!
        var nowCount = aggregate.FirstOrDefault();

        var futureCount = session.Query.ExecuteFutureScalar(() => aggregate.FirstOrDefault());

        transactionScope.Complete();
      }
    }
  }
}