// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class PrefetchManager
  {
    #region Nested classes

    private struct RootContainerCacheKey : IEquatable<RootContainerCacheKey>
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
        if (ReferenceEquals(null, obj))
          return false;
        if (obj.GetType()!=typeof (RootContainerCacheKey))
          return false;
        return Equals((RootContainerCacheKey) obj);
      }

      public override int GetHashCode()
      {
        return hashCode;
      }


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

      public RootContainerCacheEntry(RootContainerCacheKey key, SortedDictionary<int, ColumnInfo> columns,
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

    private readonly SetSlim<GraphContainer> graphContainers = new SetSlim<GraphContainer>();
    private readonly LruCache<RootContainerCacheKey,RootContainerCacheEntry> columnsCache;
    private StrongReferenceContainer referenceContainer;
    private readonly Fetcher fetcher;
    private readonly Session session;

    public SessionHandler Owner
    {
      get { return session.Handler; }
    }

    public int TaskExecutionCount { get; private set; }

    public StrongReferenceContainer Prefetch(Key key, TypeInfo type, IList<PrefetchFieldDescriptor> descriptors)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(descriptors, "fields");
      if (descriptors.Count == 0)
        return null;

      try {
        EnsureKeyTypeCorrespondsToSpecifiedType(key, type);

        EntityState ownerState;
        var currentKey = key;
        if (!TryGetTupleOfNonRemovedEntity(ref currentKey, out ownerState))
          return null;
        IEnumerable<PrefetchFieldDescriptor> selectedFields = descriptors;
        var currentType = type;
        StrongReferenceContainer hierarchyRootContainer = null;
        var isKeyTypeExact = currentKey.HasExactType || currentKey.TypeReference.Type.IsLeaf
          || currentKey.TypeReference.Type==type;
        if (isKeyTypeExact) {
          currentType = currentKey.TypeReference.Type;
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
        }
        else {
          ArgumentValidator.EnsureArgumentNotNull(currentType, "type");
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
          SetUpContainers(currentKey, currentKey.TypeReference.Type,
            PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, currentKey.TypeReference.Type),
            true, ownerState, true);
          var hierarchyRoot = currentKey.TypeReference.Type;
          selectedFields = descriptors.Where(descriptor => descriptor.Field.DeclaringType!=hierarchyRoot);
        }
        SetUpContainers(currentKey, currentType, selectedFields, isKeyTypeExact, ownerState, ReferenceEquals(descriptors, selectedFields));

        StrongReferenceContainer container = null;
        if (graphContainers.Count >= MaxContainerCount)
          container = ExecuteTasks();
        if (referenceContainer!=null) {
          referenceContainer.JoinIfPossible(container);
          return referenceContainer;
        }
        return container;
      }
      catch {
        CancelTasks();
        throw;
      }
    }

    public StrongReferenceContainer ExecuteTasks()
    {
      return ExecuteTasks(false);
    }

    public StrongReferenceContainer ExecuteTasks(bool skipPersist)
    {
      if (graphContainers.Count == 0) {
        referenceContainer = null;
        return null;
      }
      try {
        var batchExecuted = fetcher.ExecuteTasks(graphContainers, skipPersist);
        TaskExecutionCount += batchExecuted;
        foreach (var graphContainer in graphContainers)
          graphContainer.NotifyAboutExtractionOfKeysWithUnknownType();
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
      bool isRemoved;
      var entityState = GetCachedEntityState(ref key, out isRemoved);
      if (isRemoved)
        return false;
      if (entityState != null) {
        SaveStrongReference(entityState);
        state = entityState;
        key = entityState.Key;
      }
      return true;
    }

    public GraphContainer SetUpContainers(Key key, TypeInfo type,
      IEnumerable<PrefetchFieldDescriptor> descriptors, bool exactType, EntityState state, bool canUseCache)
    {
      var result = GetGraphContainer(key, type, exactType);
      var areAnyColumns = false;
      var haveColumnsBeenSet = false;
      if (canUseCache)
        haveColumnsBeenSet = TrySetCachedColumnIndexes(result, descriptors, state);
      foreach (var descriptor in descriptors) {
        if (descriptor.Field.IsEntity && descriptor.FetchFieldsOfReferencedEntity && !type.IsAuxiliary) {
          areAnyColumns = true;
          result.RegisterReferencedEntityContainer(state, descriptor);
        }
        else if (descriptor.Field.IsEntitySet)
          result.RegisterEntitySetTask(state, descriptor);
        else
          areAnyColumns = true;
      }
      if (!haveColumnsBeenSet && areAnyColumns)
        result.AddEntityColumns(ExtractColumns(descriptors));
      return result;
    }

    public void SaveStrongReference(EntityState reference)
    {
      if (referenceContainer == null)
        referenceContainer = new StrongReferenceContainer(null);
      referenceContainer.Join(new StrongReferenceContainer(reference));
    }

    public EntityState GetCachedEntityState(ref Key key, out bool isRemoved)
    {
      EntityState cachedState;
      if (Owner.TryGetEntityState(key, out cachedState)) {
        if (cachedState == null) {
          isRemoved = false;
          return null;
        }
        key = cachedState.Key;
        isRemoved = cachedState.PersistenceState==PersistenceState.Removed;
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

    #region Private \ internal methods

    private static void EnsureKeyTypeCorrespondsToSpecifiedType(Key key, TypeInfo type)
    {
      if (type == null || key.TypeReference.Type == type)
        return;
      if (!key.TypeReference.Type.IsInterface && !type.IsInterface)
        if (key.TypeReference.Type.Hierarchy == type.Hierarchy)
          return;
        else
          throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
      if (type.GetInterfaces(true).Contains(key.TypeReference.Type)
        || key.TypeReference.Type.GetInterfaces(true).Contains(type))
        return;
      throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
    }

    private static void EnsureAllFieldsBelongToSpecifiedType(IList<PrefetchFieldDescriptor> descriptors, TypeInfo type)
    {
      for (int i = 0; i < descriptors.Count; i++) {
        var declaringType = descriptors[i].Field.DeclaringType;
        if (type!=declaringType && !declaringType.UnderlyingType.IsAssignableFrom(type.UnderlyingType))
          throw new InvalidOperationException(
            String.Format(Strings.ExFieldXIsNotDeclaredInTypeYOrInOneOfItsAncestors, descriptors[i].Field, type));
      }
    }

    private GraphContainer GetGraphContainer(Key key, TypeInfo type, bool exactType)
    {
      var newTaskContainer = new GraphContainer(key, type, exactType, this);
      var registeredTaskContainer = graphContainers[newTaskContainer];
      if (registeredTaskContainer == null) {
        graphContainers.Add(newTaskContainer);
        return newTaskContainer;
      }
      return registeredTaskContainer;
    }

    private static IEnumerable<ColumnInfo> ExtractColumns(IEnumerable<PrefetchFieldDescriptor> descriptors)
    {
      foreach (var descriptor in descriptors) {
        IEnumerable<ColumnInfo> columns;
        if (descriptor.Field.IsStructure && !descriptor.FetchLazyFields)
          columns = descriptor.Field.Columns.Where(column => !column.Field.IsLazyLoad);
        else
          columns = descriptor.Field.Columns;
        foreach (var column in columns)
          yield return column;
      }
    }

    private bool TrySetCachedColumnIndexes(GraphContainer container,
      IEnumerable<PrefetchFieldDescriptor> descriptors, EntityState state)
    {
      var result = false;
      if (container.RootEntityContainer == null) {
        SortedDictionary<int, ColumnInfo> columns;
        List<int> columnsToBeLoaded;
        GetCachedColumnIndexes(container.Type, descriptors, out columns, out columnsToBeLoaded);
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
      columnsCache = new LruCache<RootContainerCacheKey, RootContainerCacheEntry>(ColumnIndexesCacheSize,
        cacheEntry => cacheEntry.Key);
    }
  }
}