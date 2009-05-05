// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using RseEnumerationScope=Xtensive.Storage.Rse.Providers.EnumerationScope;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An wrapper of <see cref="Rse.Providers.EnumerationScope"/> 
  /// suitable for storage.
  /// </summary>
  public class EnumerationScope : SimpleScope<EnumerationScope>
  {
    private JoiningDisposable toDispose;


    // Constructors

    /// <inheritdoc/>
    public EnumerationScope(Rse.Providers.EnumerationContext context)
    {
      toDispose = RseEnumerationScope.Open().Join(Transaction.Open());
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