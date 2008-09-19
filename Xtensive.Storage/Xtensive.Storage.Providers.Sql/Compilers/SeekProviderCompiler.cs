// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.18

using System.Linq;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Compilation;
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
      var request = new SqlFetchRequest(query, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      var keyColumns = provider.Header.Order.Select(pair => query.Columns[pair.Key]).ToList();
      for (int i = 0; i < keyColumns.Count; i++) {
        var p = new SqlParameter();
        int index = i;
        request.ParameterBindings.Add(p, () => provider.Key().GetValue(index));
        query.Where &= keyColumns[i] == SqlFactory.ParameterRef(p);
      }

      return new SqlProvider(provider, request, Handlers, source);
    }

    public SeekProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}