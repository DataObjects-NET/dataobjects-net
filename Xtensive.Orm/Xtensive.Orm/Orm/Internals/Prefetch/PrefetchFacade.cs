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
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchFacade<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<T> source;
    private readonly Collections.LinkedList<KeyExtractorNode<T>> nodes;

    private Queue<Key> unknownTypeQueue;
    private Queue<Pair<IEnumerable<Key>, IHasNestedNodes>> prefetchQueue;
    private Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>> fieldDescriptorCache;
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
      Initialize();
      var currentTaskCount = taskCount = session.Handler.PrefetchTaskExecutionCount;
      var aggregatedNodes = NodeAggregator<T>.Aggregate(nodes);
      var resultQueue = new Queue<T>();
      var container = new StrongReferenceContainer(null);
      foreach (var item in source) {
        resultQueue.Enqueue(item);
        if (item != null)
          foreach (var ken in aggregatedNodes)
            container.JoinIfPossible(RegisterPrefetch(ken.ExtractKeys(item), ken));
        if (currentTaskCount == session.Handler.PrefetchTaskExecutionCount) 
          continue;
        while (container.JoinIfPossible(session.Handler.ExecutePrefetchTasks()))
          container.JoinIfPossible(ProcessFetchedElements());
        while (resultQueue.Count > 0)
          yield return resultQueue.Dequeue();
        currentTaskCount = session.Handler.PrefetchTaskExecutionCount;
      }
      while (container.JoinIfPossible(session.Handler.ExecutePrefetchTasks()))
        container.JoinIfPossible(ProcessFetchedElements());
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
        var cacheKey = new Pair<IHasNestedNodes, TypeInfo>(fieldContainer, type);
        if (!fieldDescriptorCache.TryGetValue(cacheKey, out fieldDescriptors)) {
          fieldDescriptors = PrefetchHelper
            .GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type)
            .Concat(fieldContainer.NestedNodes.Select(fn => new PrefetchFieldDescriptor(fn.Field, false, true))).ToList();
          fieldDescriptorCache.Add(cacheKey, fieldDescriptors);
        }
        container.JoinIfPossible(session.Handler.Prefetch(key, type, fieldDescriptors));
        if (taskCount != session.Handler.PrefetchTaskExecutionCount)
          container.JoinIfPossible(ProcessFetchedElements());
      }
      var nestedContainers = fieldContainer.NestedNodes.OfType<IHasNestedNodes>();
      foreach (var nestedContainer in nestedContainers)
        prefetchQueue.Enqueue(new Pair<IEnumerable<Key>, IHasNestedNodes>(keys, nestedContainer));

      return container;
    }

    private StrongReferenceContainer ProcessFetchedElements()
    {
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
        var keys = new List<Key>();
        foreach (var parentKey in parentKeys) {
          var entityState = session.EntityStateCache[parentKey, false];
          if (entityState == null) {
            container.JoinIfPossible(session.Handler.ExecutePrefetchTasks(true));
            taskCount = session.Handler.PrefetchTaskExecutionCount;
            entityState = session.EntityStateCache[parentKey, false];
            if (entityState == null)
              throw new InvalidOperationException(string.Format(Strings.ExCannotResolveEntityWithKeyX, parentKey));
          }
          keys.AddRange(fieldContainer.ExtractKeys(entityState.Entity));
        }
        container.JoinIfPossible(RegisterPrefetch(keys, fieldContainer));
      }
      return container;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

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
      nodes = Collections.LinkedList<KeyExtractorNode<T>>.Empty;
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, Collections.LinkedList<KeyExtractorNode<T>>.Empty)
    {}

    private PrefetchFacade(Session session, IEnumerable<T> source, Collections.LinkedList<KeyExtractorNode<T>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}