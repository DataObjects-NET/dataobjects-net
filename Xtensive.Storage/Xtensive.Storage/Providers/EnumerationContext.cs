// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    /// <inheritdoc/>
    public override GlobalTemporaryData GlobalTemporaryData {
      get {
        var domain = Domain.Current;
        return domain!=null ? domain.TemporaryData : null;
      }
    }

    /// <inheritdoc/>
    public override TransactionTemporaryData TransactionTemporaryData {
      get {
        var transaction = Transaction.Current;
        return transaction!=null ? transaction.TemporaryData : null;
      }
    }

    /// <inheritdoc/>
    protected override Rse.Providers.EnumerationScope CreateActiveScope()
    {
      return new EnumerationScope(this);
    }

    /// <inheritdoc/>
    public override Rse.Providers.EnumerationScope Activate()
    {
      Session.Current.Persist();
      return base.Activate();
    }
  }
}