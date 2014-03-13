// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System.Collections.Concurrent;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm
{
  /// <summary>
  /// Manager of storage nodes.
  /// </summary>
  public sealed class NodeManager
  {
    private readonly ConcurrentDictionary<string, NodeConfiguration> configurations
      = new ConcurrentDictionary<string, NodeConfiguration>();

    /// <summary>
    /// Adds node with the specified <paramref name="configuration"/>
    /// and performs required upgrade actions.
    /// </summary>
    /// <param name="configuration">Node configuration.</param>
    public void AddNode([NotNull] NodeConfiguration configuration)
    {
    }

    /// <summary>
    /// Removes node with specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns>True if node was removed, otherwise false.</returns>
    public bool RemoveNode([NotNull] string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      return false;
    }

    /// <summary>
    /// Gets <see cref="NodeConfiguration"/> for the node
    /// with the specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns><see cref="NodeConfiguration"/> for the specified <paramref name="nodeId"/>,
    /// or null if not found.</returns>
    [CanBeNull]
    public NodeConfiguration GetConfiguration([NotNull] string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      NodeConfiguration result;
      if (configurations.TryGetValue(nodeId, out result))
        return result;
      return null;
    }

    internal NodeManager()
    {
    }
  }
}