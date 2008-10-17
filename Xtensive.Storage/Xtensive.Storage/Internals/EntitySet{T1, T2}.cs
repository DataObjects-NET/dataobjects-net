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
  internal class EntitySet<T1, T2> : EntitySet<T1>
    where T1 : Entity
    where T2 : Entity
  {
    private static readonly Func<Tuple, T2> itemConstructor = DelegateHelper.CreateDelegate<Func<Tuple, T2>>(null, typeof(T2), DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);
    protected CombineTransform KeyCombineTransform { get; private set; }

    /// <inheritdoc/>
    public override bool Add(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Add, OwnerEntity, item, Field.Association);

      itemConstructor(KeyCombineTransform.Apply(TupleTransformType.TransformedTuple, item.Key, OwnerEntity.Key));
      OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
      return true;
    }

    /// <inheritdoc/>
    public override bool Remove(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      if (!Contains(item))
        return false;

      AssociationInfo association = Field.Association;
      if (association!=null && association.IsPaired)
        SyncManager.Enlist(OperationType.Remove, OwnerEntity, item, Field.Association);

      Key entityKey = Key.Get(typeof (T2), KeyCombineTransform.Apply(TupleTransformType.TransformedTuple, item.Key, OwnerEntity.Key));
      var referenceEntity = (T2) entityKey.Resolve(); // Resolve entity
      referenceEntity.Remove();
      OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
      return true;
    }

    public override bool Contains(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    #region Initialization members

    protected internal override void Initialize()
    {
      base.Initialize();
      KeyCombineTransform = GetKeyCombineTransform();
    }

    protected override IndexInfo GetIndex()
    {
      return Field.ReflectedType.Model.Types[typeof (T2)].Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
    }

    protected override MapTransform GetKeyExtractTransform()
    {
      TypeInfoCollection types = Session.Domain.Model.Types;
      var field = types[typeof (T2)].Fields[Session.Domain.NameBuilder.EntitySetItemMasterFieldName];
      var columns = field.Fields.ExtractColumns();
      var keyTupleDescriptor = types[typeof (T1)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = columns.Select(columnInfo => Index.Columns.First(columnInfo2 => columnInfo2.Name==columnInfo.Name)).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    protected virtual CombineTransform GetKeyCombineTransform()
    {
      return new CombineTransform(true, Field.Association.ReferencedType.Hierarchy.KeyTupleDescriptor, OwnerEntity.Key.Descriptor);
    }

    #endregion


    // Constructors.

    public EntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}