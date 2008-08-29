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
  public class TransactionLevelTemporaryDataScope : TemporaryDataScopeBase
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The <see cref="Scope{TContext}.Context"/> property value.</param>
    [DebuggerStepThrough]
    public TransactionLevelTemporaryDataScope(TemporaryDataBase context)
      : base(context)
    {
    }
  }
}