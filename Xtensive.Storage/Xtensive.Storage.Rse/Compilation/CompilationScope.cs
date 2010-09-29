// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="CompilationContext"/> activation scope. 
  /// </summary>
  public sealed class CompilationScope : Scope<CompilationContext>
  {
    private IDisposable toDispose;

    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static CompilationContext CurrentContext
    {
      get { return Scope<CompilationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new CompilationContext Context
    {
      get { return base.Context; }
    }

    /// <inheritdoc/>
    public override void Activate(CompilationContext newContext)
    {
      base.Activate(newContext);
      // We must close the EnumerationScope it 
      // to ensure next EnumerationScope.Open() call 
      // will return a new one
      toDispose = EnumerationScope.Block(); 
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public CompilationScope(CompilationContext context)
      : base(context)
    {
    }

    // Desctructor

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        var disposable = toDispose;
        toDispose = null;
        disposable.DisposeSafely();
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}