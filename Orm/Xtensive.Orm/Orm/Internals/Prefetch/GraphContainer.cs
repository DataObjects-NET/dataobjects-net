// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class GraphContainer
  {
    private Dictionary<FieldInfo, ReferencedEntityContainer> referencedEntityContainers;
    private Dictionary<FieldInfo, EntitySetTask> entitySetTasks;
    private readonly bool exactType;
    private int? cachedHashCode;
    private bool isReferenceContainerCreated;

    public readonly Key Key;
    public readonly TypeInfo Type;
    public readonly PrefetchManager Manager;

    public RootEntityContainer RootEntityContainer { get; private set; }

    public bool ContainsTask {
      get {
        return RootEntityContainer!=null
          || (referencedEntityContainers!=null && referencedEntityContainers.Count > 0)
          || (entitySetTasks!=null && entitySetTasks.Count > 0);
      }
    }

    public IEnumerable<ReferencedEntityContainer> ReferencedEntityContainers
    {
      get { return referencedEntityContainers!=null ? referencedEntityContainers.Values : null; }
    }

    public IEnumerable<EntitySetTask> EntitySetTasks
    {
      get { return entitySetTasks!=null ? entitySetTasks.Values : null; }
    }

    public void AddEntityColumns(IEnumerable<ColumnInfo> columns)
    {
      if (RootEntityContainer==null)
        RootEntityContainer = new RootEntityContainer(Key, Type, exactType, Manager);
      RootEntityContainer.AddColumns(columns);
    }

    public void CreateRootEntityContainer(
      SortedDictionary<int, ColumnInfo> forcedColumns, List<int> forcedColumnsToBeLoaded)
    {
      RootEntityContainer = new RootEntityContainer(Key, Type, exactType, Manager);
      RootEntityContainer.SetColumnCollections(forcedColumns, forcedColumnsToBeLoaded);
    }
    
    public void RegisterReferencedEntityContainer(
      EntityState ownerState, PrefetchFieldDescriptor referencingFieldDescriptor)
    {
      if (referencedEntityContainers != null
        && referencedEntityContainers.ContainsKey(referencingFieldDescriptor.Field))
        return;
      if (!AreAllForeignKeyColumnsLoaded(ownerState, referencingFieldDescriptor.Field))
        RegisterFetchByUnknownForeignKey(referencingFieldDescriptor);
      else
        RegisterFetchByKnownForeignKey(referencingFieldDescriptor, ownerState);
    }

    public void RegisterEntitySetTask(EntityState ownerState, PrefetchFieldDescriptor referencingFieldDescriptor)
    {
      if (entitySetTasks==null)
        entitySetTasks = new Dictionary<FieldInfo, EntitySetTask>();
      if (RootEntityContainer==null)
        AddEntityColumns(Key.TypeReference.Type.Fields
          .Where(field => field.IsPrimaryKey || field.IsSystem).SelectMany(field => field.Columns));
      EntitySetTask task;
      if (!entitySetTasks.TryGetValue(referencingFieldDescriptor.Field, out task))
        entitySetTasks.Add(referencingFieldDescriptor.Field,
          new EntitySetTask(Key, referencingFieldDescriptor, ownerState!=null, Manager));
      else if (task.ItemCountLimit==null)
        return;
      else if (referencingFieldDescriptor.EntitySetItemCountLimit==null
        || task.ItemCountLimit < referencingFieldDescriptor.EntitySetItemCountLimit)
        entitySetTasks[referencingFieldDescriptor.Field] =
          new EntitySetTask(Key, referencingFieldDescriptor, ownerState!=null, Manager);
    }

    public void NotifyAboutExtractionOfKeysWithUnknownType()
    {
      if (RootEntityContainer!=null)
        RootEntityContainer.NotifyOwnerAboutKeyWithUnknownType();
      if (referencedEntityContainers==null)
        return;
      foreach (var pair in referencedEntityContainers)
        pair.Value.NotifyOwnerAboutKeyWithUnknownType();
    }

    public bool Equals(GraphContainer other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (!Type.Equals(other.Type))
        return false;
      return Key.Equals(other.Key);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var otherType = obj.GetType();
      if (otherType != (typeof (GraphContainer)))
        return false;
      return Equals((GraphContainer) obj);
    }

    public override int GetHashCode()
    {
      if (cachedHashCode==null)
        unchecked {
          cachedHashCode = (Key.GetHashCode() * 397) ^ Type.GetHashCode();
        }
      return cachedHashCode.Value;
    }

    #region Private . internal methods

    private static bool AreAllForeignKeyColumnsLoaded(EntityState state, FieldInfo field)
    {
      if (state==null || !state.IsTupleLoaded)
        return false;
      var tuple = state.Tuple;
      var fieldStateMap = tuple.GetFieldStateMap(TupleFieldState.Available);
      for (var i = 0; i < field.MappingInfo.Length; i++)
        if (!fieldStateMap[field.MappingInfo.Offset + i])
          return false;
      return true;
    }

    private void RegisterFetchByKnownForeignKey(PrefetchFieldDescriptor referencingFieldDescriptor,
      EntityState ownerState)
    {
      var association = referencingFieldDescriptor.Field.Associations.Last();
      var referencedKeyTuple = association
        .ExtractForeignKey(ownerState.Type, ownerState.Tuple);
      var referencedKeyTupleState = referencedKeyTuple.GetFieldStateMap(TupleFieldState.Null);
      for (var i = 0; i < referencedKeyTupleState.Length; i++)
        if (referencedKeyTupleState[i])
          return;
      var session = Manager.Owner.Session;
      var referencedKey = Key.Create(session.Domain, session.NodeId,
        association.TargetType, TypeReferenceAccuracy.BaseType,
        referencedKeyTuple);
      var targetType = association.TargetType;
      var areToNotifyOwner = true;
      TypeInfo exactReferencedType;
      var hasExactTypeBeenGotten = PrefetchHelper.TryGetExactKeyType(referencedKey, Manager,
        out exactReferencedType);
      if (hasExactTypeBeenGotten!=null) {
        if (hasExactTypeBeenGotten.Value) {
          targetType = exactReferencedType;
          areToNotifyOwner = false;
        }
      }
      else
        return;
      var fieldsToBeLoaded = PrefetchHelper
        .GetCachedDescriptorsForFieldsLoadedByDefault(session.Domain, targetType);
      var graphContainer = Manager.SetUpContainers(referencedKey, targetType,
        fieldsToBeLoaded, true, null, true);
      if (areToNotifyOwner)
        graphContainer.RootEntityContainer.SetParametersOfReference(referencingFieldDescriptor, referencedKey);
    }

    private void RegisterFetchByUnknownForeignKey(PrefetchFieldDescriptor referencingFieldDescriptor)
    {
      if (referencedEntityContainers==null)
        referencedEntityContainers = new Dictionary<FieldInfo, ReferencedEntityContainer>();
      referencedEntityContainers.Add(referencingFieldDescriptor.Field, new ReferencedEntityContainer(Key,
        referencingFieldDescriptor, exactType, Manager));
    }

    #endregion


    // Constructors

    public GraphContainer(Key key, TypeInfo type, bool exactType, PrefetchManager manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(manager, "processor");

      Key = key;
      Type = type;

      Manager = manager;
      this.exactType = exactType;
    }
  }
}