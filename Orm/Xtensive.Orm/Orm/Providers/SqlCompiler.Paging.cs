// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers
{
  partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativeTake))
        return VisitTakeNative(provider);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitTakeRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Take"));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativeSkip))
        return VisitSkipNative(provider);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitSkipRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Skip"));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitPaging(PagingProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativePaging))
        return VisitPagingNative(provider);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitPagingRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Take/Skip"));
    }

    private SqlProvider VisitTakeNative(TakeProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Limit = binding.ParameterReference;
      // For SQL Server / SQL Server CE which support take but not skip
      // AddOrderByStatement below leads to incorrect queries
      // because "order by" is already added at different place.
      // Corresponding check in VisitSkipNative() is not required
      // because there are no servers that support Skip, but don't support Take.
      if (providerInfo.Supports(ProviderFeatures.NativeSkip))
        AddOrderByStatement(provider, query);
      return CreateProvider(query, binding, provider, compiledSource);
    }

    private SqlProvider VisitSkipNative(SkipProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Offset = binding.ParameterReference;
      AddOrderByStatement(provider, query);
      return CreateProvider(query, binding, provider, compiledSource);
    }

    private SqlProvider VisitPagingNative(PagingProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var skipBinding = CreateLimitOffsetParameterBinding(provider.Skip);
      var takeBinding = CreateLimitOffsetParameterBinding(provider.Take);
      query.Offset = skipBinding.ParameterReference;
      query.Limit = takeBinding.ParameterReference;
      AddOrderByStatement(provider, query);
      return CreateProvider(query, new[] {skipBinding, takeBinding}, provider, compiledSource);
    }

    private SqlProvider VisitSkipRowNumber(SkipProvider provider)
    {
      var skipParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> {skipParameterBinding};

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = queryRef.Columns.Last() > skipParameterBinding.ParameterReference;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlProvider VisitTakeRowNumber(TakeProvider provider)
    {
      var takeParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> { takeParameterBinding };

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = queryRef.Columns.Last() <= takeParameterBinding.ParameterReference;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlProvider VisitPagingRowNumber(PagingProvider provider)
    {
      var fromParameterBinding = CreateLimitOffsetParameterBinding(provider.From);
      var toParameterBinding = CreateLimitOffsetParameterBinding(provider.To);
      var bindings = new List<QueryParameterBinding> { fromParameterBinding, toParameterBinding };

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      var rowNumberColumn = queryRef.Columns.Last();
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = SqlDml.Between(
        rowNumberColumn, 
        fromParameterBinding.ParameterReference,
        toParameterBinding.ParameterReference);
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private void AddOrderByStatement(UnaryProvider provider, SqlSelect query)
    {
      var columnExpressions = ExtractColumnExpressions(query, provider);
      foreach (KeyValuePair<int, Direction> pair in provider.Source.Header.Order)
        query.OrderBy.Add(columnExpressions[pair.Key], pair.Value==Direction.Positive);
    }

    private static QueryParameterBinding CreateLimitOffsetParameterBinding(Func<int> accessor)
    {
      return new QueryParameterBinding(null,
        BuildLimitOffsetAccessor(accessor), QueryParameterBindingType.LimitOffset);
    }

    private static Func<object> BuildLimitOffsetAccessor(Func<int> originalAccessor)
    {
      return () => {
        var value = originalAccessor.Invoke();
        return value < 0 ? 0 : value;
      };
    }
  }
}