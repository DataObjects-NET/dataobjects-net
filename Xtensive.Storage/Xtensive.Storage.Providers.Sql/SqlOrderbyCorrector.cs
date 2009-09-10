// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.12

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlOrderbyCorrector : IPostCompiler
  {
    private readonly HandlerAccessor handlers;
    private readonly SqlCompiler sqlCompiler;

    public ExecutableProvider Process(ExecutableProvider rootProvider)
    {
      var sqlProvider = (SqlProvider) rootProvider;
      var query = (SqlSelect) sqlProvider.Request.SelectStatement.Clone();
      if (query.OrderBy.Count > 0)
        return rootProvider;
      if (rootProvider.Origin.Header.Order.Count > 0)
        ApplyOrderingToProvider(sqlProvider, query);
      else {
        var originAsSelect = rootProvider.Origin as SelectProvider;
        if (originAsSelect!=null && originAsSelect.Source.Header.Order.Count > 0)
          query = ApplyOrderingToSelectProvider(sqlProvider, originAsSelect);
        else
          return rootProvider;
      }
      return new SqlProvider(rootProvider.Origin, query, handlers, sqlProvider.Request.ParameterBindings,
        (ExecutableProvider[]) rootProvider.Sources);
    }

    private static void ApplyOrderingToProvider(ExecutableProvider sqlProvider, SqlSelect query)
    {
      foreach (KeyValuePair<int, Direction> pair in sqlProvider.Origin.Header.Order)
        query.OrderBy.Add(query.Columns[pair.Key], pair.Value==Direction.Positive);
    }

    private SqlSelect ApplyOrderingToSelectProvider(SqlProvider rootProvider,
      SelectProvider originAsSelect)
    {
      var result = rootProvider.Request.SelectStatement.ShallowClone();
      var columnExpressions = sqlCompiler.ExtractColumnExpressions(result, originAsSelect);
      var shouldUseColumnPosition = originAsSelect.Source.Header.Order.Any(o => o.Key >= result.From.Columns.Count);
      if (shouldUseColumnPosition)
        foreach (KeyValuePair<int, Direction> pair in originAsSelect.Source.Header.Order) {
          var orderColumnIndex = originAsSelect.ColumnIndexes.IndexOf(pair.Key);
          if (orderColumnIndex >= 0)
            result.OrderBy.Add(orderColumnIndex + 1, pair.Value == Direction.Positive);
          else
            result.OrderBy.Add(columnExpressions[pair.Key], pair.Value == Direction.Positive);
        }
      else
        foreach (KeyValuePair<int, Direction> pair in originAsSelect.Source.Header.Order)
          result.OrderBy.Add(columnExpressions[pair.Key], pair.Value == Direction.Positive);
      return result;
    }


    // Constructors

    public SqlOrderbyCorrector(HandlerAccessor handlers, SqlCompiler sqlCompiler)
    {
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      this.handlers = handlers;
      this.sqlCompiler = sqlCompiler;
    }
  }
}