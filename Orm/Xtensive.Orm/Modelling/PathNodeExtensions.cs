// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    /// <param name="node">An instance implementing <see cref="IPathNode"/> interface.</param>
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