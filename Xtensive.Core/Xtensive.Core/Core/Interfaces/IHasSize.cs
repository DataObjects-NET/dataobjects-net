// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.22

namespace Xtensive.Core
{
  /// <summary>
  /// Describes an object that able to calculate its own size.
  /// </summary>
  public interface IHasSize
  {
    /// <summary>
    /// Gets size of the instance in bytes.
    /// </summary>
    long Size { get; }
  }
}