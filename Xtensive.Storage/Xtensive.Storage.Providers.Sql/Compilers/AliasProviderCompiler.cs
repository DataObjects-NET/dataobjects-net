// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System.Linq;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class AliasProviderCompiler : TypeCompiler<AliasProvider>
  {
    /// <inheritdoc/>
    protected override ExecutableProvider Compile(AliasProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var sourceQuery = (SqlSelect)source.Request.Statement;
      var queryRef  = SqlFactory.QueryRef(sourceQuery);
      var sqlSelect = SqlFactory.Select(queryRef);
      for (int i = 0; i < queryRef.Columns.Count; i++)
        sqlSelect.Columns.Add(queryRef.Columns[i], provider.Header.Columns[i].Name);
      var request = new SqlQueryRequest(sqlSelect, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructor

    public AliasProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}