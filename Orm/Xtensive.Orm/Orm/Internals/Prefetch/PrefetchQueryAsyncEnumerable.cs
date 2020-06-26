using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchQueryAsyncEnumerable<TItem> : IAsyncEnumerable<TItem>
  {
    private readonly Session session;
    private readonly IEnumerable<TItem> source;
    private readonly SinglyLinkedList<KeyExtractorNode<TItem>> nodes;
    private readonly Queue<Key> unknownTypeQueue;
    private readonly Queue<Pair<IEnumerable<Key>, IHasNestedNodes>> prefetchQueue;
    private readonly Dictionary<Pair<IHasNestedNodes, TypeInfo>, IList<PrefetchFieldDescriptor>> fieldDescriptorCache;
    private readonly SessionHandler sessionHandler;

    private StrongReferenceContainer strongReferenceContainer;

    internal StrongReferenceContainer StrongReferenceContainer => strongReferenceContainer;

    public async IAsyncEnumerator<TItem> GetAsyncEnumerator(CancellationToken token = default)
    {
      var currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      var aggregatedNodes = NodeAggregator<TItem>.Aggregate(nodes);
      var resultQueue = new Queue<TItem>();
      strongReferenceContainer = new StrongReferenceContainer(null);

      async IAsyncEnumerable<TItem> ProcessItem(TItem item1)
      {
        resultQueue.Enqueue(item1);
        if (item1 != null) {
          foreach (var extractorNode in aggregatedNodes) {
            var fetchedItems = await RegisterPrefetchAsync(extractorNode.ExtractKeys(item1), extractorNode, token);
            strongReferenceContainer.JoinIfPossible(fetchedItems);
          }
        }

        if (currentTaskCount == sessionHandler.PrefetchTaskExecutionCount) {
          yield break;
        }

        while (strongReferenceContainer.JoinIfPossible(await sessionHandler.ExecutePrefetchTasksAsync(token))) {
          strongReferenceContainer.JoinIfPossible(await ProcessFetchedElementsAsync(token));
        }

        while (resultQueue.TryDequeue(out var resultItem)) {
          yield return resultItem;
        }

        currentTaskCount = sessionHandler.PrefetchTaskExecutionCount;
      }

      if (source is IAsyncEnumerable<TItem> asyncItemSource) {
        await foreach (var item in asyncItemSource.WithCancellation(token)) {
          await foreach (var p in ProcessItem(item)) {
            yield return p;
          }
        }
      }
      else {
        var items = source is IQueryable<TItem> queryableSource
          ? await queryableSource.ExecuteAsync(token)
          : source;

        foreach (var item in items) {
          await foreach (var p in ProcessItem(item)) {
            yield return p;
          }
        }
      }

      while (strongReferenceContainer.JoinIfPossible(await sessionHandler.ExecutePrefetchTasksAsync(token))) {
        strongReferenceContainer.JoinIfPossible(await ProcessFetchedElementsAsync(token));
      }

      while (resultQueue.TryDequeue(out var resultItem)) {
        yield return resultItem;
      }
    }

    private async Task<StrongReferenceContainer> RegisterPrefetchAsync(
      IReadOnlyCollection<Key> keys, IHasNestedNodes fieldContainer, CancellationToken token)
    {
      var container = new StrongReferenceContainer(null);
      TypeInfo modelType = null;
      if (fieldContainer is ReferenceNode refNode) {
        modelType = refNode.ReferenceType;
      }

      foreach (var key in keys) {
        var type = key.HasExactType || modelType==null
          ? key.TypeReference.Type
          : modelType;
        if (!key.HasExactType && !type.IsLeaf) {
          unknownTypeQueue.Enqueue(key);
        }

        var cacheKey = new Pair<IHasNestedNodes, TypeInfo>(fieldContainer, type);
        if (!fieldDescriptorCache.TryGetValue(cacheKey, out var fieldDescriptors)) {
          fieldDescriptors = PrefetchHelper
            .GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, type)
            .Concat(fieldContainer.NestedNodes.Select(fn => new PrefetchFieldDescriptor(fn.Field, false, true))).ToList();
          fieldDescriptorCache.Add(cacheKey, fieldDescriptors);
        }
        container.JoinIfPossible(await sessionHandler.PrefetchAsync(key, type, fieldDescriptors, token));
      }
      var nestedContainers = fieldContainer.NestedNodes.OfType<IHasNestedNodes>();
      foreach (var nestedContainer in nestedContainers) {
        prefetchQueue.Enqueue(new Pair<IEnumerable<Key>, IHasNestedNodes>(keys, nestedContainer));
      }

      return container;
    }

    private async Task<StrongReferenceContainer> ProcessFetchedElementsAsync(CancellationToken token)
    {
      var container = new StrongReferenceContainer(null);
      while (unknownTypeQueue.TryDequeue(out var unknownKey)) {
        var unknownType = session.EntityStateCache[unknownKey, false].Type;
        var unknownDescriptors = PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, unknownType);
        await sessionHandler.PrefetchAsync(unknownKey, unknownType, unknownDescriptors, token);
      }
      while (prefetchQueue.TryDequeue(out var pair)) {
        var parentKeys = pair.First;
        var nestedNodes = pair.Second;
        var keys = new List<Key>();
        foreach (var parentKey in parentKeys) {
          var entityState = session.EntityStateCache[parentKey, false];
          if (entityState == null) {
            container.JoinIfPossible(await sessionHandler.ExecutePrefetchTasksAsync(true, token));
            entityState = session.EntityStateCache[parentKey, false];
            if (entityState == null) {
              throw new InvalidOperationException(string.Format(Strings.ExCannotResolveEntityWithKeyX, parentKey));
            }
          }
          keys.AddRange(nestedNodes.ExtractKeys(entityState.Entity));
        }
        container.JoinIfPossible(await RegisterPrefetchAsync(keys, nestedNodes, token));
      }
      return container;
    }

    public PrefetchQueryAsyncEnumerable(Session session, IEnumerable<TItem> source, SinglyLinkedList<KeyExtractorNode<TItem>> nodes)
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