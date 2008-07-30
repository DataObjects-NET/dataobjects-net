// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class WellKnown
  {
    /// <summary>
    /// Returns ".ctor".
    /// </summary>
    public static readonly string CtorName = ".ctor";

    /// <summary>
    /// Returns "get_".
    /// </summary>
    public static readonly string GetterPrefix = "get_";
    /// <summary>
    /// Returns "set_".
    /// </summary>
    public static readonly string SetterPrefix = "set_";

    /// <summary>
    /// Returns "add_".
    /// </summary>
    public static readonly string AddEventHandlerPrefix = "add_";
    /// <summary>
    /// Returns "remove_".
    /// </summary>
    public static readonly string RemoveEventHandlerPrefix = "remove_";
  }
}