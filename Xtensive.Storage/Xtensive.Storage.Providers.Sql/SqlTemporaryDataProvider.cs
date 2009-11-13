// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlTemporaryDataProvider : SqlProvider
  {
    private const string TemporaryTableLockName = "TemporaryTableLockName";

    private readonly TemporaryTableDescriptor tableDescriptor;
    
    protected void LockAndStore(Rse.Providers.EnumerationContext context, IEnumerable<Tuple> data)
    {
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(tableDescriptor);
      context.SetValue(this, TemporaryTableLockName, tableLock);
      var executor = handlers.SessionHandler.GetService<IQueryExecutor>();
      executor.Store(tableDescriptor, data);
    }

    protected bool ClearAndUnlock(Rse.Providers.EnumerationContext context)
    {
      var tableLock = context.GetValue<IDisposable>(this, TemporaryTableLockName);
      if (tableLock==null)
        return false;
      using (tableLock)
        handlers.SessionHandler.GetService<IQueryExecutor>().Clear(tableDescriptor);
      return true;
    }


    // Constructors

    protected SqlTemporaryDataProvider(
      HandlerAccessor handlers, QueryRequest request, TemporaryTableDescriptor tableDescriptor,
      CompilableProvider origin, ExecutableProvider[] sources)
      : base(handlers, request, origin, sources)
    {
      this.tableDescriptor = tableDescriptor;
    }
  }
}