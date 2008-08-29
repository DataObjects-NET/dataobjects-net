// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// <see cref="GlobalTemporaryData"/> activation scope.
  /// </summary>
  public class GlobalTemporaryDataScope : Scope<GlobalTemporaryData>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static GlobalTemporaryData CurrentContext
    {
      [DebuggerStepThrough]
      get { return Scope<GlobalTemporaryData>.CurrentContext; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The <see cref="Scope{TContext}.Context"/> property value.</param>
    [DebuggerStepThrough]
    public GlobalTemporaryDataScope(GlobalTemporaryData context)
      : base(context)
    {
    }
  }
}