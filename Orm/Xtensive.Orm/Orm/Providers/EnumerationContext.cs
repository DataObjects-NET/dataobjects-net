// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An implementation of <see cref="Xtensive.Orm.Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    private class EnumerationFinalizer : ICompletableScope
    {
      private readonly Queue<Action> finalizationQueue;
      private readonly TransactionScope transactionScope;

      public void Complete()
      {
        if (IsCompleted)
          return;
        IsCompleted = true;
        transactionScope.Complete();
      }

      public bool IsCompleted { get; private set; }

      public void Dispose()
      {
        while (finalizationQueue.TryDequeue(out var materializeSelf)) {
          materializeSelf.Invoke();
        }
        transactionScope.DisposeSafely();
      }

      public EnumerationFinalizer(Queue<Action> finalizationQueue, TransactionScope transactionScope)
      {
        this.finalizationQueue = finalizationQueue;
        this.transactionScope = transactionScope;
      }
    }

    private readonly ParameterContext parameterContext;
    private readonly EnumerationContextOptions options;

    /// <summary>
    /// Gets the session handler.
    /// </summary>
    /// <value>The session handler.</value>
    public Session Session { get; private set; }

    /// <inheritdoc/>
    protected override EnumerationContextOptions Options { get { return options; } }

    internal MaterializationContext MaterializationContext { get; set; }

    public ParameterContext ParameterContext => parameterContext;

    /// <inheritdoc/>
    public override ICompletableScope BeginEnumeration()
    {
      var tx = Session.OpenAutoTransaction();
      if (!Session.Configuration.Supports(SessionOptions.NonTransactionalReads))
        Session.DemandTransaction();
      if (MaterializationContext!=null && MaterializationContext.MaterializationQueue!=null)
        return new EnumerationFinalizer(MaterializationContext.MaterializationQueue, tx);
      return tx;
    }

    // Constructors

    internal EnumerationContext(Session session, ParameterContext parameterContext, EnumerationContextOptions options = default)
    {
      Session = session;

      this.parameterContext = parameterContext;
      this.options = options;
    }
  }
}