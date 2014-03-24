// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Internals
{
  internal sealed class StorageNodeRegistry
  {
    private readonly ConcurrentDictionary<string, StorageNode> nodes = new ConcurrentDictionary<string, StorageNode>();

    public bool Add(StorageNode node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      return nodes.TryAdd(node.Id, node);
    }

    public bool Remove(string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      if (nodeId==WellKnown.DefaultNodeId)
        throw new InvalidOperationException(Strings.ExDefaultStorageNodeCanNotBeRemoved);
      StorageNode dummy;
      return nodes.TryRemove(nodeId, out dummy);
    }

    public StorageNode TryGet(string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      StorageNode result;
      nodes.TryGetValue(nodeId, out result);
      return result;
    }

    public StorageNode Get(string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      StorageNode result;
      if (!nodes.TryGetValue(nodeId, out result))
        throw new KeyNotFoundException(string.Format(Strings.ExStorageNodeWithIdXIsNotFound, nodeId));
      return result;
    }
  }
}