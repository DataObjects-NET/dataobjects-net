// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2011.01.11

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class PrefetchKeyIterator<T> : IEnumerable<T>, IAsyncEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<Key> source;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<T> GetEnumerator()
    {
      session.Domain.Model.Types.TryGetValue(typeof(T), out var modelType);
      var taskCount = session.Handler.PrefetchTaskExecutionCount;
      var container = new StrongReferenceContainer(null);
      var resultQueue = new Queue<Key>();
      var unknownTypeQueue = new Queue<Key>();
      using var se = source.GetEnumerator();
      bool exists;
      do {
        exists = se.MoveNext();
        if (exists) {
          var key = se.Current;
          var type = key.HasExactType || modelType == null
            ? key.TypeReference.Type
            : modelType;
          if (!key.HasExactType && !type.IsLeaf) {
            unknownTypeQueue.Enqueue(key);
          }

          resultQueue.Enqueue(key);
          var defaultDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type);
          container.JoinIfPossible(session.Handler.Prefetch(key, type, defaultDescriptors));
        }

        if (exists && taskCount == session.Handler.PrefetchTaskExecutionCount) {
          continue;
        }

        if (!exists) {
          container.JoinIfPossible(session.Handler.ExecutePrefetchTasks());
        }

        if (unknownTypeQueue.Count > 0) {
          while (unknownTypeQueue.TryDequeue(out var unknownKey)) {
            var unknownType = session.EntityStateCache[unknownKey, false].Type;
            var unknownDescriptors =
              PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
            session.Handler.Prefetch(unknownKey, unknownType, unknownDescriptors);
          }

          session.Handler.ExecutePrefetchTasks();
        }

        while (resultQueue.TryDequeue(out var item)) {
          yield return (T) (IEntity) session.EntityStateCache[item, true].Entity;
        }

        taskCount = session.Handler.PrefetchTaskExecutionCount;
      } while (exists);
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default)
    {
      session.Domain.Model.Types.TryGetValue(typeof(T), out var modelType);
      var taskCount = session.Handler.PrefetchTaskExecutionCount;
      var container = new StrongReferenceContainer(null);
      var resultQueue = new Queue<Key>();
      var unknownTypeQueue = new Queue<Key>();
      using var se = source.GetEnumerator();
      bool exists;
      do {
        exists = se.MoveNext();
        if (exists) {
          var key = se.Current;
          var type = key.HasExactType || modelType == null
            ? key.TypeReference.Type
            : modelType;
          if (!key.HasExactType && !type.IsLeaf) {
            unknownTypeQueue.Enqueue(key);
          }

          resultQueue.Enqueue(key);
          var defaultDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type);
          container.JoinIfPossible(
            await session.Handler.PrefetchAsync(key, type, defaultDescriptors, token).ConfigureAwaitFalse());
        }

        if (exists && taskCount == session.Handler.PrefetchTaskExecutionCount) {
          continue;
        }

        if (!exists) {
          container.JoinIfPossible(
            await session.Handler.ExecutePrefetchTasksAsync(token).ConfigureAwaitFalse());
        }

        if (unknownTypeQueue.Count > 0) {
          while (unknownTypeQueue.TryDequeue(out var unknownKey)) {
            var unknownType = session.EntityStateCache[unknownKey, false].Type;
            var unknownDescriptors =
              PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
            await session.Handler.PrefetchAsync(
              unknownKey, unknownType, unknownDescriptors, token).ConfigureAwaitFalse();
          }

          await session.Handler.ExecutePrefetchTasksAsync(token).ConfigureAwaitFalse();
        }

        while (resultQueue.TryDequeue(out var item)) {
          yield return (T) (IEntity) session.EntityStateCache[item, true].Entity;
        }

        taskCount = session.Handler.PrefetchTaskExecutionCount;
      } while (exists);
    }

    public PrefetchKeyIterator(Session session, IEnumerable<Key> source)
    {
      this.session = session;
      this.source = source;
    }
  }
}