// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Integrity.Atomicity.Internals;
using Xtensive.Integrity.Aspects;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Redo scope - provides access to <see cref="CurrentDescriptor"/> 
  /// (current <see cref="IRedoDescriptor"/>).
  /// </summary>
  public class RedoScope: Scope<IRedoDescriptor>
  {
    [DebuggerHidden]
    public static IRedoDescriptor CurrentDescriptor {
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

    [DebuggerHidden]
    public IRedoDescriptor Descriptor
    {
      get { return Context; }
    }

    [DebuggerHidden]
    internal new RedoScope OuterScope
    {
      get { return (RedoScope)base.OuterScope; }
    }
    
    public override void Activate(IRedoDescriptor newContext)
    {
      base.Activate(newContext);
      AtomicityScope.CurrentContext.OnActivateRedoScope(this);
    }


    // Constructors

    internal RedoScope(IRedoDescriptor descriptor) 
      : base(descriptor)
    {
    }

    protected override void Dispose(bool disposing)
    {
      AtomicityScope.CurrentContext.OnDeactivateRedoScope(this);
      base.Dispose(disposing);
    }
  }
}