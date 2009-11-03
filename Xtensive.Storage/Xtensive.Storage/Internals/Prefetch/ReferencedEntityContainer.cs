// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals.Prefetch
{
  [Serializable]
  internal sealed class ReferencedEntityContainer : EntityContainer
  {
    private static readonly object defaultFieldsCachingRegion = new object();
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
      var isStateCached = Processor.Owner.TryGetEntityState(ownerKey, out ownerState);
      if (isStateCached
        && (ownerState.Tuple==null || ownerState.PersistenceState==PersistenceState.Removed))
        return null;
      if (!isStateCached)
        throw new KeyNotFoundException(
          String.Format(Strings.ExReferencingEntityWithKeyXIsNotFound, ownerKey));
      if (!ownerState.IsTupleLoaded)
        throw Exceptions.InternalError(Strings.ExReferencingEntityTupleIsNotLoaded, Log.Instance);
      if (!isOwnerTypeKnown && !ownerState.Key.Type.Fields.Contains(ReferencingField))
        return null;
      var foreignKeyTuple = ExtractForeignKeyTuple(ownerState);
      if (foreignKeyTuple == null)
        return null;
      Key = Key.Create(Processor.Owner.Session.Domain, Type, TypeReferenceAccuracy.BaseType, foreignKeyTuple);
      return CreateTask();
    }

    public void NotifyOwnerAboutKeyWithUnknownType()
    {
      if (needToNotifyOwner && Task != null)
        referencingFieldDescriptor.NotifySubscriber(ownerKey, Key);
    }

    private Tuple ExtractForeignKeyTuple(EntityState ownerState)
    {
      var result = ReferencingField.Association.ExtractForeignKey(ownerState.Tuple);
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
      if (!Key.TypeRef.Type.IsLeaf) {
        needToNotifyOwner = true;
        var cachedKey = Key;
        Tuple tuple;
        if (!Processor.TryGetTupleOfNonRemovedEntity(ref cachedKey, out tuple))
          return null;
        if (cachedKey.HasExactType && ReferencingField.Association.TargetType!=cachedKey.Type) {
          Type = cachedKey.Type;
          FillColumnCollection();
          needToNotifyOwner = false;
        }
      }
      if (!SelectColumnsToBeLoaded())
        return null;
      Task = new EntityGroupTask(Type, ColumnIndexesToBeLoaded.ToArray(), Processor);
      return Task;
    }

    private void FillColumnCollection()
    {
      var fieldsToBeLoaded = (IEnumerable<FieldInfo>) Processor.Owner.Session.Domain
        .GetCachedItem(new Pair<object, TypeInfo>(defaultFieldsCachingRegion, Type),
          pair => ((Pair<object, TypeInfo>) pair).Second.Fields
            .Where(field => field.Parent == null && PrefetchHelper.IsFieldToBeLoadedByDefault(field)));
      foreach (var field in fieldsToBeLoaded)
        AddColumns(field.Columns);
    }


    // Constructors

    public ReferencedEntityContainer(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor,
      bool isOwnerTypeKnown, PrefetchProcessor processor)
      : base(null, referencingFieldDescriptor.Field.Association.TargetType, true, processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(referencingFieldDescriptor, "referencingFieldDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      this.ownerKey = ownerKey;
      this.referencingFieldDescriptor = referencingFieldDescriptor;
      this.isOwnerTypeKnown = isOwnerTypeKnown;
      FillColumnCollection();
    }
  }
}