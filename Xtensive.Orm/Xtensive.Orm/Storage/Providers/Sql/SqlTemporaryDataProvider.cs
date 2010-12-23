// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using Xtensive.Core;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Abstract base class for any SQL temporary data provider.
  /// </summary>
  public abstract class SqlTemporaryDataProvider : SqlProvider
  {
    public const string TemporaryTableLockName = "TemporaryTableLockName";

    protected readonly TemporaryTableDescriptor tableDescriptor;
    
    protected void LockAndStore(Rse.Providers.EnumerationContext context, IEnumerable<Tuple> data)
    {
      var storageContext = (EnumerationContext)context;
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(storageContext, tableDescriptor);
      if (tableLock == null) 
        return;
      storageContext.SetValue(this, TemporaryTableLockName, tableLock);
      var executor = storageContext.SessionHandler.GetService<IQueryExecutor>(true);
      executor.Store(tableDescriptor, data);
    }

    protected bool ClearAndUnlock(Rse.Providers.EnumerationContext context)
    {
      var tableLock = context.GetValue<IDisposable>(this, TemporaryTableLockName);
      if (tableLock==null)
        return false;
      var storageContext = (EnumerationContext)context;
      using (tableLock)
        storageContext.SessionHandler.GetService<IQueryExecutor>(true)
          .Clear(tableDescriptor);
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