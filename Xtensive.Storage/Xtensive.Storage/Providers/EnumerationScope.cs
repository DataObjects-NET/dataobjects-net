// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Core.Disposing;
using RseEnumerationScope=Xtensive.Storage.Rse.Providers.EnumerationScope;
using RseEnumerationContext=Xtensive.Storage.Rse.Providers.EnumerationContext;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationScope"/> 
  /// suitable for storage.
  /// </summary>
  public class EnumerationScope : RseEnumerationScope
  {
    private IDisposable toDispose;

    /// <inheritdoc/>
    public override void Activate(RseEnumerationContext newContext)
    {
      base.Activate(newContext);
      toDispose = Transaction.Open();
    }

    // Constructors

    /// <inheritdoc/>
    public EnumerationScope(RseEnumerationContext context)
      : base(context)
    {
    }

    // Desctructor

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        var disposable = toDispose;
        toDispose = null;
        disposable.DisposeSafely();
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}