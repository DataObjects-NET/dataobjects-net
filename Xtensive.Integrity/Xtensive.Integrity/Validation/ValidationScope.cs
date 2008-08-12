// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// <see cref="ValidationContextBase"/> activation scope.
  /// </summary>
  public class ValidationScope: Scope<ValidationContextBase>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    [DebuggerStepThrough]
    public static new ValidationContextBase CurrentContext {
      get {
        return Scope<ValidationContextBase>.CurrentContext;
      }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    [DebuggerStepThrough]
    public new ValidationContextBase Context
    {
      get { return base.Context; }
    }

    [DebuggerStepThrough]
    internal new ValidationScope OuterScope
    {
      get { return (ValidationScope)base.OuterScope; }
    }
    
    public override void Activate(ValidationContextBase newContext)
    {
      base.Activate(newContext);
      newContext.OnActivate(this);
    }


    // Constructors

    public ValidationScope(ValidationContextBase context) 
      : base(context)
    {
    }

    public ValidationScope()
    {
    }

    // Destructor

    protected override void Dispose(bool disposing)
    {
      try {
        Context.OnDeactivate(this);
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}