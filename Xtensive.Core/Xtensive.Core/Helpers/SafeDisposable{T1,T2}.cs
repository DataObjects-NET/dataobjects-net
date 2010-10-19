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
  /// <typeparam name="T1"><inheritdoc/></typeparam>
  /// <typeparam name="T2"><inheritdoc/></typeparam>
  public sealed class SafeDisposable<T1,T2>: Disposable<T1,T2>
  {
    // Constructors

    /// <inheritdoc/>
    public SafeDisposable(T1 parameter1, T2 parameter2, Action<bool, T1, T2> onDispose)
      : base(parameter1, parameter2, onDispose)
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
