// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.02

using System.Linq;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class LoadProviderCompiler : TypeCompiler<LoadProvider>
  {
    protected override ExecutableProvider Compile(LoadProvider provider)
    {
      SqlTableRef tableRef = null;
      SqlSaveProvider saveProvider = null;
        if (provider.Sources != null && provider.Sources.Count() > 0) {
        SaveProvider sp = provider.Sources[0] as SaveProvider;
        if (sp != null) {
          saveProvider = sp.Compile() as SqlSaveProvider;
          tableRef = saveProvider.Data.Table;
        }
      }
      if (tableRef == null) {
        Table table = ((DomainHandler)Handlers.DomainHandler).Schema.Tables[provider.Name];
        tableRef = SqlFactory.TableRef(table);
      }
      SqlSelect query = SqlFactory.Select(tableRef);
      foreach (SqlTableColumn column in tableRef.Columns)
        query.Columns.Add(column);
      SqlQueryRequest request = new SqlQueryRequest(query, provider.Header.TupleDescriptor);
      return new SqlLoadProvider(provider, request, Handlers, saveProvider);
    }


    // Constructors

    public LoadProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}