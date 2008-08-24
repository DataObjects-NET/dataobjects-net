// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.20

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Interface for class incapsulating action which should be execute
  /// after object deserialization.
  /// </summary>
  public interface IFixup
  {
    /// <summary>
    /// Execute fixup action.
    /// </summary>
    void Execute();
  }
}