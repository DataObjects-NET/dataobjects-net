// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Helpers
{
  /// <summary>
  /// <inheritdoc />
  /// Has finalizer (i.e. this type is safer then its base).
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public sealed class SafeLeasedAccessor<T>: LeasedAccessor<T>
  {
    // Constructors

    /// <inheritdoc/>
    public SafeLeasedAccessor(T leased, Action<bool, T> onDispose)
      : base(leased, onDispose)
    {
    }

    // Finalizer

    /// <see cref="ClassDocTemplate.Dtor" copy="true" />
    ~SafeLeasedAccessor()
    {
      Dispose(false);
    }
  }
}
