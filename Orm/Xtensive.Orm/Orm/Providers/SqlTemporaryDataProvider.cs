// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using Xtensive.Core;
using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Providers;
using System.Threading.Tasks;
using System.Threading;

namespace Xtensive.Orm.Providers
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
      var storageContext = (EnumerationContext) context;
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(storageContext, tableDescriptor);
      if (tableLock == null) 
        return;
      storageContext.SetValue(this, TemporaryTableLockName, tableLock);
      var executor = storageContext.Session.Services.Demand<IProviderExecutor>();
      executor.Store(tableDescriptor, data, storageContext.ParameterContext);
    }

    protected async ValueTask LockAndStoreAsync(Rse.Providers.EnumerationContext context, IEnumerable<Tuple> data, CancellationToken token)
    {
      var storageContext = (EnumerationContext) context;
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(storageContext, tableDescriptor);
      if (tableLock == null)
        return;
      storageContext.SetValue(this, TemporaryTableLockName, tableLock);
      var executor = storageContext.Session.Services.Demand<IProviderExecutor>();
      await executor.StoreAsync(tableDescriptor, data, storageContext.ParameterContext, token).ConfigureAwaitFalse();
    }

    protected bool ClearAndUnlock(Rse.Providers.EnumerationContext context)
    {
      var tableLock = context.GetValue<IDisposable>(this, TemporaryTableLockName);
      if (tableLock==null)
        return false;
      var storageContext = (EnumerationContext) context;
      using (tableLock)
        storageContext.Session.Services.Demand<IProviderExecutor>().Clear(tableDescriptor, storageContext.ParameterContext);
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
