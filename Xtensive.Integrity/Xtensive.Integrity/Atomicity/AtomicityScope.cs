// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Integrity.Aspects;
using Xtensive.Core.Helpers;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// <see cref="AtomicityContextBase"/> activation scope.
  /// </summary>
  public class AtomicityScope: Scope<AtomicityContextBase>
  {
    private RedoScope cleanRedoScope;
    private UndoScope cleanUndoScope;

    public static new AtomicityContextBase CurrentContext {
      [DebuggerStepThrough]
      get { return Scope<AtomicityContextBase>.CurrentContext; }
    }

    public new AtomicityContextBase Context {
      [DebuggerStepThrough]
      get { return base.Context; }
    }

    internal new AtomicityScope OuterScope {
      [DebuggerStepThrough]
      get { return (AtomicityScope)base.OuterScope; }
    }

    public override void Activate(AtomicityContextBase newContext)
    {
      base.Activate(newContext);
      newContext.OnActivate(this);
      cleanRedoScope = new RedoScope(null);
      cleanUndoScope = new UndoScope(null);
    }


    // Constructors

    public AtomicityScope(AtomicityContextBase context) 
      : base(context)
    {
    }

    public AtomicityScope() 
    {
    }

    // Destructor

    protected override void Dispose(bool disposing)
    {
      cleanUndoScope.DisposeSafely();
      cleanRedoScope.DisposeSafely();
      try {
        Context.OnDeactivate(this);
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}