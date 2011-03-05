// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.24

namespace Xtensive.Core
{
  /// <summary>
  /// This interface should be implemented by classes, that can
  /// calculate <see cref="long"/> hashes.
  /// </summary>
  public interface IHashable
  {
    /// <summary>
    /// Gets hash.
    /// </summary>
    long Hash { get; }
  }
}