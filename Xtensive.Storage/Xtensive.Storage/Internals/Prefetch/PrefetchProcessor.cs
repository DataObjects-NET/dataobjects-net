// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals.Prefetch
{
  internal sealed class PrefetchProcessor
  {
    private const int MaxContainerCount = 100;

    private readonly SetSlim<GraphContainer> graphContainers = new SetSlim<GraphContainer>();
    private StrongReferenceContainer referenceContainer;
    private readonly Fetcher fetcher;
    private readonly Session session;

    public SessionHandler Owner
    {
      get { return session.Handler; }
    }

    public int TaskExecutionCount { get; private set; }

    public StrongReferenceContainer Prefetch(Key key, TypeInfo type,
      params PrefetchFieldDescriptor[] descriptors)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(descriptors, "fields");
      if (descriptors.Length == 0)
        return null;

      try {
        StrongReferenceContainer prevContainer = null;
        if (graphContainers.Count >= MaxContainerCount)
          prevContainer = ExecuteTasks();

        EnsureKeyTypeCorrespondsToSpecifiedType(key, type);

        Tuple ownerEntityTuple;
        var currentKey = key;
        if (!TryGetTupleOfNonRemovedEntity(ref currentKey, out ownerEntityTuple))
          return null;
        IEnumerable<PrefetchFieldDescriptor> selectedFields = descriptors;
        var currentType = type;
        StrongReferenceContainer hierarchyRootContainer = null;
        var isKeyTypeExact = currentKey.HasExactType || currentKey.TypeRef.Type.IsLeaf
          || currentKey.TypeRef.Type==type;
        if (isKeyTypeExact) {
          currentType = currentKey.TypeRef.Type;
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
        }
        else {
          ArgumentValidator.EnsureArgumentNotNull(currentType, "type");
          EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
          SetUpContainers(currentKey, currentKey.TypeRef.Type,
            PrefetchHelper.CreateDescriptorsForFieldsLoadedByDefault(currentKey.TypeRef.Type),
            true, ownerEntityTuple);
          var hierarchyRoot = currentKey.TypeRef.Type;
          selectedFields = descriptors.Where(descriptor => descriptor.Field.DeclaringType!=hierarchyRoot);
        }
        SetUpContainers(currentKey, currentType, selectedFields, isKeyTypeExact, ownerEntityTuple);
        if (referenceContainer!=null) {
          referenceContainer.JoinIfPossible(prevContainer);
          return referenceContainer;
        }
        return prevContainer;
      }
      catch {
        graphContainers.Clear();
        referenceContainer = null;
        throw;
      }
    }

    public StrongReferenceContainer ExecuteTasks()
    {
      if (graphContainers.Count == 0) {
        referenceContainer = null;
        return null;
      }
      try {
        fetcher.ExecuteTasks(graphContainers);
        foreach (var graphContainer in graphContainers)
          graphContainer.NotifyAboutExtractionOfKeysWithUnknownType();
        return referenceContainer;
      }
      finally {
        Clear();
        if (TaskExecutionCount < int.MaxValue)
          TaskExecutionCount++;
        else
          TaskExecutionCount = int.MinValue;
      }
    }

    public void Clear()
    {
      referenceContainer = null;
      graphContainers.Clear();
    }

    public bool TryGetTupleOfNonRemovedEntity(ref Key key, out Tuple tuple)
    {
      tuple = null;
      bool isRemoved;
      var entityState = GetCachedEntityState(key, out isRemoved);
      if (isRemoved)
        return false;
      if (entityState != null) {
        SaveStrongReference(entityState);
        tuple = entityState.Tuple;
        key = entityState.Key;
      }
      return true;
    }

    public GraphContainer SetUpContainers(Key key, TypeInfo type,
      IEnumerable<PrefetchFieldDescriptor> descriptors, bool exactType, Tuple entityTuple)
    {
      var result = GetGraphContainer(key, type, exactType);
      foreach (var descriptor in descriptors) {
        if (descriptor.Field.IsEntity && descriptor.FetchFieldsOfReferencedEntity && !type.IsAuxiliary)
          result.RegisterReferencedEntityContainer(entityTuple, descriptor);
        else if (descriptor.Field.IsEntitySet)
          result.RegisterEntitySetTask(entityTuple, descriptor);
        else
          result.AddEntityColumns(descriptor.Field.Columns);
      }
      return result;
    }

    public void SaveStrongReference(EntityState reference)
    {
      if (referenceContainer == null)
        referenceContainer = new StrongReferenceContainer(null);
      referenceContainer.Join(new StrongReferenceContainer(reference));
    }

    public EntityState GetCachedEntityState(Key key, out bool isRemoved)
    {
      EntityState cachedState;
      if (Owner.TryGetEntityState(key, out cachedState)) {
        isRemoved = cachedState.PersistenceState==PersistenceState.Removed;
        return cachedState.IsTupleLoaded ? cachedState : null;
      }
      isRemoved = false;
      return null;
    }

    #region Private \ internal methods

    private static void EnsureKeyTypeCorrespondsToSpecifiedType(Key key, TypeInfo type)
    {
      if (type == null || key.TypeRef.Type == type)
        return;
      if (!key.TypeRef.Type.IsInterface && !type.IsInterface)
        if (key.TypeRef.Type.Hierarchy == type.Hierarchy)
          return;
        else
          throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
      if (type.GetInterfaces(true).Contains(key.TypeRef.Type)
        || key.TypeRef.Type.GetInterfaces(true).Contains(type))
        return;
      throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
    }

    private static void EnsureAllFieldsBelongToSpecifiedType(PrefetchFieldDescriptor[] descriptors,
      TypeInfo type)
    {
      for (int i = 0; i < descriptors.Length; i++) {
        var declaringType = descriptors[i].Field.DeclaringType;
        if (type!=declaringType && !declaringType.UnderlyingType.IsAssignableFrom(type.UnderlyingType))
          throw new InvalidOperationException(
            String.Format(Strings.ExFieldXIsNotDeclaredInTypeYOrInOneOfItsAncestors,
              descriptors[i].Field, type));
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

    #endregion


    // Constructors

    public PrefetchProcessor(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      this.session = session;
      fetcher = new Fetcher(this);
    }
  }
}