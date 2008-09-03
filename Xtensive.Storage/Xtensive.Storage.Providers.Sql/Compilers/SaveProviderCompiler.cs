// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.02

using System;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Compilation;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SaveProviderCompiler : TypeCompiler<SaveProvider>
  {
    protected override ExecutableProvider Compile(SaveProvider provider)
    {
      DomainHandler domainHandler = (DomainHandler) Handlers.DomainHandler;
      var executableSource = provider.Source.Compile();
      SqlBatch beforeStep = SqlFactory.Batch();
      string name = provider.Name.IsNullOrEmpty() ? GenerateTemporaryName() : provider.Name;
      var tmp = domainHandler.Schema.CreateTemporaryTable(string.Format("Tmp_{0}", name));
      tmp.IsGlobal = provider.Scope==TemporaryDataScope.Global;
      foreach (Column column in provider.Header.Columns) {
        var tableColumn = tmp.CreateColumn(column.Name, domainHandler.GetSqlType(column.Type, null));
        tableColumn.IsNullable = true;
      }
      beforeStep.Add(SqlFactory.Create(tmp));

      SqlBatch afterStep = SqlFactory.Batch();
      afterStep.Add(SqlFactory.Drop(tmp));

      SqlSaveProviderData data = new SqlSaveProviderData();
      data.BeforeEnumerate = beforeStep;
      data.AfterEnumerate = afterStep;
      data.Table = SqlFactory.TableRef(tmp);

      return new SqlSaveProvider(provider, Handlers, executableSource, data);
    }


    private static string GenerateTemporaryName()
    {
      return Guid.NewGuid().ToString();
    }


    // Constructors

    public SaveProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}