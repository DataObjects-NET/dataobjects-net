// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Providers.Compilable.Interfaces;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlCacheProvider : ExecutableProvider
  {
    private readonly HandlerAccessor handlerAccessor;
    private TemporaryTable temporaryTable;
    
    protected override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      SaveProvider provider = (SaveProvider) Origin;

      if (provider.ResultName!=null) {
        Schema schema = ((DomainHandler) handlerAccessor.DomainHandler).Schema;
        temporaryTable = schema.CreateTemporaryTable(provider.ResultName);
        SqlBatch batch = SqlFactory.Batch();
        if (provider.SourceProvider!=null) {
          
        }
        batch.Add(SqlFactory.Create(temporaryTable));
      }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      throw new System.NotImplementedException();
    }

    public SqlCacheProvider(CompilableProvider origin, HandlerAccessor handlerAccessor,params ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.handlerAccessor = handlerAccessor;
    }
  }
}