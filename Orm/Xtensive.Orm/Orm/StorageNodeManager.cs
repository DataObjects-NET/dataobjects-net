// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// Asynchronously adds node with the specified <paramref name="configuration"/>
    /// and performs required upgrade actions.
    /// </summary>
    /// <param name="configuration">Node configuration.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    public async Task<bool> AddNodeAsync([NotNull] NodeConfiguration configuration, CancellationToken token = default)
    {
      var node = await UpgradingDomainBuilder.BuildNodeAsync(handlers.Domain, configuration, token)
        .ConfigureAwait(false);
      return handlers.StorageNodeRegistry.Add(node);
    }

    /// <summary>
    /// Removes node with specified <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    /// <returns>True if node was removed, otherwise false.</returns>
    public bool RemoveNode([NotNull] string nodeId)
    {
      var queryCache = handlers.Domain.QueryCache;
      foreach (var key in queryCache.Keys.Where(k => k is (object _, string keyNodeId) && keyNodeId == nodeId).ToList()) {
        queryCache.RemoveKey(key);
      }
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
