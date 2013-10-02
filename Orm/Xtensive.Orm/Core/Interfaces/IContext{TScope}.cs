// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Context contract.
  /// </summary>
  /// <typeparam name="TScope">The type of <see cref="Scope{TContext}"/> of the context.</typeparam>
  /// <seealso cref="Scope{TContext}"/>
  public interface IContext<TScope>: IContext
    where TScope: class, IDisposable
  {
    /// <summary>
    /// Activates the current context.
    /// </summary>
    /// <returns><typeparamref name="TScope"/> object (normally - <see cref="Scope{TContext}"/> descendant)
    /// that can be used to deactivate the context by disposing it.</returns>
    /// <seealso cref="IContext.IsActive"/>
    new TScope Activate();
  }
}