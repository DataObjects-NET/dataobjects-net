// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    private readonly bool preloadEnumerator;

    /// <inheritdoc/>
    public override bool PreloadEnumerator { get { return preloadEnumerator; } }

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
    public override Rse.Providers.EnumerationContext CreateNew()
    {
      return new EnumerationContext(preloadEnumerator);
    }

    /// <inheritdoc/>
    protected override Rse.Providers.EnumerationScope CreateActiveScope()
    {
      return new EnumerationScope(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="preloadEnumerator">A value for <see cref="PreloadEnumerator"/>.</param>
    public EnumerationContext(bool preloadEnumerator)
    {
      this.preloadEnumerator = preloadEnumerator;
    }
  }
}