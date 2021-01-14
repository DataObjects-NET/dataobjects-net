// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.11.18
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchQueryEnumerable<TItem> : IEnumerable<TItem>
  {
    private readonly Session session;
    private readonly IEnumerable<TItem> source;
    private readonly SinglyLinkedList<KeyExtractorNode<TItem>> nodes;
    private readonly Queue<Key> unknownTypeQueue;
    private readonly Queue<Pair<IEnumerable<Key>, IHasNestedNodes>> prefetchQueue;
    private readonly Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>> fieldDescriptorCache;
    private readonly SessionHandler sessionHandler;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator()
    {
      var currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      var aggregatedNodes = NodeAggregator<TItem>.Aggregate(nodes);
      var resultQueue = new Queue<TItem>();
      var container = new StrongReferenceContainer(null);
      foreach (var item in source) {
        resultQueue.Enqueue(item);
        if (item != null) {
          foreach (var extractorNode in aggregatedNodes) {
            container.JoinIfPossible(RegisterPrefetch(extractorNode.ExtractKeys(item), extractorNode));
          }
        }

        if (currentTaskCount == sessionHandler.PrefetchTaskExecutionCount) {
          continue;
        }

        while (container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks())) {
          container.JoinIfPossible(ProcessFetchedElements());
        }

        while (resultQueue.TryDequeue(out var resultItem)) {
          yield return resultItem;
        }

        currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      }

      while (container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks())) {
        container.JoinIfPossible(ProcessFetchedElements());
      }

      while (resultQueue.TryDequeue(out var resultItem)) {
        yield return resultItem;
      }
    }

    private StrongReferenceContainer RegisterPrefetch(IReadOnlyCollection<Key> keys, IHasNestedNodes fieldContainer)
    {
      var container = new StrongReferenceContainer(null);
      TypeInfo modelType = null;
      if (fieldContainer is ReferenceNode refNode) {
        modelType = refNode.ReferenceType;
      }

      foreach (var key in keys) {
        var type = key.HasExactType || modelType == null
          ? key.TypeReference.Type
          : modelType;
        if (!key.HasExactType && !type.IsLeaf) {
          unknownTypeQueue.Enqueue(key);
        }

        var cacheKey = new Pair<IHasNestedNodes, TypeInfo>(fieldContainer, type);
        if (!fieldDescriptorCache.TryGetValue(cacheKey, out var fieldDescriptors)) {
          fieldDescriptors = PrefetchHelper
            .GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type)
            .Concat(fieldContainer.NestedNodes.Select(fn => new PrefetchFieldDescriptor(fn.Field, false, true)))
            .ToList();
          fieldDescriptorCache.Add(cacheKey, fieldDescriptors);
        }

        container.JoinIfPossible(sessionHandler.Prefetch(key, type, fieldDescriptors));
      }

      var nestedContainers = fieldContainer.NestedNodes.OfType<IHasNestedNodes>();
      foreach (var nestedContainer in nestedContainers) {
        prefetchQueue.Enqueue(new Pair<IEnumerable<Key>, IHasNestedNodes>(keys, nestedContainer));
      }

      return container;
    }

    private StrongReferenceContainer ProcessFetchedElements()
    {
      var container = new StrongReferenceContainer(null);
      while (unknownTypeQueue.TryDequeue(out var unknownKey)) {
        var unknownType = session.EntityStateCache[unknownKey, false].Type;
        var unknownDescriptors =
          PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
        sessionHandler.Prefetch(unknownKey, unknownType, unknownDescriptors);
      }

      while (prefetchQueue.TryDequeue(out var pair)) {
        var parentKeys = pair.First;
        var nestedNodes = pair.Second;
        var keys = new List<Key>();
        foreach (var parentKey in parentKeys) {
          var entityState = session.EntityStateCache[parentKey, false];
          if (entityState == null) {
            container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks(true));
            entityState = session.EntityStateCache[parentKey, false];
            if (entityState == null) {
              throw new InvalidOperationException(string.Format(Strings.ExCannotResolveEntityWithKeyX, parentKey));
            }
          }

          keys.AddRange(nestedNodes.ExtractKeys(entityState.Entity));
        }

        container.JoinIfPossible(RegisterPrefetch(keys, nestedNodes));
      }

      return container;
    }

    public PrefetchQueryEnumerable(Session session, IEnumerable<TItem> source,
      SinglyLinkedList<KeyExtractorNode<TItem>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
      sessionHandler = session.Handler;
      unknownTypeQueue = new Queue<Key>();
      prefetchQueue = new Queue<Pair<IEnumerable<Key>, IHasNestedNodes>>();
      fieldDescriptorCache = new Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>>();
    }
  }
}