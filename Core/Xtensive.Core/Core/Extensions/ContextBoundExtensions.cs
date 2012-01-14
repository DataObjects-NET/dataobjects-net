// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.27

using System;
using System.Diagnostics;
using Xtensive.IoC;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="IContextBound{TContext}"/> related extension methods.
  /// </summary>
  public static class ContextBoundExtensions
  {
    /// <summary>
    /// Activates the context of <typeparamref name="TContext"/> type
    /// of specified <paramref name="contextBound"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of context to activate.</typeparam>
    /// <param name="contextBound">The object to activate the context of.</param>
    /// <returns><see cref="IDisposable"/> object (normally - <see cref="Scope{TContext}"/> descendant)
    /// that can be used to deactivate the context by disposing it.</returns>
    [DebuggerStepThrough]
    public static IDisposable ActivateContext<TContext>(this IContextBound<TContext> contextBound)
      where TContext: class, IContext
    {
      if (contextBound==null)
        return null;
      TContext context = contextBound.Context;
      if (context==null)
        return null;
      return context.Activate();
    }
  }
}
