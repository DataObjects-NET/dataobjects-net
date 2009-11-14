// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    private readonly EnumerationContextOptions options;

    /// <inheritdoc/>
    public override EnumerationContextOptions Options { get { return options; } }

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
      return new EnumerationContext(options);
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
    /// <param name="options">A value for <see cref="Options"/>.</param>
    public EnumerationContext(EnumerationContextOptions options)
    {
      this.options = options;
    }
  }
}