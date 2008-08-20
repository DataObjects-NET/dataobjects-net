// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
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
    public static IUndoDescriptor CurrentDescriptor {
      [DebuggerStepThrough]
      get { return CurrentContext; }
    }

    public static UndoScope CreateBlockingScope()
    {
      if (CurrentDescriptor is BlockingUndoDescriptor)
        return null;
      else
        return new UndoScope(new BlockingUndoDescriptor());
    }

    public IUndoDescriptor Descriptor {
      [DebuggerStepThrough]
      get { return Context; }
    }

    internal new UndoScope OuterScope {
      [DebuggerStepThrough]
      get { return (UndoScope)base.OuterScope; }
    }

    public override void Activate(IUndoDescriptor newContext)
    {
      base.Activate(newContext);
      AtomicityScope.CurrentContext.OnActivateUndoScope(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    internal UndoScope(IUndoDescriptor descriptor) 
      : base(descriptor)
    {
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      AtomicityScope.CurrentContext.OnDeactivateUndoScope(this);
      base.Dispose(disposing);
    }
  }
}