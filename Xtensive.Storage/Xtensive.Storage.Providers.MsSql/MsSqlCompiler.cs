// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.MsSql.Resources;
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
    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      var isSourceTake = provider.Source is TakeProvider;
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource==null)
        return null;
      
      var sourceQuery = ShallowCopy(compiledSource.Request.SelectStatement);
      if (isSourceTake) {
        sourceQuery.Where = AddSkipPartToTakeWhereExpression(sourceQuery, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddSkipPartToTakeWhereExpression(sourceQuery, provider, provider.Source);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitTake(TakeProvider provider)
    {
      var isSourceSkip = provider.Source is SkipProvider;
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource==null)
        return null;

      var sourceQuery = ShallowCopy(compiledSource.Request.SelectStatement);
      if (isSourceSkip) {
        sourceQuery.Where = AddTakePartToSkipWhereExpression(sourceQuery, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddTakePartToSkipWhereExpression(sourceQuery, provider, provider.Source);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider)
    {
      if(provider.Header.Order.Count == 0)
        throw new InvalidOperationException(Strings.ExOrderingOfRecordsIsNotSpecifiedForRowNumberProvider);
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      var sourceQuery = ShallowCopy(compiledSource.Request.SelectStatement);
      var rowNumberColumnName = provider.Header.Columns.Last().Name;
      var queryRef = SqlFactory.QueryRef(sourceQuery);
      var query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query = AddRowNumberColumn(query, provider, rowNumberColumnName);

      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitApply(ApplyProvider provider)
    {
      var overrideDefaultCompilation = provider.SequenceType == ApplySequenceType.First ||
                                       provider.SequenceType == ApplySequenceType.FirstOrDefault;
      if (!overrideDefaultCompilation) {
        var result = base.VisitApply(provider);
        if (result != null)
          return result;
      }
      bool isOuter = provider.ApplyType==JoinType.LeftOuter;

      var left = GetCompiled(provider.Left) as SqlProvider;
      var right = GetCompiled(provider.Right) as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftQuery = left.PermanentReference;
      var rightQuery = right.PermanentReference;
      var joinedTable = SqlFactory.Join(isOuter ? SqlJoinType.LeftOuterApply : SqlJoinType.CrossApply,
        leftQuery, rightQuery);

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Concat(rightQuery.Columns).Cast<SqlColumn>());
      return new SqlProvider(provider, query, Handlers, left, right);
    }

    protected override SqlExpression TranslateAggregate(SqlProvider source, List<SqlTableColumn> sourceColumns, AggregateColumn aggregateColumn)
    {
      var aggregateType = aggregateColumn.Type;
      var result = base.TranslateAggregate(source, sourceColumns, aggregateColumn);
      if (aggregateColumn.AggregateType == AggregateType.Avg) {
        var originType = source.Origin.Header.Columns[aggregateColumn.SourceIndex].Type;
        // floats are promoted to doubles, but we need the same type
        if (originType == aggregateType && originType != typeof (float))
          return result;
        var sqlType = ValueTypeMapper.BuildSqlValueType(aggregateType, null);
        return SqlFactory.Cast(SqlFactory.Avg(SqlFactory.Cast(sourceColumns[aggregateColumn.SourceIndex], sqlType)), sqlType);
      }
      // cast to decimal is dangerous, because 'decimal' defaults to integer type
      if (aggregateColumn.AggregateType == AggregateType.Sum && aggregateType != typeof(decimal))
        return SqlFactory.Cast(result, ValueTypeMapper.BuildSqlValueType(aggregateType, null));
      return result;
    }

    #region Private methods

    private static SqlExpression AddTakePartToSkipWhereExpression(SqlSelect sourceQuery,
      TakeProvider provider, CompilableProvider source)
    {
      var sourceAsSkip = source as SkipProvider;
      SqlExpression result;
      if (sourceAsSkip==null)
        result = sourceQuery.Columns.Last() <= provider.Count();
      else {
        SqlBinary skipPartOfWhere;
        SqlExpression prevPart;
        GetWhereParts(sourceQuery, out skipPartOfWhere, out prevPart);
        var rowNumberColumn = (SqlColumn) skipPartOfWhere.Left;
        result = prevPart
          && (rowNumberColumn <= sourceAsSkip.Count() + provider.Count());
      }
      return result;
    }

    private static SqlExpression AddSkipPartToTakeWhereExpression(SqlSelect sourceQuery,
      SkipProvider provider, CompilableProvider source)
    {
      var sourceAsTake = source as TakeProvider;
      SqlExpression result;
      if (sourceAsTake==null)
        result = sourceQuery.Columns.Last() > provider.Count();
      else {
        SqlBinary skipPartOfWhere;
        SqlExpression prevPart;
        GetWhereParts(sourceQuery, out skipPartOfWhere, out prevPart);
        var rowNumberColumn = (SqlColumn) skipPartOfWhere.Left;
        result = prevPart
          && (rowNumberColumn > provider.Count());
      }
      return result;
    }

    private static void GetWhereParts(SqlSelect sourceQuery, out SqlBinary currentPart,
      out SqlExpression prevPart)
    {
      var whereAsBinary = sourceQuery.Where as SqlBinary;
      if (whereAsBinary!=null) {
        var rightAsBinary = whereAsBinary.Right as SqlBinary;
        currentPart = rightAsBinary ?? whereAsBinary;
        prevPart = rightAsBinary != null ? whereAsBinary.Left && currentPart : whereAsBinary;
      }
      else {
        currentPart = (SqlBinary) sourceQuery.Where;
        prevPart = currentPart;
      }
    }

    private static SqlSelect AddRowNumberColumn(SqlSelect sourceQuery, Provider provider,
      string rowNumberColumnName)
    {
      SqlExpression rowNumberExpression = SqlFactory.Native("ROW_NUMBER() OVER (ORDER BY ");
      for (var i = 0; i < provider.Header.Order.Count; i++) {
        if (i!=0)
          rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, SqlFactory.Native(", "));
        rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression,
          sourceQuery[provider.Header.Order[i].Key]);
        rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression,
          SqlFactory.Native(provider.Header.Order[i].Value==Direction.Positive ? " ASC" : " DESC"));
      }
      rowNumberExpression = SqlFactory.RawConcat(rowNumberExpression, SqlFactory.Native(")"));
      sourceQuery.Columns.Add(rowNumberExpression, rowNumberColumnName);
      return sourceQuery;
    }
    
    #endregion


    // Constructors

    public MsSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
      BoolIsNativelySupported = false;
    }
  }
}