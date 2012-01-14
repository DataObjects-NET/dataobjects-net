// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.11

using System;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Enumerates transfer preference options.
  /// </summary>
  [Serializable]
  public enum TransferType
  {
    /// <summary>
    /// Default option is a <see cref="Server"/>.
    /// </summary>
    Default = Server,
    /// <summary>
    /// Prefer server execution.
    /// </summary>
    Server = 0,
    /// <summary>
    /// Prefer client execution.
    /// </summary>
    Client
  }
}