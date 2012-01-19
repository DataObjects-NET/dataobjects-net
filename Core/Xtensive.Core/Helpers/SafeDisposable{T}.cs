// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Helpers
{
  /// <summary>
  /// <inheritdoc />
  /// Has finalizer (i.e. this type is safer then its base).
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public sealed class SafeDisposable<T>: Disposable<T>
  {
    // Constructors

    /// <inheritdoc/>
    public SafeDisposable(T parameter, Action<bool, T> onDispose)
      : base(parameter, onDispose)
    {
    }

    // Finalizer

    /// <see cref="ClassDocTemplate.Dtor" copy="true" />
    ~SafeDisposable()
    {
      Dispose(false);
    }
  }
}
