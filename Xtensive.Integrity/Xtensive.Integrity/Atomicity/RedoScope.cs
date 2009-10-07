// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;
using Xtensive.Integrity.Atomicity.Internals;
using Xtensive.Integrity.Aspects;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Redo scope - provides access to <see cref="CurrentDescriptor"/> 
  /// (current <see cref="IRedoDescriptor"/>).
  /// </summary>
  public sealed class RedoScope: Scope<IRedoDescriptor>
  {
    public static IRedoDescriptor CurrentDescriptor {
      [DebuggerStepThrough]
      get {
        return CurrentContext;
      }
    }

    public static RedoScope CreateBlockingScope()
    {
      if (CurrentDescriptor is BlockingRedoDescriptor)
        return null;
      else
        return new RedoScope(new BlockingRedoDescriptor());
    }

    public IRedoDescriptor Descriptor {
      [DebuggerStepThrough]
      get { return Context; }
    }

    internal new RedoScope OuterScope {
      [DebuggerStepThrough]
      get { return (RedoScope)base.OuterScope; }
    }
    
    public override void Activate(IRedoDescriptor newContext)
    {
      base.Activate(newContext);
      AtomicityScope.CurrentContext.OnActivateRedoScope(this);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor">The redo descriptor.</param>
    internal RedoScope(IRedoDescriptor descriptor) 
      : base(descriptor)
    {
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      try {
        AtomicityScope.CurrentContext.OnDeactivateRedoScope(this);
      }
      finally {
        base.Dispose(disposing);
      }           
    }
  }
}