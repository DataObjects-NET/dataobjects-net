// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.27

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Provides access to the thread-bound stack of contextual information
  /// (<typeparamref name="TVariator"/>).
  /// </summary>
  /// <typeparam name="TVariator">The type of the variator. Must be an internal type.</typeparam>
  public class SimpleScope<TVariator> : IDisposable
  {
    [ThreadStatic]
    private static SimpleScope<TVariator> current;
    private SimpleScope<TVariator> outer;

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    protected internal static SimpleScope<TVariator> Current
    {
      [DebuggerStepThrough]
      get { return current; }
    }

    /// <summary>
    /// Gets the outer <see cref="Scope{TContext}"/> of this instance.
    /// </summary>
    protected SimpleScope<TVariator> Outer
    {
      [DebuggerStepThrough]
      get { return outer; }
    }

    /// <summary>
    /// Gets a value indicating whether this scope is nested to another one.
    /// </summary>
    protected bool IsNested
    {
      [DebuggerStepThrough]
      get { return outer != null; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected internal SimpleScope()
    {
      outer = current;
      current = this;
    }


    // Destructor


    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)"/>
    /// </summary>
    protected virtual void Dispose(bool disposing) 
    {
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Scope can't be disposed.</exception>
    public void Dispose()
    {
      bool bStop = false;
      Exception error = null;
      while (!bStop) {
        try {
          if (current==null) {
            bStop = true;
            throw new InvalidOperationException(Strings.ExScopeCantBeDisposed);
          }
          else if (current==this) {
            bStop = true;
            current.Dispose(true);
          }
          else
            current.Dispose();
        }
        catch (Exception e) {
          if (error==null)
            error = e;
          try {
            Log.Error(e, Strings.LogScopeDisposeError);
          }
          catch {}
        }
      }
      current = outer;
      outer = null;
      if (error!=null)
        throw error;
    }
  }
}