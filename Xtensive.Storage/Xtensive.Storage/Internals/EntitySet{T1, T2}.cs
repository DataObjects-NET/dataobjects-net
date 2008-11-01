// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal class EntitySet<TEntity, TEntitySetItem> : EntitySet<TEntity>
    where TEntity : Entity
    where TEntitySetItem : Entity
  {
    private static readonly Func<Tuple, TEntitySetItem> itemConstructor = DelegateHelper.CreateDelegate<Func<Tuple, TEntitySetItem>>(null, typeof(TEntitySetItem), DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);

    /// <inheritdoc/>
    public override bool Add(TEntity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Add, OwnerEntity, item, Field.Association);

      itemConstructor(item.Key.CombineWith(OwnerEntity.Key));
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
      return true;
    }

    /// <inheritdoc/>
    public override bool Remove(TEntity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (!Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Remove, OwnerEntity, item, Field.Association);

      Key entityKey = new Key(typeof (TEntitySetItem), item.Key.CombineWith(OwnerEntity.Key));
      var referenceEntity = (TEntitySetItem) entityKey.Resolve(); // Resolve entity
      referenceEntity.Remove();
      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
      return true;
    }

    public override bool Contains(TEntity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    #region Initialization members

    protected override IndexInfo GetIndex()
    {
      return Field.ReflectedType.Model.Types[typeof (TEntitySetItem)].Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
    }

    protected override MapTransform GetKeyExtractTransform()
    {
      TypeInfoCollection types = Session.Domain.Model.Types;
      var field = types[typeof (TEntitySetItem)].Fields[Session.Domain.NameBuilder.EntitySetItemMasterFieldName];
      var columns = field.Fields.ExtractColumns();
      var keyTupleDescriptor = types[typeof (TEntity)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = columns.Select(columnInfo => Index.Columns.First(columnInfo2 => columnInfo2.Name==columnInfo.Name)).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    #endregion


    // Constructors.

    public EntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}