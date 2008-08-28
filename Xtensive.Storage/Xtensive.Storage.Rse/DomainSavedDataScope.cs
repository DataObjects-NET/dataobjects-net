// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Domain saved data scope.
  /// </summary>
  public class DomainSavedDataScope : Scope<DomainSavedData>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static DomainSavedData CurrentContext
    {
      [DebuggerStepThrough]
      get { return Scope<DomainSavedData>.CurrentContext; }
    }

    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static DomainSavedDataScope CurrentScope
    {
      [DebuggerStepThrough]
      get { return (DomainSavedDataScope) Scope<DomainSavedData>.CurrentScope; }
    }


    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new DomainSavedData Context
    {
      [DebuggerStepThrough]
      get { return base.Context; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The <see cref="Scope{TContext}.Context"/> property value.</param>
    [DebuggerStepThrough]
    public DomainSavedDataScope(DomainSavedData context)
      : base(context)
    {
    }
  }
}