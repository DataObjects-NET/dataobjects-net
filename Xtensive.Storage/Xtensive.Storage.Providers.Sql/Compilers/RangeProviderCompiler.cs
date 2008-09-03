// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class RangeProviderCompiler : TypeCompiler<RangeProvider>
  {
    protected override ExecutableProvider Compile(RangeProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      var keyColumns = provider.Header.Order.Select(pair => query.Columns[pair.Key]).ToList();
      var originalRange = provider.CompiledRange.Invoke();
      var request = new SqlQueryRequest(query, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      var rangeProvider = new SqlRangeProvider(provider, request, Handlers, originalRange, source);

      Func<int,SqlParameter,SqlExpression> fromCompiler = null;
      fromCompiler = (i,pp) => {
        SqlExpression result = null;
        bool bContinue = false;
        if (originalRange.EndPoints.First.Count > i && originalRange.EndPoints.First.IsAvailable(i)) {
          var p = new SqlParameter();
          switch (originalRange.EndPoints.First.GetValueType(i)) {
          case EntireValueType.Default:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.First.GetValue(i));
            result = keyColumns[i] >= SqlFactory.ParameterRef(p);
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinitesimal:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.First.GetValue(i));
            result = keyColumns[i] > SqlFactory.ParameterRef(p);
            break;
          case EntireValueType.NegativeInfinitesimal:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.First.GetValue(i));
            result = keyColumns[i] >= SqlFactory.ParameterRef(p);
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinity:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.First.GetValue(i));
            result = SqlFactory.Constant("1") == SqlFactory.Constant("0");
            break;
          case EntireValueType.NegativeInfinity:
            break;
          }
          if (pp!=null)
            result = (keyColumns[i - 1]==pp) & result;
          if (bContinue) {
            var nextColumnExpression = fromCompiler(i + 1, p);
            if (!SqlExpression.IsNull(nextColumnExpression))
              result = result & nextColumnExpression;
          }
        }
        return result;
      };
      Func<int,SqlParameter,SqlExpression> toCompiler = null;
      toCompiler = (i,pp) => {
        SqlExpression result = null;
        bool bContinue = false;
        if (originalRange.EndPoints.Second.Count > i && originalRange.EndPoints.Second.IsAvailable(i)) {
          var p = new SqlParameter();
          switch (originalRange.EndPoints.Second.GetValueType(i)) {
          case EntireValueType.Default:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.Second.GetValue(i));
            result = keyColumns[i] <= SqlFactory.ParameterRef(p);
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinitesimal:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.Second.GetValue(i));
            result = keyColumns[i] <= SqlFactory.ParameterRef(p);
            bContinue = true;
            break;
          case EntireValueType.NegativeInfinitesimal:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.Second.GetValue(i));
            result = keyColumns[i] < SqlFactory.ParameterRef(p);
            break;
          case EntireValueType.PositiveInfinity:
            break;
          case EntireValueType.NegativeInfinity:
            request.ParameterBindings.Add(p, () => rangeProvider.CurrentRange.EndPoints.Second.GetValue(i));
            result = SqlFactory.Constant("1") == SqlFactory.Constant("0");
            break;
          }
          if (pp!=null)
            result = (keyColumns[i - 1]==pp) & result;
          if (bContinue) {
            var nextColumnExpression = toCompiler(i + 1, p);
            if (!SqlExpression.IsNull(nextColumnExpression))
              result = result & nextColumnExpression;
          }
        }
        return result;
      };
      query.Where &= fromCompiler(0, null) && toCompiler(0, null);

      return rangeProvider;
    }


    // Constructors

    public RangeProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}