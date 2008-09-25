// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.18

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SeekProviderCompiler : TypeCompiler<SeekProvider>
  {
    protected override ExecutableProvider Compile(SeekProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      var request = new SqlFetchRequest(query, provider.Header.TupleDescriptor, source.Request.Parameters);
      var typeIdColumnName = Handlers.NameBuilder.TypeIdColumnName;
      Func<KeyValuePair<int, Direction>, bool> filterNonTypeId = 
        pair => ((MappedColumn)provider.Header.Columns[pair.Key]).ColumnInfoRef.ColumnName!=typeIdColumnName;
      var keyColumns = provider.Header.Order
        .Where(filterNonTypeId)
        .ToList();
      
      for (int i = 0; i < keyColumns.Count; i++) {
        int columnIndex = keyColumns[i].Key;
        var sqlColumn = query.Columns[columnIndex];
        var column = provider.Header.Columns[columnIndex] as MappedColumn;
        int index = i;
        var cir = column != null ? column.ColumnInfoRef : null;
        var binding = new SqlFetchRequestParameter(cir, () => provider.Key().GetValue(index));
        request.Parameters.Add(binding);
        query.Where &= sqlColumn == SqlFactory.ParameterRef(binding.Parameter);
      }

      return new SqlProvider(provider, request, Handlers, source);
    }

    public SeekProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}