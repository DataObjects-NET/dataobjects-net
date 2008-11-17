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
      var originalRange = provider.CompiledRange.Invoke();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var rangeProvider = new SqlRangeProvider(provider, request, Handlers, originalRange, source);

      if (originalRange.EndPoints.First.HasValue) {
        for (int i = 0; i < originalRange.EndPoints.First.Value.Count; i++) {
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler)Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          int fieldIndex = i;
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.First.Value.GetValue(fieldIndex), typeMapping);
          request.ParameterBindings.Add(binding);
          if (i == originalRange.EndPoints.First.Value.Count - 1) {
            switch (originalRange.EndPoints.First.ValueType) {
              case EntireValueType.Default:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
                break;
              case EntireValueType.PositiveInfinitesimal:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] > binding.SqlParameter;
                break;
              case EntireValueType.NegativeInfinitesimal:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
                break;
              case EntireValueType.PositiveInfinity:
                query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");
                break;
              case EntireValueType.NegativeInfinity:
                break;
            }
          }
          else
            query.Where &= query.Columns[keyColumns[i].Key] >= binding.SqlParameter;
        }
      }
      else if (originalRange.EndPoints.First.ValueType==EntireValueType.PositiveInfinity)
        query.Where &= SqlFactory.Native("1")==SqlFactory.Native("0");

      if (originalRange.EndPoints.Second.HasValue) {
        for (int i = 0; i < originalRange.EndPoints.Second.Value.Count; i++) {
          var column = provider.Header.Columns[keyColumns[i].Key];
          DataTypeMapping typeMapping = ((DomainHandler)Handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(column.Type);
          int fieldIndex = i;
          var binding = new SqlFetchParameterBinding(() => rangeProvider.CurrentRange.EndPoints.Second.Value.GetValue(fieldIndex), typeMapping);
          request.ParameterBindings.Add(binding);
          if (i == originalRange.EndPoints.Second.Value.Count - 1) {
            switch (originalRange.EndPoints.Second.ValueType) {
              case EntireValueType.Default:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
                break;
              case EntireValueType.PositiveInfinitesimal:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
                break;
              case EntireValueType.NegativeInfinitesimal:
                request.ParameterBindings.Add(binding);
                query.Where &= query.Columns[keyColumns[i].Key] < binding.SqlParameter;
                break;
              case EntireValueType.PositiveInfinity:
                break;
              case EntireValueType.NegativeInfinity:
                query.Where &= SqlFactory.Native("1") == SqlFactory.Native("0");
                break;
            }
          }
          else
            query.Where &= query.Columns[keyColumns[i].Key] <= binding.SqlParameter;
        }
      }
      else if (originalRange.EndPoints.First.ValueType==EntireValueType.PositiveInfinity)
        query.Where &= SqlFactory.Native("1")==SqlFactory.Native("0");

      return rangeProvider;
    }


    // Constructors

    public RangeProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}