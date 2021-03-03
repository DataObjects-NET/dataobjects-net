// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.11.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchFacade<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly SessionHandler sessionHandler;
    private readonly IEnumerable<T> source;
    private readonly SinglyLinkedList<KeyExtractorNode<T>> nodes;

    private Queue<Key> unknownTypeQueue;
    private Queue<Pair<IEnumerable<Key>, IHasNestedNodes>> prefetchQueue;
    private Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>> fieldDescriptorCache;
    private int taskCount;

    public PrefetchFacade<T> RegisterPath<TValue>(Expression<Func<T, TValue>> expression)
    {
      var node = NodeBuilder.Build(session.Domain.Model, expression);
      return node == null
        ? this
        : new PrefetchFacade<T>(session, source, nodes.Add(node));
    }

    public IEnumerator<T> GetEnumerator()
    {
      Initialize();

      var currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      var aggregatedNodes = NodeAggregator<T>.Aggregate(nodes);
      var resultQueue = new Queue<T>();
      var container = new StrongReferenceContainer(null);
      var enumerationIdentifier = Guid.NewGuid();
      foreach (var item in source) {
        resultQueue.Enqueue(item);
        if (item != null) {
          foreach (var extractorNode in aggregatedNodes) {
            _ = container.JoinIfPossible(RegisterPrefetch(extractorNode.ExtractKeys(item), extractorNode, enumerationIdentifier));
          }
        }
        if (currentTaskCount==sessionHandler.PrefetchTaskExecutionCount) {
          continue;
        }
        while (container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks())) {
          _ = container.JoinIfPossible(ProcessFetchedElements(enumerationIdentifier));
        }
        while (resultQueue.Count > 0) {
          yield return resultQueue.Dequeue();
        }
        currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      }
      while (container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks())) {
        _ = container.JoinIfPossible(ProcessFetchedElements(enumerationIdentifier));
      }
      while (resultQueue.Count > 0) {
        yield return resultQueue.Dequeue();
      }
    }

    private StrongReferenceContainer RegisterPrefetch(IEnumerable<Key> keys, IHasNestedNodes fieldContainer, Guid enumerationId)
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
            .Concat(fieldContainer.NestedNodes.Select(fn => new PrefetchFieldDescriptor(fn.Field, false, true, enumerationId))).ToList();
          fieldDescriptorCache.Add(cacheKey, fieldDescriptors);
        }
        _ = container.JoinIfPossible(sessionHandler.Prefetch(key, type, fieldDescriptors));
      }
      var nestedContainers = fieldContainer.NestedNodes.OfType<IHasNestedNodes>();
      foreach (var nestedContainer in nestedContainers) {
        prefetchQueue.Enqueue(new Pair<IEnumerable<Key>, IHasNestedNodes>(keys, nestedContainer));
      }
      return container;
    }

    private StrongReferenceContainer ProcessFetchedElements(Guid enumerationId)
    {
      var container = new StrongReferenceContainer(null);
      //taskCount = sessionHandler.PrefetchTaskExecutionCount;
      while (unknownTypeQueue.Count > 0) {
        var unknownKey = unknownTypeQueue.Dequeue();
        var unknownType = session.EntityStateCache[unknownKey, false].Type;
        var unknownDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
        _ = sessionHandler.Prefetch(unknownKey, unknownType, unknownDescriptors);
      }
      while (prefetchQueue.Count > 0) {
        var pair = prefetchQueue.Dequeue();
        var parentKeys = pair.First;
        var nestedNodes = pair.Second;
        var keys = new List<Key>();
        foreach (var parentKey in parentKeys) {
          var entityState = session.EntityStateCache[parentKey, false];
          if (entityState == null) {
            _ = container.JoinIfPossible(sessionHandler.ExecutePrefetchTasks(true));
            //taskCount = sessionHandler.PrefetchTaskExecutionCount;
            entityState = session.EntityStateCache[parentKey, false];
            if (entityState == null) {
              throw new InvalidOperationException(string.Format(Strings.ExCannotResolveEntityWithKeyX, parentKey));
            }
          }
          keys.AddRange(nestedNodes.ExtractKeys(entityState.Entity));
        }
        _ = container.JoinIfPossible(RegisterPrefetch(keys, nestedNodes, enumerationId));
      }
      return container;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Initialize()
    {
      unknownTypeQueue = new Queue<Key>();
      prefetchQueue = new Queue<Pair<IEnumerable<Key>, IHasNestedNodes>>();
      fieldDescriptorCache = new Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>>();
    }


    // Constructors

    public PrefetchFacade(Session session, IEnumerable<Key> keySource)
    {
      this.session = session;
      source = new PrefetchKeyIterator<T>(session, keySource);
      nodes = SinglyLinkedList<KeyExtractorNode<T>>.Empty;
      sessionHandler = this.session.Handler;
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, SinglyLinkedList<KeyExtractorNode<T>>.Empty)
    {
    }

    private PrefetchFacade(Session session, IEnumerable<T> source, SinglyLinkedList<KeyExtractorNode<T>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
      sessionHandler = this.session.Handler;
    }
  }
}