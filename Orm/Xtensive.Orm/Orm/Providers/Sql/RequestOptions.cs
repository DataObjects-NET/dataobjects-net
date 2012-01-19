// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Options for <see cref="Request"/>.
  /// </summary>
  [Flags]
  public enum RequestOptions
  {
    /// <summary>
    /// Empty option set.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Batching of this request is allowed.
    /// </summary>
    AllowBatching = 0x1
  }
}