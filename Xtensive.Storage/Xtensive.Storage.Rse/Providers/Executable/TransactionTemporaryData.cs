// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System.Diagnostics;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Transaction (<see cref="Transaction"/>-level) temporary data context.
  /// </summary>
  public class TransactionTemporaryData : TemporaryDataBase<TransactionTemporaryDataScope>
  {
    /// <summary>
    /// Gets the current <see cref="TransactionTemporaryData"/> instance.
    /// </summary>
    public static TransactionTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return TransactionTemporaryDataScope.CurrentContext; }
    }

    #region IContext<...> methods

    /// <inheritdoc/>
    public override bool IsActive
    {
      [DebuggerStepThrough]
      get { return Current==this; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TransactionTemporaryDataScope CreateActiveScope()
    {
      return new TransactionTemporaryDataScope(this);
    }

    #endregion
  }
}