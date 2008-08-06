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
  /// Undo scope - provides access to <see cref="CurrentDescriptor"/> 
  /// (current <see cref="IUndoDescriptor"/>).
  /// </summary>
  public class UndoScope: Scope<IUndoDescriptor>
  {
    [DebuggerHidden]
    public static IUndoDescriptor CurrentDescriptor {
      get {
        return CurrentContext;
      }
    }

    public static UndoScope CreateBlockingScope()
    {
      if (CurrentDescriptor is BlockingUndoDescriptor)
        return null;
      else
        return new UndoScope(new BlockingUndoDescriptor());
    }

    [DebuggerHidden]
    public IUndoDescriptor Descriptor
    {
      get { return Context; }
    }

    [DebuggerHidden]
    internal new UndoScope OuterScope
    {
      get { return (UndoScope)base.OuterScope; }
    }

    public override void Activate(IUndoDescriptor newContext)
    {
      base.Activate(newContext);
      AtomicityScope.CurrentContext.OnActivateUndoScope(this);
    }


    // Constructors

    internal UndoScope(IUndoDescriptor descriptor) 
      : base(descriptor)
    {
    }

    // Destructor


    protected override void Dispose(bool disposing)
    {
      AtomicityScope.CurrentContext.OnDeactivateUndoScope(this);
      base.Dispose(disposing);
    }
  }
}