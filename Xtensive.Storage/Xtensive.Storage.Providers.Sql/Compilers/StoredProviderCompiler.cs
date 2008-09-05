// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class StoredProviderCompiler : TypeCompiler<StoredProvider>
  {
    private const string TABLE_NAME_PATTERN = "Tmp_{0}";

    public StoredProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }

    protected override ExecutableProvider Compile(StoredProvider provider)
    {
      ExecutableProvider ex = null;
      DomainHandler domainHandler = (DomainHandler) Handlers.DomainHandler;
      Schema schema = domainHandler.Schema;
      Table table;
      string tableName = string.Format(TABLE_NAME_PATTERN, provider.Name);
      if (provider.Source!=null) {
        ex = provider.Source.Compile();
        if (provider.Scope==TemporaryDataScope.Global)
          table = schema.CreateTable(tableName);
        else
          table = schema.CreateTemporaryTable(tableName);

        foreach (Column column in provider.Header.Columns) {
          TableColumn tableColumn = table.CreateColumn(column.Name, domainHandler.GetSqlType(column.Type, null));
          tableColumn.IsNullable = true;
        }
      }
      else
        table = schema.Tables[tableName];

      SqlTableRef tr = SqlFactory.TableRef(table);
      SqlSelect query = SqlFactory.Select(tr);
      foreach (SqlTableColumn column in tr.Columns)
        query.Columns.Add(column);
      SqlQueryRequest request = new SqlQueryRequest(query, provider.Header.TupleDescriptor);
      schema.Tables.Remove(table);

      return new SqlStoredProvider(provider, request, Handlers, ex, table);
    }
  }
}