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

namespace Xtensive.Storage.Internals
{
  internal sealed class PrefetchProcessor
  {
    private const int MaxContainerCount = 10;

    private readonly SetSlim<PrefetchTaskContainer> taskContainers = new SetSlim<PrefetchTaskContainer>();
    
    public SessionHandler Owner { get; private set; }

    public int TaskExecutionCount { get; private set; }

    public void Prefetch(Key key, TypeInfo type, params PrefetchFieldDescriptor[] descriptors)
    {
      if (taskContainers.Count >= MaxContainerCount)
        ExecuteTasks();

      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(descriptors, "fields");
      if (type != null && type.Hierarchy != null && key.Hierarchy != type.Hierarchy)
        throw new ArgumentException(Strings.ExSpecifiedTypeHierarchyIsDifferentFromKeyHierarchy);
      Tuple ownerEntityTuple;
      var currentKey = key;
      if (!TryGetTupleOfNonRemovedEntity(ref currentKey, out ownerEntityTuple))
        return;
      IEnumerable<PrefetchFieldDescriptor> selectedFields = descriptors;
      var currentType = type;
      if (currentKey.IsTypeCached) {
        currentType = currentKey.Type;
        EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
      }
      else {
        ArgumentValidator.EnsureArgumentNotNull(currentType, "type");
        EnsureAllFieldsBelongToSpecifiedType(descriptors, currentType);
        PrefetchHierarchyRootColumns(currentKey);
        var hierarchyRoot = currentKey.Hierarchy.Root;
        selectedFields = descriptors.Where(descriptor => descriptor.Field.DeclaringType!=hierarchyRoot);
      }
      CreateTasks(currentKey, currentType, selectedFields, currentKey.IsTypeCached, ownerEntityTuple);
    }

    public void ExecuteTasks()
    {
      try {
        foreach (var taskContainer in taskContainers) {
          if (taskContainer.EntityPrefetchTask!=null)
            taskContainer.EntityPrefetchTask.RegisterQueryTask();
          var entitySetPrefetchTasks = taskContainer.EntitySetPrefetchTasks;
          if (entitySetPrefetchTasks!=null) {
            foreach (var task in entitySetPrefetchTasks)
              task.RegisterQueryTask();
          }
        }
        Owner.Session.ExecuteAllDelayedQueries(false);
        var reader = Owner.Session.Domain.RecordSetReader;
        foreach (var taskContainer in taskContainers) {
          if (taskContainer.EntityPrefetchTask!=null && taskContainer.EntityPrefetchTask.IsActive)
            UpdateCache(taskContainer.EntityPrefetchTask, reader, taskContainer.Key, taskContainer.Type);
          var entitySetPrefetchTasks = taskContainer.EntitySetPrefetchTasks;
          if (entitySetPrefetchTasks!=null)
            foreach (var task in entitySetPrefetchTasks)
              if (task.IsActive)
                UpdateCache(taskContainer.Key, task, reader);
        }

        foreach (var taskContainer in taskContainers) {
          var referencedEntityPrefetchTasks = taskContainer.ReferencedEntityPrefetchTasks;
          if (referencedEntityPrefetchTasks!=null) {
            foreach (var task in referencedEntityPrefetchTasks)
              task.RegisterQueryTask();
          }
        }
        Owner.Session.ExecuteAllDelayedQueries(false);
        foreach (var taskContainer in taskContainers) {
          var referencedEntityPrefetchTasks = taskContainer.ReferencedEntityPrefetchTasks;
          if (referencedEntityPrefetchTasks!=null) {
            foreach (var task in referencedEntityPrefetchTasks)
              if (task.IsActive)
                UpdateCache(task, reader, task.Key, task.ReferencingField.Association.TargetType);
          }
        }
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
      taskContainers.Clear();
    }

    public void ChangeOwner(SessionHandler newOwner)
    {
      Clear();
      Owner = newOwner;
    }
    
    public bool TryGetTupleOfNonRemovedEntity(ref Key key, out Tuple tuple)
    {
      tuple = null;
      bool isRemoved;
      var entityState = GetCachedEntityState(key, out isRemoved);
      if (isRemoved)
        return false;
      if (entityState != null) {
        tuple = entityState.Tuple;
        key = entityState.Key;
      }
      return true;
    }

    public void PrefetchHierarchyRootColumns(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      Tuple entityTuple;
      var currentKey = key;
      if (!TryGetTupleOfNonRemovedEntity(ref currentKey, out entityTuple))
        return;
      var descriptors = key.Hierarchy.Root.Fields.Where(PrefetchTask.IsFieldIntrinsicNonLazy)
        .Select(field => new PrefetchFieldDescriptor(field));
      CreateTasks(currentKey, currentKey.Hierarchy.Root, descriptors, true, entityTuple);
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

    private void CreateTasks(Key key, TypeInfo type, IEnumerable<PrefetchFieldDescriptor> descriptors,
      bool exactType, Tuple ownerEntityTuple)
    {
      var taskContainer = GetTaskContainer(key, type, exactType);
      foreach (var descriptor in descriptors) {
        if (descriptor.Field.IsEntity && descriptor.FetchFieldsOfReferencedEntity && !type.IsAuxiliary)
          taskContainer.RegisterReferencedEntityPrefetchTask(ownerEntityTuple, descriptor.Field);
        else if (descriptor.Field.IsEntitySet)
          taskContainer.RegisterEntitySetPrefetchTask(descriptor);
        else
          taskContainer.AddEntityColumns(descriptor.Field.Columns);
      }
    }

    private PrefetchTaskContainer GetTaskContainer(Key key, TypeInfo type, bool exactType)
    {
      var newTaskContainer = new PrefetchTaskContainer(key, type, exactType, this);
      var registeredTaskContainer = taskContainers[newTaskContainer];
      if (registeredTaskContainer == null) {
        taskContainers.Add(newTaskContainer);
        return newTaskContainer;
      }
      return registeredTaskContainer;
    }

    private EntityState GetCachedEntityState(Key key, out bool isRemoved)
    {
      EntityState cachedState;
      if (Owner.TryGetEntityState(key, out cachedState)) {
        isRemoved = cachedState.PersistenceState==PersistenceState.Removed;
        return cachedState.IsTupleLoaded ? cachedState : null;
      }
      isRemoved = false;
      return null;
    }
    
    private void UpdateCache(PrefetchTask task, RecordSetReader reader, Key key, TypeInfo type)
    {
      var record = reader.ReadSingleRow(task.Result, task.RecordSet.Header, key);
      if (record==null) {
        bool isRemoved;
        var cachedEntityState = GetCachedEntityState(key, out isRemoved);
        if (task.ExactType && !isRemoved && cachedEntityState != null && cachedEntityState.Type == type) {
          // Ensures there will be "removed" EntityState associated with this key
          Owner.RegisterEntityState(task.Key, null);
        }
        else
          return;
      }
      else {
        var fetchedKey = record.GetKey();
        var tuple = record.GetTuple();
        if (tuple!=null)
          Owner.RegisterEntityState(fetchedKey, tuple);
      }
    }

    private void UpdateCache(Key ownerKey, EntitySetPrefetchTask task, RecordSetReader reader)
    {
      var records = reader.Read(task.Result, task.RecordSet.Header);
      var entities = new List<Pair<Key, Tuple>>(task.Result.Count);
      List<Pair<Key, Tuple>> auxEntities = null;
      if (task.ReferencingField.Association.AuxiliaryType != null)
        auxEntities = new List<Pair<Key, Tuple>>(task.Result.Count);
      foreach (var record in records) {
        for (int i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key==null)
            continue;
          var tuple = record.GetTuple(i);
          if (tuple==null)
            continue;
          if (task.ReferencingField.Association.AuxiliaryType != null)
            if (i==0)
              auxEntities.Add(new Pair<Key, Tuple>(key, tuple));
            else
              entities.Add(new Pair<Key, Tuple>(key, tuple));
          else
            entities.Add(new Pair<Key, Tuple>(key, tuple));
        }
      }
      Owner.RegisterEntitySetState(ownerKey, task.ReferencingField, entities.Count < task.ItemCountLimit,
        entities, auxEntities);
    }


    // Constructors

    public PrefetchProcessor(SessionHandler sessionHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(sessionHandler, "sessionHandler");
      Owner = sessionHandler;
    }
  }
}