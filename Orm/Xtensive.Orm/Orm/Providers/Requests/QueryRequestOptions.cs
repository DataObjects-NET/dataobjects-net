// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Options for <see cref="QueryRequest"/>.
  /// </summary>
  [Flags]
  public enum QueryRequestOptions
  {
    /// <summary>
    /// Empty option set.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Optimization of this request is allowed.
    /// </summary>
    AllowOptimization = 0x1
  }
}