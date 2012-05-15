// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.14

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Various options for <see cref="EnumerationContext"/>.
  /// </summary>
  [Flags]
  public enum EnumerationContextOptions
  {
    /// <summary>
    /// Empty option set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that <see cref="IEnumerator{T}"/> of the root provider
    /// should be fully read before returning data to user.
    /// </summary>
    GreedyEnumerator = 0x1,
  }
}