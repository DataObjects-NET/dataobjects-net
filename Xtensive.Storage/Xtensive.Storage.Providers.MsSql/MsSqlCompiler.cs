// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  internal class MsSqlCompiler : SqlCompiler
  {
    protected override ExecutableProvider VisitExistence(ExistenceProvider provider)
    {
      var result = (SqlProvider) base.VisitExistence(provider);
      if (result == null)
        return null;
      var select = result.Request.SelectStatement;
      ReplaceBooleanColumn(select, 0);
      return result;
    }

    protected override ExecutableProvider VisitCalculate(CalculateProvider provider)
    {
      var result = (SqlProvider)base.VisitCalculate(provider);
      if (result == null)
        return null;
      var select = result.Request.SelectStatement;
      int columnsCount = provider.Header.Length;

      for (int i = provider.Source.Header.Length; i < columnsCount; i++)
        if (provider.Header.Columns[i].Type == typeof(bool))
          ReplaceBooleanColumn(select, i);

      return result;
    }

    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      const string rowNumber = "RowNumber";

      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
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
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
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
      var sourceQuery = (SqlSelect)source.Request.SelectStatement.Clone();
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
      var result = base.VisitApply(provider);
      if (result != null)
        return result;
      bool isOuter = provider.ApplyType==ApplyType.Outer;

      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftQuery = left.PermanentReference;
      var rightQuery = SqlFactory.QueryRef(right.Request.SelectStatement);
      var joinedTable = SqlFactory.Join(isOuter ? SqlJoinType.LeftOuterApply : SqlJoinType.CrossApply,
        leftQuery, rightQuery);

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Concat(rightQuery.Columns).Cast<SqlColumn>());
      var request = new SqlFetchRequest(query, provider.Header);
      return new SqlProvider(provider, request, Handlers, left, right);
    }

    protected override LambdaExpression PreprocessExpression(LambdaExpression lambda)
    {
      return ExpressionPreprocessor.Preprocess(lambda);
    }

    protected override SqlExpression PostprocessExpression(SqlExpression expression, Type resultType)
    {
      if (resultType == typeof(bool)) {
        if (expression.NodeType == SqlNodeType.Parameter)
          return SqlFactory.NotEquals(expression, 0);

        if (expression.NodeType == SqlNodeType.Not) {
          var operand = ((SqlUnary)expression).Operand;
          if (operand.NodeType == SqlNodeType.Parameter)
            return SqlFactory.Equals(operand, 0);
          return expression;
        }
      }
      // expression = (SqlExpression) expression.Clone();
      ReplaceBooleanParameters(expression);
      return expression;
    }

    private static void ReplaceBooleanParameters(SqlExpression expression)
    {
      var binary = expression as SqlBinary;
      if (binary == null)
        return;

      var left = binary.Left;
      var right = binary.Right;

      switch (expression.NodeType) {
        case SqlNodeType.Or:
        case SqlNodeType.And:
          break;
        default:
          ReplaceBooleanParameters(left);
          ReplaceBooleanParameters(right);
          return;
      }

      if (left.NodeType == SqlNodeType.Parameter)
        left = SqlFactory.NotEquals(left, 0);
      else
        ReplaceBooleanParameters(left);

      if (right.NodeType == SqlNodeType.Parameter)
        right = SqlFactory.NotEquals(right, 0);
      else
        ReplaceBooleanParameters(right);

      switch (binary.NodeType) {
        case SqlNodeType.Or:
          binary.ReplaceWith(SqlFactory.Or(left, right));
          break;
        case SqlNodeType.And:
          binary.ReplaceWith(SqlFactory.And(left, right));
          break;
      }
    }

    private static void ReplaceBooleanColumn(SqlSelect select, int index)
    {
      var columnRef = (SqlColumnRef) select.Columns[index];
      var oldExpression = ((SqlUserColumn)columnRef.SqlColumn).Expression;
      var caseExpression = SqlFactory.Case();
      caseExpression.Add(oldExpression, 1);
      caseExpression.Else = 0;
      var newExpression = SqlFactory.Cast(caseExpression, SqlDataType.Boolean);
      select[index].ReplaceWith(SqlFactory.ColumnRef(SqlFactory.Column(newExpression), columnRef.Name));
    }
    
    // Constructor

    public MsSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}