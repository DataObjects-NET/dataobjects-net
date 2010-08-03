// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.03

using System;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage
{
  public partial class Session
  {
    private bool isOperationLoggingEnabled = true;

    /// <summary>
    /// Indicates whether operation logging is enabled.
    /// <see cref="IsSystemLogicOnly"/> and <see cref="SessionBound."/> implicitely turn this option off;
    /// <see cref="DisableOperationLogging"/> does this explicitly.
    /// </summary>
    public bool IsOperationLoggingEnabled {
      get { return isOperationLoggingEnabled && !IsSystemLogicOnly; }
    }

    internal bool IsOutermostOperationLoggingEnabled {
      get { return OutermostOperationCompleted!=null && IsOperationLoggingEnabled; }
    }

    internal bool IsNestedOperationLoggingEnabled {
      get { return NestedOperationCompleted!=null && IsOperationLoggingEnabled; }
    }

    internal IDisposable DisableOperationLogging()
    {
      var result = new Disposable<Session, bool>(this, isOperationLoggingEnabled,
        (disposing, session, previousState) => session.isOperationLoggingEnabled = previousState);
      isOperationLoggingEnabled = false;
      return result;
    }
  }
}