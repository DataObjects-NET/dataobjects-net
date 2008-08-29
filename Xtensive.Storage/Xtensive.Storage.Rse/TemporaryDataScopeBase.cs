// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse
{
  public class TemporaryDataScopeBase : Scope<TemporaryDataBase>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static TemporaryDataBase CurrentContext
    {
      [DebuggerStepThrough]
      get { return Scope<TemporaryDataBase>.CurrentContext; }
    }

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public new static TemporaryDataScopeBase CurrentScope
    {
      [DebuggerStepThrough]
      get { return (TemporaryDataScopeBase) Scope<TemporaryDataBase>.CurrentScope; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new TemporaryDataBase Context
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
    public TemporaryDataScopeBase(TemporaryDataBase context)
      : base(context)
    {
    }
  }
}