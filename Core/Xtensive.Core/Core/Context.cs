// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.15

using System;
using System.Diagnostics;

namespace Xtensive.Core
{
  /// <summary>
  /// Base <see cref="IContext{TScope}"/> implementation.
  /// To be used with various <see cref="Scope{TContext}"/> descendants.
  /// </summary>
  /// <typeparam name="TScope">The type of the associated scope.</typeparam>
  public abstract class Context<TScope>: IContext<TScope>
    where TScope: class, IDisposable
  {
    /// <inheritdoc/>
    public abstract bool IsActive { get; }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public virtual TScope Activate()
    {
      if (!IsActive)
        return CreateActiveScope();
      else
        return null;
    }

    /// <summary>
    /// Creates the associated scope.
    /// </summary>
    /// <returns>New instance of the <typeparamref name="TScope"/> type.</returns>
    protected abstract TScope CreateActiveScope();
  }
}
