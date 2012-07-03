// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using System.Collections.Generic;
using Xtensive.Core;

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
        while (finalizationQueue.Count > 0) {
          var materializeSelf = finalizationQueue.Dequeue();
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

    private readonly EnumerationContextOptions options;

    /// <summary>
    /// Gets the session handler.
    /// </summary>
    /// <value>The session handler.</value>
    public Session Session { get; private set; }

    /// <inheritdoc/>
    protected override EnumerationContextOptions Options { get { return options; } }

    internal MaterializationContext MaterializationContext { get; set; }

    /// <inheritdoc/>
    public override ICompletableScope BeginEnumeration()
    {
      var tx = Session.OpenAutoTransaction();
      Session.EnsureTransactionIsStarted();
      if (MaterializationContext!=null && MaterializationContext.MaterializationQueue!=null)
        return new EnumerationFinalizer(MaterializationContext.MaterializationQueue, tx);
      return tx;
    }

    /// <inheritdoc/>
    protected override Rse.Providers.EnumerationScope CreateActiveScope()
    {
      return new EnumerationScope(this);
    }


    // Constructors

    internal EnumerationContext(Session session, EnumerationContextOptions options)
    {
      Session = session;

      this.options = options;
    }

    internal EnumerationContext(Session session)
    {
      Session = session;
    }
  }
}