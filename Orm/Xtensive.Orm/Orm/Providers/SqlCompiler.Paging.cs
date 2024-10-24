// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Providers
{
  public partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativeTake))
        return VisitPagingNative(provider, provider.Count, null);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitTakeRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Take"));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativeSkip))
        return VisitPagingNative(provider, null, provider.Count);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitSkipRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Skip"));
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitPaging(PagingProvider provider)
    {
      if (providerInfo.Supports(ProviderFeatures.NativePaging))
        return VisitPagingNative(provider, provider.Take, provider.Skip);
      if (providerInfo.Supports(ProviderFeatures.RowNumber))
        return VisitPagingRowNumber(provider);
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSupported, "Take/Skip"));
    }

    private SqlProvider VisitPagingNative(UnaryProvider provider, Func<ParameterContext, int> take, Func<ParameterContext, int> skip)
    {
      var compiledSource = Compile(provider.Source);
      var query = ExtractSqlSelect(provider, compiledSource);

      var bindings = new List<QueryParameterBinding>();

      if (take!=null) {
        // Some servers (e.g. SQL Server 2012) don't like Take(0)
        // We work around it with special hacks:
        // Limit argument is replaced with 1
        // and false condition is added to "where" part.
        static bool TakeIsZero(Func<ParameterContext, int> takeFunc)
        {
          var currentScope = CompiledQueryProcessingScope.Current;
          int? takeValue = null;
          if (currentScope == null) {
            takeValue = takeFunc(new ParameterContext());// we just try to evaluate it, new ParameterContext shouldn't be a problem
          }
          return takeValue.HasValue ? takeValue < 1 : true;
        }

        var applyZeroLimitHack = providerInfo.Supports(ProviderFeatures.ZeroLimitIsError) && TakeIsZero(take);
        var takeBinding = CreateLimitOffsetParameterBinding(take, applyZeroLimitHack);
        bindings.Add(takeBinding);
        if (applyZeroLimitHack)
          query.Where &= SqlDml.Variant(takeBinding, SqlDml.Literal(1), SqlDml.Literal(0))!=SqlDml.Literal(0);
        query.Limit = takeBinding.ParameterReference;
      }

      if (skip!=null) {
        var skipBinding = CreateLimitOffsetParameterBinding(skip);
        bindings.Add(skipBinding);
        query.Offset = skipBinding.ParameterReference;
      }

      query.OrderBy.Clear();
      if (provider.Source.Header.Order.Count > 0) {
        var columnExpressions = ExtractColumnExpressions(query);
        foreach (KeyValuePair<int, Direction> pair in provider.Source.Header.Order) {
          query.OrderBy.Add(columnExpressions[pair.Key], pair.Value == Direction.Positive);
        }
      }

      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlProvider VisitSkipRowNumber(SkipProvider provider)
    {
      var skipParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> {skipParameterBinding};
      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns);
      query.Where = queryRef.Columns.Last() > skipParameterBinding.ParameterReference;
      query.Comment = source.Comment;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlProvider VisitTakeRowNumber(TakeProvider provider)
    {
      var takeParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> {takeParameterBinding};
      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns);
      query.Where = queryRef.Columns.Last() <= takeParameterBinding.ParameterReference;
      query.Comment = source.Comment;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private SqlProvider VisitPagingRowNumber(PagingProvider provider)
    {
      var fromParameterBinding = CreateLimitOffsetParameterBinding(provider.From);
      var toParameterBinding = CreateLimitOffsetParameterBinding(provider.To);
      var bindings = new List<QueryParameterBinding> {fromParameterBinding, toParameterBinding};
      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.Statement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      var rowNumberColumn = queryRef.Columns.Last();
      query.Columns.AddRange(queryRef.Columns);
      query.Where = SqlDml.Between(rowNumberColumn,
        fromParameterBinding.ParameterReference,
        toParameterBinding.ParameterReference);
      query.Comment = source.Comment;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    private static QueryParameterBinding CreateLimitOffsetParameterBinding(Func<ParameterContext, int> accessor)
    {
      return CreateLimitOffsetParameterBinding(accessor, false);
    }

    private static QueryParameterBinding CreateLimitOffsetParameterBinding(Func<ParameterContext, int> accessor, bool nonZero)
    {
      var type = nonZero ? QueryParameterBindingType.NonZeroLimitOffset : QueryParameterBindingType.LimitOffset;
      var valueAccessor = BuildLimitOffsetAccessor(accessor);
      return new QueryParameterBinding(null, valueAccessor, type);
    }

    private static Func<ParameterContext, object> BuildLimitOffsetAccessor(Func<ParameterContext, int> originalAccessor)
    {
      return context => {
        var value = originalAccessor.Invoke(context);
        return value < 0 ? 0 : value;
      };
    }
  }
}
