// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Global (<see cref="Domain"/>-level) temporary data context.
  /// </summary>
  public class GlobalTemporaryData : TemporaryDataBase<GlobalTemporaryDataScope>
  {
    /// <summary>
    /// Gets the current <see cref="GlobalTemporaryData"/> instance.
    /// </summary>
    public static GlobalTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return GlobalTemporaryDataScope.CurrentContext; }
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
    protected override GlobalTemporaryDataScope CreateActiveScope()
    {
      return new GlobalTemporaryDataScope(this);
    }

    #endregion
  }
}