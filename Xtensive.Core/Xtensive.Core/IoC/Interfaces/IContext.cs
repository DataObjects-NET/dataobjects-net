// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;

namespace Xtensive.IoC
{
  /// <summary>
  /// Base interface for any <see cref="IContext{TScope}"/> contract.
  /// </summary>
  /// <seealso cref="IContext{TScope}"/>
  /// <seealso cref="Scope{TContext}"/>
  public interface IContext
  {
    /// <summary>
    /// Indicates whether current context is active.
    /// </summary>
    /// <seealso cref="Activate"/>
    bool IsActive { get; }
    
    /// <summary>
    /// Activates the current context.
    /// </summary>
    /// <returns><see cref="IDisposable"/> object (normally - <see cref="Scope{TContext}"/> descendant)
    /// that can be used to deactivate the context by disposing it.</returns>
    /// <seealso cref="IsActive"/>
    IDisposable Activate();
  }
}