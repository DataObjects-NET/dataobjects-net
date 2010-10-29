// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.13

namespace Xtensive.Threading
{
  /// <summary>
  /// Describes an object that supports synchronized access to it.
  /// </summary>
  public interface ISynchronizable : IHasSyncRoot
  {
    /// <summary>
    /// Indicates whether object supports synchronized access to it, or not.
    /// </summary>
    bool IsSynchronized { get; }
  }
}