// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.11

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class PrefetchKeyIterator<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<Key> source;

    public IEnumerator<T> GetEnumerator()
    {
      TypeInfo modelType;
      session.Domain.Model.Types.TryGetValue(typeof (T), out modelType);
      var taskCount = session.Handler.PrefetchTaskExecutionCount;
      var container = new StrongReferenceContainer(null);
      var fieldDescriptors = new List<PrefetchFieldDescriptor>();
      var resultQueue = new Queue<Key>();
      var unknownTypeQueue = new Queue<Key>();
      var se = source.GetEnumerator();
      bool exists;
      do {
        exists = se.MoveNext();
        if (exists) {
          var key = se.Current;
          var type = key.HasExactType || modelType==null
            ? key.TypeReference.Type
            : modelType;
          if (!key.HasExactType && !type.IsLeaf)
            unknownTypeQueue.Enqueue(key);
          resultQueue.Enqueue(key);
          var defaultDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type);
          container.JoinIfPossible(session.Handler.Prefetch(key, type, defaultDescriptors));
        }
        if (exists && taskCount==session.Handler.PrefetchTaskExecutionCount)
          continue;

        if (!exists)
          container.JoinIfPossible(session.Handler.ExecutePrefetchTasks());
        if (unknownTypeQueue.Count > 0) {
          while (unknownTypeQueue.Count > 0) {
            var unknownKey = unknownTypeQueue.Dequeue();
            var unknownType = session.EntityStateCache[unknownKey, false].Type;
            var unknownDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
            session.Handler.Prefetch(unknownKey, unknownType, unknownDescriptors);
          }
          session.Handler.ExecutePrefetchTasks();
        }
        while (resultQueue.Count > 0)
          yield return (T) (IEntity) session.EntityStateCache[resultQueue.Dequeue(), true].Entity;
        taskCount = session.Handler.PrefetchTaskExecutionCount;
      } while (exists);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PrefetchKeyIterator(Session session, IEnumerable<Key> source)
    {
      this.session = session;
      this.source = source;
    }
  }
}