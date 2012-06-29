// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.27

using System;
using System.Diagnostics;
using System.Security;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Provides access to the thread-bound stack of contextual information
  /// (<typeparamref name="TVariator"/>).
  /// </summary>
  /// <typeparam name="TVariator">The type of the variator. Must be an internal type.</typeparam>
  public class SimpleScope<TVariator> : IDisposable
  {
    private readonly static object @lock = new object();
    private volatile static Type allowedType = null;
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
    /// <exception cref="SecurityException">Only one ancestor of each instance 
    /// of this generic type is allowed.</exception>
    protected internal SimpleScope()
    {
      if (allowedType==null) lock (@lock) if (allowedType==null)
        allowedType = GetType();
      if (allowedType!=GetType())
        throw new SecurityException(
          Strings.ExOnlyOneAncestorOfEachInstanceOfThisGenericTypeIsAllowed);
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