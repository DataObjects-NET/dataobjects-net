// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal sealed class ReferencedEntityContainer : EntityContainer
  {
    private readonly MapTransform referencedEntityKeyTransform;
    private readonly Key ownerKey;
    private readonly bool isOwnerTypeKnown;
    private readonly PrefetchFieldDescriptor referencingFieldDescriptor;
    private bool needToNotifyOwner;

    public FieldInfo ReferencingField {get { return referencingFieldDescriptor.Field; } }

    public override EntityGroupTask GetTask()
    {
      if (Key!=null && Task==null)
        return null;

      if (Task!=null)
        return Task;

      EntityState ownerState;
      var isStateCached = Manager.Owner.TryGetEntityState(ownerKey, out ownerState);
      if (isStateCached) {
        if (ownerState == null)
          return null;
        if (ownerState.Tuple == null)
          return null;
        if (ownerState.PersistenceState == PersistenceState.Removed)
          return null;
      }
      if (!isStateCached)
        throw new KeyNotFoundException(
          String.Format(Strings.ExReferencingEntityWithKeyXIsNotFound, ownerKey));
      if (!ownerState.IsTupleLoaded)
        throw Exceptions.InternalError(Strings.ExReferencingEntityTupleIsNotLoaded, Log.Instance);
      if (!isOwnerTypeKnown && !ownerState.Key.TypeReference.Type.Fields.Contains(ReferencingField))
        return null;
      var foreignKeyTuple = ExtractForeignKeyTuple(ownerState);
      if (foreignKeyTuple == null)
        return null;
      Key = Key.Create(Manager.Owner.Session.Domain, Type, TypeReferenceAccuracy.BaseType, foreignKeyTuple);
      return CreateTask();
    }

    public void NotifyOwnerAboutKeyWithUnknownType()
    {
      if (needToNotifyOwner && Task != null)
        referencingFieldDescriptor.NotifySubscriber(ownerKey, Key);
    }

    private Tuple ExtractForeignKeyTuple(EntityState ownerState)
    {
      var association = ReferencingField.Associations.Last();
      var result = association.ExtractForeignKey(ownerState.Type, ownerState.Tuple);
      var tupleState = result.GetFieldStateMap(TupleFieldState.Null);
      for (int i = 0; i < result.Count; i++) {
        if (!result.GetFieldState(i).IsAvailable())
          if (isOwnerTypeKnown)
            throw Exceptions.InternalError(Strings.ExForeignKeyValueHaveNotBeenLoaded, Log.Instance);
          else
            return null;
        if (tupleState[i])
          return null;
      }
      return result;
    }

    private EntityGroupTask CreateTask()
    {
      TypeInfo exactReferencedType;
      var hasExactTypeBeenGotten = PrefetchHelper.TryGetExactKeyType(Key, Manager, out exactReferencedType);
      if (hasExactTypeBeenGotten!=null) {
        if (hasExactTypeBeenGotten.Value) {
          Type = exactReferencedType;
          //FillColumnCollection();
          needToNotifyOwner = false;
        }
        else
          needToNotifyOwner = true;
      }
      else
        return null;
      FillColumnCollection();
      if (!SelectColumnsToBeLoaded())
        return null;
      Task = new EntityGroupTask(Type, ColumnIndexesToBeLoaded.ToArray(), Manager);
      return Task;
    }

    private void FillColumnCollection()
    {
      var descriptors = PrefetchHelper
        .GetCachedDescriptorsForFieldsLoadedByDefault(Manager.Owner.Session.Domain, Type);
      SortedDictionary<int, ColumnInfo> columns;
      List<int> columnsToBeLoaded;
      Manager.GetCachedColumnIndexes(Type, descriptors, out columns, out columnsToBeLoaded);
      SetColumnCollections(columns, columnsToBeLoaded);
    }


    // Constructors

    public ReferencedEntityContainer(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor,
      bool isOwnerTypeKnown, PrefetchManager manager)
      : base(null, referencingFieldDescriptor.Field.Associations.Last().TargetType, true, manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(referencingFieldDescriptor, "referencingFieldDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      this.ownerKey = ownerKey;
      this.referencingFieldDescriptor = referencingFieldDescriptor;
      this.isOwnerTypeKnown = isOwnerTypeKnown;
    }
  }
}