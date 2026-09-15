// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class ReferencedEntityContainer : EntityContainer
  {
    private readonly Key ownerKey;
    private readonly bool isOwnerTypeKnown;
    private readonly PrefetchFieldDescriptor referencingFieldDescriptor;
    private bool needToNotifyOwner;

    public FieldInfo ReferencingField => referencingFieldDescriptor.Field;

    public override EntityGroupTask GetTask()
    {
      if (Key is not null && Task is null)
        return null;

      if (Task is not null)
        return Task;

      var isStateCached = Manager.Owner.LookupState(ownerKey, out var ownerState);
      if (isStateCached) {
        if (ownerState is null)
          return null;
        if (ownerState.Tuple is null)
          return null;
        if (ownerState.PersistenceState == PersistenceState.Removed)
          return null;
      }
      if (!isStateCached)
        throw new KeyNotFoundException(
          String.Format(Strings.ExReferencingEntityWithKeyXIsNotFound, ownerKey));
      if (!ownerState.IsTupleLoaded)
        throw Exceptions.InternalError(Strings.ExReferencingEntityTupleIsNotLoaded, OrmLog.Instance);
      if (!isOwnerTypeKnown && !ownerState.Key.TypeReference.Type.Fields.Contains(ReferencingField))
        return null;
      var foreignKeyTuple = ExtractForeignKeyTuple(ownerState);
      if (foreignKeyTuple is null)
        return null;
      var session = Manager.Owner.Session;
      Key = Key.Create(session.Domain, session.StorageNodeId, Type, TypeReferenceAccuracy.BaseType, foreignKeyTuple);
      return CreateTask();
    }

    public void NotifyOwnerAboutKeyWithUnknownType()
    {
      if (needToNotifyOwner && Task is not null)
        referencingFieldDescriptor.NotifySubscriber(ownerKey, Key);
    }

    private Tuple ExtractForeignKeyTuple(EntityState ownerState)
    {
      var association = ReferencingField.Associations[^1];
      var result = association.ExtractForeignKey(ownerState.Type, ownerState.Tuple);
      var tupleState = result.GetFieldStateMap(TupleFieldState.Null);
      for (int i = 0, count = result.Count; i < count; i++) {
        if (!result.GetFieldState(i).IsAvailable())
          if (isOwnerTypeKnown)
            throw Exceptions.InternalError(Strings.ExForeignKeyValueHaveNotBeenLoaded, OrmLog.Instance);
          else
            return null;
        if (tupleState[i])
          return null;
      }
      return result;
    }

    private EntityGroupTask CreateTask()
    {
      var hasExactTypeBeenGotten = PrefetchHelper.TryGetExactKeyType(Key, Manager, out var exactReferencedType);
      if (hasExactTypeBeenGotten is not null) {
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

      Manager.GetCachedColumnIndexes(Type, descriptors, out var columns, out var columnsToBeLoaded);
      SetColumnCollections(columns, columnsToBeLoaded);
    }


    // Constructors

    public ReferencedEntityContainer(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor,
      bool isOwnerTypeKnown, PrefetchManager manager)
      : base(null, referencingFieldDescriptor.Field.Associations[^1].TargetType, true, manager)
    {
      this.ownerKey = ownerKey ?? throw new ArgumentNullException(nameof(ownerKey));
      this.referencingFieldDescriptor = referencingFieldDescriptor ?? throw new ArgumentNullException(nameof(referencingFieldDescriptor));
      this.isOwnerTypeKnown = isOwnerTypeKnown;
    }
  }
}
