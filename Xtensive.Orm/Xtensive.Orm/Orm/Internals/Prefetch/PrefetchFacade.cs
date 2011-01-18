// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchFacade<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<T> source;
    private readonly Collections.LinkedList<KeyExtractorNode<T>> nodes;
    private readonly Queue<Key> unknownTypeQueue;
    private readonly Queue<Pair<IEnumerable<Key>, IHasNestedNodes>> prefetchQueue;
    private readonly Dictionary<IHasNestedNodes, IList<PrefetchFieldDescriptor>> fieldDescriptorCache;
    private int taskCount;

    public PrefetchFacade<T> RegisterPath<TValue>(Expression<Func<T, TValue>> expression)
    {
      var node = NodeParser.Parse(session.Domain.Model, expression);
      if (node != null)
        return new PrefetchFacade<T>(session, source, nodes.AppendHead(node));
      return this;
    }

    public IEnumerator<T> GetEnumerator()
    {
      taskCount = session.Handler.PrefetchTaskExecutionCount;
      var aggregatedNodes = NodeAggregator<T>.Aggregate(nodes);
      var resultQueue = new Queue<T>();
      var container = new StrongReferenceContainer(null);
      foreach (var item in source) {
        resultQueue.Enqueue(item);
        foreach (var ken in aggregatedNodes)
          container.JoinIfPossible(RegisterPrefetch(ken.ExtractKeys(item), ken));
      }
      container.JoinIfPossible(session.Handler.ExecutePrefetchTasks());
      while (resultQueue.Count > 0)
        yield return resultQueue.Dequeue();
    }

    private StrongReferenceContainer RegisterPrefetch(IEnumerable<Key> keys, IHasNestedNodes fieldContainer)
    {
      var container = new StrongReferenceContainer(null);
      TypeInfo modelType = null;
      var refNode = fieldContainer as ReferenceNode;
      if (refNode != null)
        modelType = refNode.ElementType;
      foreach (var key in keys) {
        var type = key.HasExactType || modelType == null
            ? key.TypeReference.Type
            : modelType;
        if (!key.HasExactType && !type.IsLeaf)
          unknownTypeQueue.Enqueue(key);
        IList<PrefetchFieldDescriptor> fieldDescriptors;
        if (!fieldDescriptorCache.TryGetValue(fieldContainer, out fieldDescriptors)) {
          fieldDescriptors = PrefetchHelper
            .GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type)
            .Concat(fieldContainer.NestedNodes.Select(fn => new PrefetchFieldDescriptor(fn.Field, false, false))).ToList();
          fieldDescriptorCache.Add(fieldContainer, fieldDescriptors);
        }
        container.JoinIfPossible(session.Handler.Prefetch(key, type, fieldDescriptors));
        container.JoinIfPossible(ProcessFetchedElements());
      }
      var nestedContainers = fieldContainer.NestedNodes.OfType<IHasNestedNodes>();
      foreach (var nestedContainer in nestedContainers)
        prefetchQueue.Enqueue(new Pair<IEnumerable<Key>, IHasNestedNodes>(keys, nestedContainer));

      return container;
    }

    private StrongReferenceContainer ProcessFetchedElements()
    {
      if (taskCount != session.Handler.PrefetchTaskExecutionCount) {
        var container = new StrongReferenceContainer(null);
        taskCount = session.Handler.PrefetchTaskExecutionCount;
        while (unknownTypeQueue.Count > 0) {
          var unknownKey = unknownTypeQueue.Dequeue();
          var unknownType = session.EntityStateCache[unknownKey, false].Type;
          var unknownDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
          session.Handler.Prefetch(unknownKey, unknownType, unknownDescriptors);
        }
        while (prefetchQueue.Count > 0) {
          var pair = prefetchQueue.Dequeue();
          var parentKeys = pair.First;
          var fieldContainer = pair.Second;
          var keys = parentKeys
            .SelectMany(k => fieldContainer.ExtractKeys(session.EntityStateCache[k, false].Entity))
            .ToList();
          container.JoinIfPossible(RegisterPrefetch(keys, fieldContainer));
        }
      }
      return null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PrefetchFacade(Session session, IEnumerable<Key> keySource)
    {
      this.session = session;
      source = new PrefetchKeyIterator<T>(session, keySource);
      nodes = Collections.LinkedList<KeyExtractorNode<T>>.Empty;
      unknownTypeQueue = new Queue<Key>();
      prefetchQueue = new Queue<Pair<IEnumerable<Key>, IHasNestedNodes>>();
      fieldDescriptorCache = new Dictionary<IHasNestedNodes, IList<PrefetchFieldDescriptor>>();
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, Collections.LinkedList<KeyExtractorNode<T>>.Empty)
    {}

    private PrefetchFacade(Session session, IEnumerable<T> source, Collections.LinkedList<KeyExtractorNode<T>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
      unknownTypeQueue = new Queue<Key>();
      prefetchQueue = new Queue<Pair<IEnumerable<Key>, IHasNestedNodes>>();
      fieldDescriptorCache = new Dictionary<IHasNestedNodes, IList<PrefetchFieldDescriptor>>();
    }
  }
}