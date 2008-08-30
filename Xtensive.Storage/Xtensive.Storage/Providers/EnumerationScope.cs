// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Core.Disposable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationScope"/> 
  /// suitable for storage.
  /// </summary>
  public class EnumerationScope : Rse.Providers.EnumerationScope
  {
    private IDisposable toDispose;

    /// <inheritdoc/>
    public override void Activate(Rse.Providers.EnumerationContext newContext)
    {
      base.Activate(newContext);
      toDispose = Transaction.Open();
    }

    // Constructors

    /// <inheritdoc/>
    public EnumerationScope(Rse.Providers.EnumerationContext context)
      : base(context)
    {
    }

    // Desctructor

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        toDispose.DisposeSafely();
        toDispose = null;
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}