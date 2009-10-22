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

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class ReferencedEntityPrefetchContainer : EntityPrefetchContainer
  {
    private readonly MapTransform referencedEntityKeyTransform;
    private readonly Key ownerKey;
    private readonly bool isOwnerTypeKnown;

    public readonly FieldInfo ReferencingField;

    public override EntityGroupPrefetchTask GetTask()
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
      var foreignKeyTuple = ReferencingField.Association.ExtractForeignKey(ownerState.Tuple);
      for (int i = 0; i < foreignKeyTuple.Count; i++) {
        if (!foreignKeyTuple.GetFieldState(i).IsAvailable())
          if (isOwnerTypeKnown)
            throw Exceptions.InternalError(Strings.ExForeignKeyValueHaveNotBeenLoaded, Log.Instance);
          else
            return null;
        if ((foreignKeyTuple.GetFieldState(i) & TupleFieldState.Null)==TupleFieldState.Null)
          return null;
      }
      Key = Key.Create(Processor.Owner.Session.Domain, Type, TypeReferenceAccuracy.BaseType, foreignKeyTuple);
      if (!SelectColumnsToBeLoaded())
        return null;
      Task = new EntityGroupPrefetchTask(Type, ColumnIndexesToBeLoaded.ToArray(), Processor);
      return Task;
    }


    // Constructors

    public ReferencedEntityPrefetchContainer(Key ownerKey, FieldInfo referencingField, bool isOwnerTypeKnown,
      PrefetchProcessor processor)
      : base(null, referencingField.Association.TargetType, true, processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(referencingField, "referencingField");
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      this.ownerKey = ownerKey;
      ReferencingField = referencingField;
      this.isOwnerTypeKnown = isOwnerTypeKnown;
      var fieldsToBeLoaded = Type.Fields.Where(field => field.Parent == null
        && PrefetchHelper.IsFieldToBeLoadedByDefault(field));
      foreach (var field in fieldsToBeLoaded)
        AddColumns(field.Columns);
    }
  }
}