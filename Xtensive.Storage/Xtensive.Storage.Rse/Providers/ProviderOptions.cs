// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.15

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Enumerates possible <see cref="Provider.Options"/> of the <see cref="Provider"/>.
  /// </summary>
  [Flags]
  public enum ProviderOptions
  {
    /// <summary>
    /// Default options (none).
    /// </summary>
    Default = 0,
    /// <summary>
    /// The provider is indexed.
    /// </summary>
    Indexed = 1,
    /// <summary>
    /// The provider provides ordered sequence.
    /// </summary>
    Ordered = 2,
    /// <summary>
    /// Getting <see cref="Provider.Count"/> requires less then O(N) internal operations (e.g. O(log(N))).
    /// </summary>
    FastCount = 4,
    /// <summary>
    /// Provider
    /// </summary>
    RandomAccess = 8,
    /// <summary>
    /// 
    /// </summary>
    FastFirst = 16,
  }
}