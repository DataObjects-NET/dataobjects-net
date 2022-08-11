// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;


namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class PrefetchManager
  {
    #region Nested classes

    private readonly struct RootContainerCacheKey : IEquatable<RootContainerCacheKey>
    {
      private readonly int hashCode;

      private readonly TypeInfo type;
      private readonly IEnumerable<PrefetchFieldDescriptor> descriptors;
      
      public bool Equals(RootContainerCacheKey other)
      {
        return Equals(other.type, type) && Equals(other.descriptors, descriptors);
      }

      public override bool Equals(object obj)
      {
        if (obj is null) {
          return false;
        }
        if (obj.GetType() != typeof (RootContainerCacheKey)) {
          return false;
        }
        return Equals((RootContainerCacheKey) obj);
      }

      public override int GetHashCode() => hashCode;


      // Constructors

      public RootContainerCacheKey(TypeInfo type, IEnumerable<PrefetchFieldDescriptor> descriptors)
      {
        this.descriptors = descriptors;
        this.type = type;
        hashCode = unchecked ((type.GetHashCode() * 397) ^ descriptors.GetHashCode());
      }
    }

    private class RootContainerCacheEntry
    {
      public readonly RootContainerCacheKey Key;

      public readonly SortedDictionary<int, ColumnInfo> Columns;

      public readonly List<int> ColumnsToBeLoaded;


      // Constructors

      public RootContainerCacheEntry(in RootContainerCacheKey key, SortedDictionary<int, ColumnInfo> columns,
        List<int> columnsToBeLoaded)
      {
        Key = key;
        Columns = columns;
        ColumnsToBeLoaded = columnsToBeLoaded;
      }
    }

    #endregion

    private const int MaxContainerCount = 120;
    private const int ColumnIndexesCacheSize = 256;

    private readonly Dictionary<(Key key, TypeInfo type), GraphContainer> graphContainers = new Dictionary<(Key key, TypeInfo type), GraphContainer>();
    private readonly ICache<RootContainerCacheKey,RootContainerCacheEntry> columnsCache;
    private readonly Fetcher fetcher;
    private readonly Session session;

    private StrongReferenceContainer referenceContainer;

    public SessionHandler Owner => session.Handler;

    public int TaskExecutionCount { get; private set; }

    public StrongReferenceContainer Prefetch(Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors)
    {
      var prefetchTask = Prefetch(key, type, descriptors, false, default);
      return prefetchTask.GetAwaiter().GetResult();
    }

    public async Task<StrongReferenceContainer> PrefetchAsync(Key key, TypeInfo type,
      IReadOnlyList<PrefetchFieldDescriptor> descriptors, CancellationToken token = default)
    {
      var prefetchTask = Prefetch(key, type, descriptors, true, token);
      return await prefetchTask.ConfigureAwait(false);
    }

    private async ValueTask<StrongReferenceContainer> Prefetch(
      Key key, TypeInfo type, IReadOnlyList<PrefetchFieldDescriptor> descriptors, bool isAsync, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, nameof(key));
      ArgumentValidator.EnsureArgumentNotNull(descriptors, nameof(descriptors));

      if (descriptors.Count == 0) {
        return null;
      }
      try {
        EnsureKeyTypeCorrespondsToSpecifiedType(key, type);

        var currentKey = key;
        if (!TryGetTupleOfNonRemovedEntity(ref currentKey, out var ownerState)) {
          return null;
        }

        var selectedFields = descriptors;
        var currentType = type;
        var isKeyTypeExact = currentKey.HasExactType
          || currentKey.TypeReference.Type.IsLeaf
          || currentKey.TypeReference.Type == type;
        if (isKeyTypeExact) {
          currentType = currentKey.TypeReference.Type;
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
        }
        else {
          ArgumentValidator.EnsureArgumentNotNull(currentType, "type");
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
          _ = SetUpContainers(currentKey, currentKey.TypeReference.Type,
            PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, currentKey.TypeReference.Type),
            true, ownerState, true);
          var hierarchyRoot = currentKey.TypeReference.Type;
          selectedFields = descriptors
            .Where(descriptor => descriptor.Field.DeclaringType != hierarchyRoot)
            .ToList();
        }
        _ = SetUpContainers(currentKey, currentType, selectedFields, isKeyTypeExact, ownerState, ReferenceEquals(descriptors, selectedFields));

        StrongReferenceContainer container = null;
        if (graphContainers.Count >= MaxContainerCount) {
          container = await ExecuteTasks(false, isAsync, token).ConfigureAwait(false);
        }

        if (referenceContainer != null) {
          _ = referenceContainer.JoinIfPossible(container);
          return referenceContainer;
        }
        return container;
      }
      catch {
        CancelTasks();
        throw;
      }
    }

    public StrongReferenceContainer ExecuteTasks(bool skipPersist = false) =>
      ExecuteTasks(skipPersist, false, default).GetAwaiter().GetResult();

    public async Task<StrongReferenceContainer> ExecuteTasksAsync(bool skipPersist, CancellationToken token = default) =>
      await ExecuteTasks(skipPersist, true, token).ConfigureAwait(false);

    private async ValueTask<StrongReferenceContainer> ExecuteTasks(bool skipPersist, bool isAsync, CancellationToken token)
    {
      if (graphContainers.Count == 0) {
        referenceContainer = null;
        return null;
      }
      try {
        var batchExecuted =
          await fetcher.ExecuteTasks(graphContainers.Values, skipPersist, isAsync, token).ConfigureAwait(false);
        TaskExecutionCount += batchExecuted;
        foreach (var graphContainer in graphContainers.Values) {
          graphContainer.NotifyAboutExtractionOfKeysWithUnknownType();
        }
        return referenceContainer;
      }
      finally {
        CancelTasks();
      }
    }

    public void CancelTasks()
    {
      referenceContainer = null;
      graphContainers.Clear();
    }

    public bool TryGetTupleOfNonRemovedEntity(ref Key key, out EntityState state)
    {
      state = null;
      var entityState = GetCachedEntityState(ref key, out var isRemoved);
      if (isRemoved) {
        return false;
      }
      if (entityState != null) {
        SaveStrongReference(entityState);
        state = entityState;
        key = entityState.Key;
      }
      return true;
    }

    public GraphContainer SetUpContainers(Key key, TypeInfo type,
      IReadOnlyList<PrefetchFieldDescriptor> descriptors, bool exactType, EntityState state, bool canUseCache)
    {
      var result = GetGraphContainer(key, type, exactType);
      var areAnyColumns = false;
      var haveColumnsBeenSet = canUseCache
        ? TrySetCachedColumnIndexes(result, descriptors, state)
        : false;

      foreach (var descriptor in descriptors) {
        if (descriptor.Field.IsEntity && descriptor.FetchFieldsOfReferencedEntity && !type.IsAuxiliary) {
          areAnyColumns = true;
          result.RegisterReferencedEntityContainer(state, descriptor);
        }
        else if (descriptor.Field.IsEntitySet) {
          result.RegisterEntitySetTask(state, descriptor);
        }
        else {
          areAnyColumns = true;
        }
      }
      if (!haveColumnsBeenSet && areAnyColumns) {
        result.AddEntityColumns(ExtractColumns(descriptors));
      }
      return result;
    }

    public void SaveStrongReference(EntityState reference)
    {
      if (referenceContainer == null) {
        referenceContainer = new StrongReferenceContainer(null);
      }
      _ = referenceContainer.Join(new StrongReferenceContainer(reference));
    }

    public EntityState GetCachedEntityState(ref Key key, out bool isRemoved)
    {
      if (Owner.LookupState(key, out var cachedState)) {
        if (cachedState == null) {
          isRemoved = false;
          return null;
        }
        key = cachedState.Key;
        isRemoved = cachedState.PersistenceState == PersistenceState.Removed;
        return cachedState.IsTupleLoaded ? cachedState : null;
      }
      isRemoved = false;
      return null;
    }

    public void GetCachedColumnIndexes(TypeInfo type,
      IEnumerable<PrefetchFieldDescriptor> descriptors, out SortedDictionary<int, ColumnInfo> columns,
      out List<int> columnsToBeLoaded)
    {
      var cacheKey = new RootContainerCacheKey(type, descriptors);
      var cacheEntry = columnsCache[cacheKey, true];
      if (cacheEntry == null) {
        columns = PrefetchHelper.GetColumns(ExtractColumns(descriptors),type);
        columnsToBeLoaded = PrefetchHelper.GetColumnsToBeLoaded(columns, type);
        cacheEntry = new RootContainerCacheEntry(cacheKey, columns, columnsToBeLoaded);
        columnsCache.Add(cacheEntry);
        return;
      }
      columns = cacheEntry.Columns;
      columnsToBeLoaded = cacheEntry.ColumnsToBeLoaded;
    }

    #region Private / internal methods

    private static void EnsureKeyTypeCorrespondsToSpecifiedType(Key key, TypeInfo type)
    {
      if (type == null || key.TypeReference.Type == type) {
        return;
      }

      if (!key.TypeReference.Type.IsInterface && !type.IsInterface) {
        if (key.TypeReference.Type.Hierarchy == type.Hierarchy) {
          return;
        }
        else {
          throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
        }
      }

      if (type.GetInterfaces(true).Contains(key.TypeReference.Type)
        || key.TypeReference.Type.GetInterfaces(true).Contains(type)) {
        return;
      }

      throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
    }

    private static void EnsureAllFieldsBelongToSpecifiedType(IReadOnlyList<PrefetchFieldDescriptor> descriptors, TypeInfo type)
    {
      for (var i = 0; i < descriptors.Count; i++) {
        var declaringType = descriptors[i].Field.DeclaringType;
        if (type != declaringType && !declaringType.UnderlyingType.IsAssignableFrom(type.UnderlyingType)) {
          throw new InvalidOperationException(
            string.Format(Strings.ExFieldXIsNotDeclaredInTypeYOrInOneOfItsAncestors, descriptors[i].Field, type));
        }
      }
    }

    private GraphContainer GetGraphContainer(Key key, TypeInfo type, bool exactType) =>
      graphContainers.GetValueOrDefault((key, type))
        ?? (graphContainers[(key, type)] = new GraphContainer(key, type, exactType, this));

    private static IEnumerable<ColumnInfo> ExtractColumns(IEnumerable<PrefetchFieldDescriptor> descriptors)
    {
      foreach (var descriptor in descriptors) {
        var columns = descriptor.Field.IsStructure && !descriptor.FetchLazyFields
          ? descriptor.Field.Columns.Where(column => !column.Field.IsLazyLoad)
          : descriptor.Field.Columns;
        foreach (var column in columns) {
          yield return column;
        }
      }
    }

    private bool TrySetCachedColumnIndexes(
      GraphContainer container, IEnumerable<PrefetchFieldDescriptor> descriptors, EntityState state)
    {
      var result = false;
      if (container.RootEntityContainer == null) {
        GetCachedColumnIndexes(container.Type, descriptors, out var columns, out var columnsToBeLoaded);
        container.CreateRootEntityContainer(columns, state == null ? columnsToBeLoaded : null);
        result = true;
      }
      return result;
    }

    #endregion


    // Constructors

    public PrefetchManager(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      this.session = session;
      fetcher = new Fetcher(this);

      columnsCache = new LruCache<RootContainerCacheKey, RootContainerCacheEntry>(
        ColumnIndexesCacheSize, cacheEntry => cacheEntry.Key);
    }
  }
}
