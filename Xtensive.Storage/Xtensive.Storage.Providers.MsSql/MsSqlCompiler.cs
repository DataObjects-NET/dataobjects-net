// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <inheritdoc/>
  [Serializable]
  public class MsSqlCompiler : SqlCompiler
  {
    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      const string rowNumber = "RowNumber";

      var compiledSource = GetBound(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      SqlSelect sourceQuery = AddRowNumberColumn(compiledSource, provider, rowNumber);

      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Where(column => column.Name != rowNumber).Cast<SqlColumn>());
      query.Where = sourceQuery[rowNumber] > provider.Count();
      
      AddOrderByForRowNumberColumn(provider, query);
      
      var request = new SqlFetchRequest(query, provider.Header);
      return new SqlProvider(provider, request, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider)
    {
      var compiledSource = GetBound(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      SqlSelect sourceQuery = AddRowNumberColumn(compiledSource, provider, provider.Header.Columns.Last().Name);

      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      AddOrderByForRowNumberColumn(provider, query);

      var request = new SqlFetchRequest(query, provider.Header);
      return new SqlProvider(provider, request, Handlers, compiledSource);
    }

    private void AddOrderByForRowNumberColumn(Provider provider, SqlSelect query)
    {
      if (provider.Header.Order.Count > 0)
        foreach (KeyValuePair<int, Direction> sortOrder in provider.Header.Order)
          query.OrderBy.Add(query.Columns[sortOrder.Key], sortOrder.Value == Direction.Positive);
      else
        query.OrderBy.Add(query.Columns[0], true);
    }

    private SqlSelect AddRowNumberColumn(SqlProvider source, Provider provider, string rowNumberColumnName)
    {
      var sourceQuery = (SqlSelect)source.Request.Statement.Clone();
      SqlExpression rowNumberExpression = SqlFactory.Native("ROW_NUMBER() OVER (ORDER BY ");
      if (provider.Header.Order.Count>0) 
        for (int i = 0; i < provider.Header.Order.Count; i++) {
          if (i != 0)
            rowNumberExpression = SqlFactory.Empty(rowNumberExpression, SqlFactory.Native(", "));
          rowNumberExpression = SqlFactory.Empty(rowNumberExpression,sourceQuery[provider.Header.Order[i].Key]);
          rowNumberExpression = SqlFactory.Empty(rowNumberExpression,SqlFactory.Native(provider.Header.Order[i].Value == Direction.Positive ? " ASC" : " DESC"));
        }
      else
        rowNumberExpression = SqlFactory.Empty(rowNumberExpression,sourceQuery[0]);
      rowNumberExpression = SqlFactory.Empty(rowNumberExpression,SqlFactory.Native(")"));
      sourceQuery.Columns.Add(rowNumberExpression, rowNumberColumnName);
      sourceQuery.OrderBy.Clear();
      return sourceQuery;
    }

    protected override ExecutableProvider VisitApply(ApplyProvider provider)
    {
      bool isOuter;
      switch (provider.ApplyType) {
        case ApplyType.Cross:
          isOuter = false;
          break;
        case ApplyType.Outer:
          isOuter = true;
          break;
        default:
          return base.VisitApply(provider);
      }

      var left = GetBound(provider.Left) as SqlProvider;
      var right = GetBound(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftQuery = left.PermanentReference;
      var rightQuery = SqlFactory.QueryRef((SqlSelect)right.Request.Statement);
      var joinedTable = SqlFactory.Join(isOuter ? SqlJoinType.LeftOuterApply : SqlJoinType.CrossApply,
        leftQuery, rightQuery);

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Union(rightQuery.Columns).Cast<SqlColumn>());
      var request = new SqlFetchRequest(query, provider.Header);
      return new SqlProvider(provider, request, Handlers, left, right);
    }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MsSqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}