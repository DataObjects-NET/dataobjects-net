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
    protected override ExecutableProvider Compile(SeekProvider provider)
    {
      var source = Compiler.Compile(provider.Source, true) as SqlProvider;
      if (source == null)
        return null;

      var query = source.Query.Clone() as SqlSelect;
      var keyColumns = provider.Header.Order.Select(pair => query.Columns[pair.Key]).ToList();
      var key = provider.CompiledKey.Invoke();

      SqlProvider result = new SqlProvider(provider, query, Handlers);
      for (int i = 0; i < keyColumns.Count; i++) {
        SqlParameter p = new SqlParameter();
        int index = i;
        result.Parameters.Add(p, () => key.GetValue(index));
        query.Where &= keyColumns[i] == SqlFactory.ParameterRef(p);
      }
      return result;
    }

    public SeekProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}