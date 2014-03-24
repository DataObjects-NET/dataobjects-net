// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using JetBrains.Annotations;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage node manager.
  /// </summary>
  public sealed class StorageNodeManager
  {
    private readonly HandlerAccessor handlers;

    /// <summary>
    /// Adds node with the specified <paramref name="configuration"/>
    /// and performs required upgrade actions.
    /// </summary>
    /// <param name="configuration">Node configuration.</param>
    public bool AddNode([NotNull] NodeConfiguration configuration)
    {
      var node = UpgradingDomainBuilder.BuildNode(handlers.Domain, configuration);
      return handlers.StorageNodeRegistry.Add(node);
    }

    /// <summary>
    /// Removes node with specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns>True if node was removed, otherwise false.</returns>
    public bool RemoveNode([NotNull] string nodeId)
    {
      return handlers.StorageNodeRegistry.Remove(nodeId);
    }

    /// <summary>
    /// Gets node with the specified <paramref name="nodeId"/>
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns><see cref="StorageNode"/> with the specified <paramref name="nodeId"/> if found,
    /// otherwise null.</returns>
    [CanBeNull]
    public StorageNode GetNode([NotNull] string nodeId)
    {
      return handlers.StorageNodeRegistry.TryGet(nodeId);
    }


    // Constructors

    internal StorageNodeManager(HandlerAccessor handlers)
    {
      this.handlers = handlers;
    }
  }
}