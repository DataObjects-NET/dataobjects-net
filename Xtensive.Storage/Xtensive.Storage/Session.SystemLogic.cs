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
    /// <summary>
    /// Gets a value indicating whether only a system logic is enabled.
    /// </summary>
    public bool IsSystemLogicOnly { get; internal set; }

    internal IDisposable OpenSystemLogicOnlyRegion()
    {
      var result = new Disposable<Session, bool>(this, IsSystemLogicOnly,
        (disposing, session, previousState) => session.IsSystemLogicOnly = previousState);
      IsSystemLogicOnly = true;
      return result;
    }
  }
}