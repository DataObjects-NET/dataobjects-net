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
  /// <see cref="DomainLevelTemporaryData"/> activation scope.
  /// </summary>
  public class DomainLevelTemporaryDataScope : Scope<DomainLevelTemporaryData>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static DomainLevelTemporaryData CurrentContext
    {
      [DebuggerStepThrough]
      get { return Scope<DomainLevelTemporaryData>.CurrentContext; }
    }

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public new static DomainLevelTemporaryDataScope CurrentScope
    {
      [DebuggerStepThrough]
      get { return (DomainLevelTemporaryDataScope) Scope<DomainLevelTemporaryData>.CurrentScope; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new DomainLevelTemporaryData Context
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
    public DomainLevelTemporaryDataScope(DomainLevelTemporaryData context)
      : base(context)
    {
    }
  }
}