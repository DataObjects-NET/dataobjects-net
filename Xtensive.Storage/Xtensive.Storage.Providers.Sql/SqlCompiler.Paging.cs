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
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Limit = binding.ParameterReference;
      if (!(provider.Source is TakeProvider) && !(provider.Source is SkipProvider))
        AddOrderByStatement(provider, query);
      return CreateProvider(query, binding, provider, compiledSource);
    } 

    /// <inheritdoc/>
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var queryRef = compiledSource.PermanentReference;
      var query = SqlDml.Select(queryRef);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Offset = binding.ParameterReference;
      AddOrderByStatement(provider, query);
      return CreateProvider(query, binding, provider, compiledSource);
    }

    protected void AddOrderByStatement(UnaryProvider provider, SqlSelect query)
    {
      var columnExpressions = ExtractColumnExpressions(query, provider);
      foreach (KeyValuePair<int, Direction> pair in provider.Source.Header.Order)
        query.OrderBy.Add(columnExpressions[pair.Key], pair.Value==Direction.Positive);
    }

    protected static QueryParameterBinding CreateLimitOffsetParameterBinding(Func<int> accessor)
    {
      return new QueryParameterBinding(
        BuildLimitOffsetAccessor(accessor),
        null,
        QueryParameterBindingType.LimitOffset);
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