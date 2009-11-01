// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.06.19

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Enumerates all possible <see cref="PluginManager{T}"/> states.
  /// </summary>
  public enum PluginManagerState
  {
    /// <summary>
    /// Initial state of <see cref="PluginManager{T}"/>. Plugin search has not been started.
    /// </summary>
    Initial = 0,

    /// <summary>
    /// <see cref="PluginManager{T}"/> currently is searching for plugins in a background thread.
    /// </summary>
    Searching = 1,

    /// <summary>
    /// Search for plugins is completed.
    /// </summary>
    Ready = 2,
  }
}