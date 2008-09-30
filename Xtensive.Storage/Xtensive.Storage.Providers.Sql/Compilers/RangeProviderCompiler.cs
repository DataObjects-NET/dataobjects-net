// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System;
using System.Linq;
using Xtensive.Indexing;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class RangeProviderCompiler : TypeCompiler<RangeProvider>
  {
    protected override ExecutableProvider Compile(RangeProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      var keyColumns = provider.Header.Order.ToList();
      var originalRange = provider.Range();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var rangeProvider = new SqlRangeProvider(provider, request, Handlers, originalRange, source);

      Func<int,SqlParameter,SqlExpression> fromCompiler = null;
      fromCompiler = (i,pp) => {
        SqlExpression result = null;
        bool bContinue = false;
        if (originalRange.EndPoints.First.Count > i && originalRange.EndPoints.First.IsAvailable(i)) {
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.First.GetValue(i), typeMapping);
          switch (originalRange.EndPoints.First.GetValueType(i)) {
          case EntireValueType.Default:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinitesimal:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] > binding.SqlParameter;
            break;
          case EntireValueType.NegativeInfinitesimal:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinity:
            result = SqlFactory.Native("1") == SqlFactory.Native("0");
            break;
          case EntireValueType.NegativeInfinity:
            break;
          }
          if (pp!=null)
            result = (query.Columns[keyColumns[i-1].Key]==pp) & result;
          if (bContinue) {
            var nextColumnExpression = fromCompiler(i + 1, binding.SqlParameter);
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
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler) Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.Second.GetValue(i), typeMapping);
          switch (originalRange.EndPoints.Second.GetValueType(i)) {
          case EntireValueType.Default:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
            bContinue = true;
            break;
          case EntireValueType.PositiveInfinitesimal:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
            bContinue = true;
            break;
          case EntireValueType.NegativeInfinitesimal:
            request.ParameterBindings.Add(binding);
            result = query.Columns[keyColumns[i].Key] < binding.SqlParameter;
            break;
          case EntireValueType.PositiveInfinity:
            break;
          case EntireValueType.NegativeInfinity:
            result = SqlFactory.Native("1") == SqlFactory.Native("0");
            break;
          }
          if (pp!=null)
            result = (query.Columns[keyColumns[i-1].Key]==pp) & result;
          if (bContinue) {
            var nextColumnExpression = toCompiler(i + 1, binding.SqlParameter);
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