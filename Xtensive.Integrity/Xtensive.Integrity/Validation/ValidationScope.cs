// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.05

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

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
    public static new ValidationContextBase CurrentContext {
      [DebuggerStepThrough]
      get { return Scope<ValidationContextBase>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new ValidationContextBase Context {
      [DebuggerStepThrough]
      get { return base.Context; }
    }

    internal new ValidationScope OuterScope {
      [DebuggerStepThrough]
      get { return (ValidationScope)base.OuterScope; }
    }
    
    public override void Activate(ValidationContextBase newContext)
    {
      base.Activate(newContext);
      newContext.OnActivate(this);
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The validation context.</param>
    public ValidationScope(ValidationContextBase context) 
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ValidationScope()
    {
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
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