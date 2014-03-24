// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage node manager.
  /// </summary>
  public sealed class NodeManager
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
    /// Gets <see cref="NodeConfiguration"/>
    /// for the node with the specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns><see cref="NodeConfiguration"/> for the specified <paramref name="nodeId"/>,
    /// or null if node is unknown.</returns>
    [CanBeNull]
    public NodeConfiguration GetConfiguration([NotNull] string nodeId)
    {
      var node = handlers.StorageNodeRegistry.TryGet(nodeId);
      return node!=null ? node.Configuration : null;
    }

    /// <summary>
    /// Gets <see cref="NodeConfiguration"/>
    /// for the specified <paramref name="session"/>.
    /// </summary>
    /// <param name="session">Session to use.</param>
    /// <returns><see cref="NodeConfiguration"/> for the specified <paramref name="session"/>.</returns>
    [NotNull]
    public NodeConfiguration GetConfiguration([NotNull] Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.StorageNode.Configuration;
    }

    /// <summary>
    /// Gets <see cref="ModelMapping"/>
    /// for the node with the specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns><see cref="ModelMapping"/> for the specified <paramref name="nodeId"/>,
    /// or null if node is unknown.</returns>
    [CanBeNull]
    public ModelMapping GetMapping([NotNull] string nodeId)
    {
      var node = handlers.StorageNodeRegistry.TryGet(nodeId);
      return node!=null ? node.Mapping : null;
    }

    /// <summary>
    /// Gets <see cref="ModelMapping"/>
    /// for the specified <paramref name="session"/>.
    /// </summary>
    /// <param name="session">Session to use.</param>
    /// <returns><see cref="ModelMapping"/> for the specified <paramref name="session"/>.</returns>
    [NotNull]
    public ModelMapping GetMapping([NotNull] Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.StorageNode.Mapping;
    }


    // Constructors

    internal NodeManager(HandlerAccessor handlers)
    {
      this.handlers = handlers;
    }
  }
}