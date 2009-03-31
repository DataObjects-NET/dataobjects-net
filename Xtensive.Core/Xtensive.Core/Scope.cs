// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.27

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Disposing;

namespace Xtensive.Core
{
  /// <summary>
  /// Provides access to the thread-bound stack of contextual information (<typeparamref name="TContext"/>).
  /// </summary>
  /// <typeparam name="TContext">The type of the context.</typeparam>
  public class Scope<TContext>: IDisposable
    where TContext: class
  {
    [ThreadStatic]
    private static Scope<TContext> currentScope;

    private TContext context;
    private Scope<TContext> outerScope;

    /// <summary>
    /// Gets the current context of this type of scope.
    /// </summary>
    protected internal static TContext CurrentContext
    {
      [DebuggerStepThrough]
      get { return currentScope != null ? currentScope.context : default(TContext); }
    }

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    protected internal static Scope<TContext> CurrentScope
    {
      [DebuggerStepThrough]
      get { return currentScope; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    protected TContext Context
    {
      [DebuggerStepThrough]
      get { return context; }
    }

    /// <summary>
    /// Gets the outer <see cref="Scope{TContext}"/> of this instance.
    /// </summary>
    protected Scope<TContext> OuterScope
    {
      [DebuggerStepThrough]
      get { return outerScope; }
    }

    /// <summary>
    /// Gets a value indicating whether this scope is nested to another one.
    /// </summary>
    protected bool IsNested
    {
      [DebuggerStepThrough]
      get { return outerScope != null; }
    }

    
    // Initializer
    
    /// <summary>
    /// Initializes the scope.
    /// </summary>
    /// <param name="newContext">The new context.</param>
    public virtual void Activate(TContext newContext)
    {
      if (context!=null)
        throw Exceptions.AlreadyInitialized("Context");
      context = newContext;
      outerScope = currentScope;
      currentScope = this;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="context">The context of this scope.</param>
    protected internal Scope(TContext context)
    {
      Activate(context);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// Does not invoke <see cref="Activate"/> method.
    /// </summary>
    protected internal Scope()
    {
    }


    // Destructor

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) 
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      bool bStop = false;
      Exception error = null;
      while (!bStop) {
        try {
          if (currentScope==null) {
            bStop = true;
            throw new InvalidOperationException(Strings.ExScopeCantBeDisposed);
          }
          else if (currentScope==this) {
            bStop = true;
            currentScope.Dispose(true);
          }
          else
            currentScope.Dispose();
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
      currentScope = outerScope;
      context = null;
      outerScope = null;
      if (error!=null)
        throw error;
    }
  }
}