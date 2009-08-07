// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using System;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class ManualPagingSqlCompiler : SqlCompiler
  {
    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      var isSourceTake = provider.Source is TakeProvider;
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource==null)
        return null;

      SqlSelect source = compiledSource.Request.SelectStatement;
      var sourceQuery = source.ShallowClone();
      if (isSourceTake) {
        sourceQuery.Where = AddSkipPartToTakeWhereExpression(sourceQuery, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
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

      SqlSelect source = compiledSource.Request.SelectStatement;
      var sourceQuery = source.ShallowClone();
      if (isSourceSkip) {
        sourceQuery.Where = AddTakePartToSkipWhereExpression(sourceQuery, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddTakePartToSkipWhereExpression(sourceQuery, provider, provider.Source);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider)
    {
      if (provider.Header.Order.Count==0)
        throw new InvalidOperationException(Strings.ExOrderingOfRecordsIsNotSpecifiedForRowNumberProvider);
      var compiledSource = GetCompiled(provider.Source) as SqlProvider;
      if (compiledSource == null)
        return null;

      SqlSelect source = compiledSource.Request.SelectStatement;
      var sourceQuery = source.ShallowClone(); // why clone?
      var rowNumberColumnName = provider.Header.Columns.Last().Name;
      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query = AddRowNumberColumn(query, provider, rowNumberColumnName);

      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    private static SqlExpression AddTakePartToSkipWhereExpression(
      SqlSelect sourceQuery, TakeProvider provider, CompilableProvider source)
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

    private static SqlExpression AddSkipPartToTakeWhereExpression(
      SqlSelect sourceQuery, SkipProvider provider, CompilableProvider source)
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

    protected abstract SqlSelect AddRowNumberColumn(
      SqlSelect sourceQuery, CompilableProvider provider, string rowNumberColumnName);

    // Constructors

    public ManualPagingSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}