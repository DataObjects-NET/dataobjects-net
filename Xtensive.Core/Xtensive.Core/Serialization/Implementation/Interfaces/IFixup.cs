// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.20

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// An action which should be executed after complete deserialization of the graph.
  /// </summary>
  public interface IFixup
  {
    /// <summary>
    /// Gets the object to execute the action on.
    /// </summary>
    object Source { get; }

    /// <summary>
    /// Gets the reference this fixup action is defined for.
    /// </summary>
    IReference Reference { get; }

    /// <summary>
    /// Executes fixup action.
    /// </summary>
    void Execute();
  }
}