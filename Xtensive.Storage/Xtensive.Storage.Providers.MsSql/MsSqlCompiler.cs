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
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

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

      foreach (var column in provider.CalculatedColumns)
        if (column.Type == typeof(bool))
          ReplaceBooleanColumn(select, column.Index);

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
      query.OrderBy.Add(queryRef.Columns[rowNumber]); 
//      AddOrderByForRowNumberColumn(provider, query);
      
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      var rowNumberColumnName = provider.Header.Columns.Last().Name;
      SqlSelect sourceQuery = AddRowNumberColumn(compiledSource, provider, rowNumberColumnName);

      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.OrderBy.Add(queryRef.Columns[rowNumberColumnName]);

//      AddOrderByForRowNumberColumn(provider, query);

      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

//    private void AddOrderByForRowNumberColumn(Provider provider, SqlSelect query)
//    {
//      if (provider.Header.Order.Count > 0)
//        foreach (KeyValuePair<int, Direction> sortOrder in provider.Header.Order)
//          query.OrderBy.Add(query.Columns[sortOrder.Key], sortOrder.Value == Direction.Positive);
//      else
//        query.OrderBy.Add(query.Columns[0], true);
//    }

    private SqlSelect AddRowNumberColumn(SqlProvider source, Provider provider, string rowNumberColumnName)
    {
      var sourceQuery = (SqlSelect)source.Request.SelectStatement.Clone();
      SqlExpression rowNumberExpression = SqlFactory.Native("ROW_NUMBER() OVER (ORDER BY ");
      if (provider.Header.Order.Count>0) 
        for (int i = 0; i < provider.Header.Order.Count; i++) {
          if (i != 0)
            rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, SqlFactory.Native(", "));
          rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, sourceQuery[provider.Header.Order[i].Key]);
          rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, SqlFactory.Native(provider.Header.Order[i].Value == Direction.Positive ? " ASC" : " DESC"));
        }
      else
        rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, sourceQuery[0]);
      rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, SqlFactory.Native(")"));
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
      return new SqlProvider(provider, query, Handlers, left, right);
    }

    protected override SqlExpression TranslateExpression(LambdaExpression le, out HashSet<SqlFetchParameterBinding> parameterBindings, SqlSelect[] selects)
    {
      le = ExpressionPreprocessor.Preprocess(le);
      var resultExpression = base.TranslateExpression(le, out parameterBindings, selects);

      var expression = le.Body;
      while (expression.Type == typeof(object) && expression.NodeType == ExpressionType.Convert)
        expression = ((UnaryExpression) expression).Operand;
      if (expression.Type == typeof(bool)) {
        if (resultExpression.NodeType == SqlNodeType.Parameter)
          return SqlFactory.NotEquals(resultExpression, 0);
        if (resultExpression.NodeType == SqlNodeType.Not) {
          var operand = ((SqlUnary) resultExpression).Operand;
          if (operand.NodeType == SqlNodeType.Parameter)
            return SqlFactory.Equals(operand, 0);
        }
      }
      ReplaceBooleanParameters(resultExpression);
      return resultExpression;
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
      if (oldExpression.NodeType == SqlNodeType.Parameter)
        return;
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