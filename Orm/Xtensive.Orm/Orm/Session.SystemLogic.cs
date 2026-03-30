// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.03

using System;
using Xtensive.Core;

namespace Xtensive.Orm
{
  public partial class Session
  {
    public readonly struct SystemLogicOnlyRegionScope : IDisposable
    {
      private readonly Session session;
      private readonly bool prevIsSystemLogicOnly;

      internal SystemLogicOnlyRegionScope(Session session)
      {
        this.session = session;
        prevIsSystemLogicOnly = session.IsSystemLogicOnly;
        session.IsSystemLogicOnly = true;
      }

      public void Dispose() => session.IsSystemLogicOnly = prevIsSystemLogicOnly;
    }

    /// <summary>
    /// Gets a value indicating whether only a system logic is enabled.
    /// </summary>
    internal bool IsSystemLogicOnly { get; set; }

    internal SystemLogicOnlyRegionScope OpenSystemLogicOnlyRegion() => new(this);
  }
}