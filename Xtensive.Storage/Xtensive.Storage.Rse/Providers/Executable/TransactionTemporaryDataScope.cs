// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// <see cref="TransactionTemporaryData"/> activation scope.
  /// </summary>
  public class TransactionTemporaryDataScope : Scope<TransactionTemporaryData>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static TransactionTemporaryData CurrentContext
    {
      [DebuggerStepThrough]
      get { return Scope<TransactionTemporaryData>.CurrentContext; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The <see cref="Scope{TContext}.Context"/> property value.</param>
    [DebuggerStepThrough]
    public TransactionTemporaryDataScope(TransactionTemporaryData context)
      : base(context)
    {
    }
  }
}