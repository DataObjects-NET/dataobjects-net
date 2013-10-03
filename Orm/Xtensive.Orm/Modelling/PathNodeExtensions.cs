// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.05

using System;
using System.Diagnostics;

using Xtensive.Core;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Extension methods to <see cref="IPathNode"/> type.
  /// </summary>
  public static class PathNodeExtensions
  {
    /// <summary>
    /// Gets the child node by its path.
    /// </summary>
    /// <param name="path">Path to the node to get.</param>
    /// <param name="throwIfNone">Indicates whether an exception 
    /// must be thrown if there is no node with the specified path, or not.</param>
    /// <returns>
    /// Path node, if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public static IPathNode Resolve(this IPathNode node, string path, bool throwIfNone)
    {
      var result = node.Resolve(path);
      if (result==null && throwIfNone)
        throw new InvalidOperationException(string.Format(Strings.ExPathXNotFound, path));
      return result;
    }
  }
}