// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Collections.Generic;

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class ManualPagingSqlCompiler : SqlCompiler
  {
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      var isSourceTake = provider.Source is TakeProvider;
      var compiledSource = Compile(provider.Source);
      var bindings = new List<QueryParameterBinding>();
      var source = compiledSource.Request.SelectStatement;
      var sourceQuery = source.ShallowClone();
      if (isSourceTake && compiledSource.Request.SelectStatement.Limit.IsNullReference()) {
        sourceQuery.Where = AddSkipPartToTakeWhereExpression(sourceQuery, null, provider, bindings);
        return CreateProvider(sourceQuery, bindings, provider.Source, compiledSource);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddSkipPartToTakeWhereExpression(sourceQuery, queryRef, provider, bindings);
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var isSourceSkip = provider.Source is SkipProvider;
      var compiledSource = Compile(provider.Source);
      var bindings = new List<QueryParameterBinding>();

      var source = compiledSource.Request.SelectStatement;
      var sourceQuery = source.ShallowClone();
      if (isSourceSkip && compiledSource.Request.SelectStatement.Offset.IsNullReference()) {
        sourceQuery.Where = AddTakePartToSkipWhereExpression(sourceQuery, null, provider, bindings);
        return CreateProvider(sourceQuery, bindings, provider.Source, compiledSource);
      }

      var queryRef = SqlDml.QueryRef(sourceQuery);
      var query = SqlDml.Select(queryRef);
      
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = AddTakePartToSkipWhereExpression(sourceQuery, queryRef, provider, bindings);
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlExpression AddTakePartToSkipWhereExpression(
      SqlSelect sourceQuery, SqlTable sourceQueryRef, TakeProvider provider,
      List<QueryParameterBinding> bindings)
    {
      SqlExpression result;
      var takeParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      bindings.Add(takeParameterBinding);
      if (sourceQueryRef != null)
        result = sourceQueryRef.Columns.Last() <= takeParameterBinding.ParameterReference;
      else {
        SqlBinary skipPartOfWhere;
        SqlExpression prevPart;
        var sourceAsSkip = (SkipProvider) provider.Source;
        var skipParameterBinding = CreateLimitOffsetParameterBinding(sourceAsSkip.Count);
        bindings.Add(skipParameterBinding);
        GetWhereParts(sourceQuery, out skipPartOfWhere, out prevPart);
        var rowNumberColumn = (SqlColumn) skipPartOfWhere.Left;
        result = prevPart
          && (rowNumberColumn <= skipParameterBinding.ParameterReference + takeParameterBinding.ParameterReference);
      }
      return result;
    }

    private SqlExpression AddSkipPartToTakeWhereExpression(
      SqlSelect sourceQuery, SqlTable sourceQueryRef, SkipProvider provider,
      List<QueryParameterBinding> bindings)
    {
      var skipParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      bindings.Add(skipParameterBinding);
      SqlExpression result;
      if (sourceQueryRef != null)
        result = sourceQueryRef.Columns.Last() > skipParameterBinding.ParameterReference;
      else {
        SqlBinary skipPartOfWhere;
        SqlExpression prevPart;
        GetWhereParts(sourceQuery, out skipPartOfWhere, out prevPart);
        var rowNumberColumn = (SqlColumn) skipPartOfWhere.Left;
        result = prevPart && (rowNumberColumn > skipParameterBinding.ParameterReference);
      }
      return result;
    }

    private void GetWhereParts(SqlSelect sourceQuery, out SqlBinary currentPart, out SqlExpression prevPart)
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

    public ManualPagingSqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}