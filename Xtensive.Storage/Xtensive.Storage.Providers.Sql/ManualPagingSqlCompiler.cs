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
      if (isSourceTake && compiledSource.Request.SelectStatement.Limit == 0) {
        sourceQuery.Where = AddSkipPartToTakeWhereExpression(sourceQuery, null, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddSkipPartToTakeWhereExpression(sourceQuery, queryRef, provider, provider.Source);
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
      if (isSourceSkip && compiledSource.Request.SelectStatement.Offset == 0) {
        sourceQuery.Where = AddTakePartToSkipWhereExpression(sourceQuery, null, provider, provider.Source);
        return new SqlProvider(provider.Source, sourceQuery, Handlers,
          (ExecutableProvider[]) compiledSource.Sources);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddTakePartToSkipWhereExpression(sourceQuery, queryRef, provider, provider.Source);
      return new SqlProvider(provider, query, Handlers, compiledSource);
    }

    private static SqlExpression AddTakePartToSkipWhereExpression(
      SqlSelect sourceQuery, SqlTable sourceQueryRef, TakeProvider provider, CompilableProvider source)
    {
      SqlExpression result;
      if (sourceQueryRef != null)
        result = sourceQueryRef.Columns.Last() <= provider.Count();
      else {
        SqlBinary skipPartOfWhere;
        SqlExpression prevPart;
        var sourceAsSkip = (SkipProvider)source;
        GetWhereParts(sourceQuery, out skipPartOfWhere, out prevPart);
        var rowNumberColumn = (SqlColumn) skipPartOfWhere.Left;
        result = prevPart
          && (rowNumberColumn <= sourceAsSkip.Count() + provider.Count());
      }
      return result;
    }

    private static SqlExpression AddSkipPartToTakeWhereExpression(
      SqlSelect sourceQuery, SqlTable sourceQueryRef, SkipProvider provider, CompilableProvider source)
    {
      SqlExpression result;
      if (sourceQueryRef != null)
        result = sourceQueryRef.Columns.Last() > provider.Count();
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
        prevPart = rightAsBinary.IsNullReference() ? whereAsBinary : whereAsBinary.Left && currentPart;
      }
      else {
        currentPart = (SqlBinary) sourceQuery.Where;
        prevPart = currentPart;
      }
    }

    // Constructors

    public ManualPagingSqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}