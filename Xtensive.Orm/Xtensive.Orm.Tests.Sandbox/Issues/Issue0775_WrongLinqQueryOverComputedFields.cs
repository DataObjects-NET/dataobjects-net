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
using Xtensive.Orm.Tests.Issues.Issue0775_WrongLinqQueryOverComputedFields_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0775_WrongLinqQueryOverComputedFields_Model
  {
    [HierarchyRoot]
    public class Share : Entity
    {
      private static readonly Expression<Func<Share, string>> PduRegNumberExpression = e =>
        e.FinTool == null
          ? null
          : e.Session.Query.All<Fund>().FirstOrDefault(f => f.Person == e.FinTool.Person) != null
              ? e.Session.Query.All<Fund>().FirstOrDefault(f => f.Person == e.FinTool.Person).PduRegNumber
              : null;

      private static readonly Func<Share, string> PduRegNumberExpressionCompiled = PduRegNumberExpression.Compile();

      public Share(Guid id)
        : base(id)
      {}

      [Field]
      [Key]
      public Guid Id { get; private set; }

      [Field(Nullable = false)]
      public Fund FinTool { get; set; }

      public string PduRegNumber
      {
        get { return PduRegNumberExpressionCompiled(this); }
      }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Share), "PduRegNumber", TargetKind.PropertyGet)]
        public static Expression SharePduRegNumber(Expression assignmentExpression)
        {
          return PduRegNumberExpression.BindParameters(assignmentExpression);
        }
      }

      #endregion
    }

    [HierarchyRoot]
    public class Person : Entity
    {
      public Person(Guid id)
        : base(id)
      {
      }

      [Field]
      [Key]
      public Guid Id { get; private set; }

      [Field]
      public int IntId { get; set; }
    }

    [HierarchyRoot]
    public class Fund : Entity
    {
      public Fund(Guid id)
        : base(id)
      {
      }

      [Field]
      [Key]
      public Guid Id { get; private set; }

      [Field]
      public int IntId { get; set; }

      [Field(Nullable = false)]
      public Person Person { get; set; }

      [Field(Length = 50, Nullable = true)]
      public string PduRegNumber { get; set; }
    }
  }

  [Serializable]
  public class Issue0775_WrongLinqQueryOverComputedFields : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Fund).Assembly, typeof (Fund).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var query = session.Query.All<Share>().Select(s => new {s.Id, s.FinTool, s.PduRegNumber});
        var list = query.ToList();

      }
    }
    /*SELECT  [a].[Id] ,
            [a].[FinTool_Id] ,
            [a].[EmissionVolume] ,
            [a].[Version] ,
            [b].[#a.Name] ,
            [c].[#b.Id] ,
            [d].[#c.PduRegNumber]
    FROM    ( SELECT TOP 41
                        [e].[Id] ,
                        191 AS [TypeId] ,
                        [e].[FinTool_Id] ,
                        [e].[EmissionVolume] ,
                        [e].[Version] ,
                        ROW_NUMBER() OVER ( ORDER BY [e].[Id] ASC ) AS [RowNumber0]
              FROM      [dbo].[Share] [e]
              WHERE     ( [e].[Version] < CAST('2010-07-15 00:00:00.000' AS DATETIME2) )
              ORDER BY  [e].[Id] ASC
            ) [a]
            INNER JOIN ( SELECT [f].[Id] AS [#a.Id] ,
                                146 AS [#a.TypeId] ,
                                [f].[IntId] AS [#a.IntId] ,
                                [f].[Emitter_Id] AS [#a.Emitter_Id] ,
                                [f].[FinToolGroup_Id] AS [#a.FinToolGroup_Id] ,
                                [f].[FinToolType_Id] AS [#a.FinToolType_Id] ,
                                [f].[Name] AS [#a.Name] ,
                                [f].[FullName] AS [#a.FullName] ,
                                [f].[Currency_Id] AS [#a.Currency_Id] ,
                                [f].[Nominal] AS [#a.Nominal] ,
                                [f].[Version] AS [#a.Version]
                         FROM   [dbo].[FinTool] [f]
                       ) [b] ON ( [a].[FinTool_Id] = [b].[#a.Id] )
            OUTER APPLY ( SELECT TOP 1
                                    [g].[Id] AS [#b.Id]
                          FROM      [dbo].[Fund] [g]
                          WHERE     ( [g].[Person_Id] = [b].[#a.IntId] )
                          ORDER BY  [g].[Id] ASC
                        ) [c]
            OUTER APPLY ( SELECT TOP 1
                                    [h].[Id] AS [#c.Id] ,
                                    [h].[PduRegNumber] AS [#c.PduRegNumber]
                          FROM      [dbo].[Fund] [h]
                          WHERE     ( [h].[Person_Id] = [b].[#a.IntId] )
                          ORDER BY  [h].[Id] ASC
                        ) [d]
    WHERE   ( [a].[RowNumber0] > 0 )
    ORDER BY [a].[Id] ASC ,
            [b].[#a.Id] ASC ,
            [c].[#b.Id] ASC ,
            [d].[#c.Id] ASC ;*/
  }
}