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
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal class EntitySet<T, TRef> : EntitySet<T>
    where T : Entity
    where TRef : Entity
  {
    private static readonly Func<Tuple, TRef> constructorDelegate = DelegateHelper.CreateDelegate<Func<Tuple, TRef>>(null, typeof(TRef), DelegateHelper.AspectedProtectedConstructorCallerName, ArrayUtils<Type>.EmptyArray);
    private readonly CombineTransform combineTransform;
    private readonly bool isReverse;

    public override bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!Contains(item)) {
        constructorDelegate(CombineKey(item.Key));
        if (Field.Association.Multiplicity==Multiplicity.ManyToMany) {
          // Update paired EntitySet
          FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
          var accessor = referencingField.GetAccessor<EntitySet>();
          var pairedEntitySet = accessor.GetValue(item, referencingField);
          pairedEntitySet.AddToCache(((Entity) Owner).Key, false);
        }
        AddToCache(item.Key, false);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
        return true;
      }
      return false;
    }

    public override bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (Contains(item)) {
        Key entityKey = Key.Get(typeof (TRef), CombineKey(item.Key));
        var referenceEntity = (TRef) entityKey.Resolve(); // Resolve entity
        referenceEntity.Remove();
        if (Field.Association.Multiplicity==Multiplicity.ManyToMany) {
          //update paired EntitySet
          FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
          var accessor = referencingField.GetAccessor<EntitySet>();
          var pairedEntitySet = accessor.GetValue(item, referencingField);
          pairedEntitySet.RemoveFromCache(((Entity) Owner).Key, false);
        }
        RemoveFromCache(item.Key, false);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        return true;
      }
      return false;
    }

    public override bool Contains(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    protected override IndexInfo GetIndex()
    {
      if (isReverse)
        return Field.ReflectedType.Model.Types[typeof (TRef)].Indexes.First(indexInfo => indexInfo.IsSecondary);
      return Field.ReflectedType.Model.Types[typeof (TRef)].Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
    }

    protected override MapTransform GetKeyTransform()
    {
      TypeInfoCollection types = Session.Domain.Model.Types;
      var field = isReverse ? types[typeof (TRef)].Fields["Entity2"] : types[typeof (TRef)].Fields["Entity1"];
      var columns = field.Fields.ExtractColumns();
      var keyTupleDescriptor = types[typeof (T)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = columns.Select(columnInfo => Index.Columns.First(columnInfo2 => columnInfo2.Name==columnInfo.Name)).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    private Tuple CombineKey(Key key)
    {
      if (isReverse)
        return combineTransform.Apply(TupleTransformType.TransformedTuple, ((Entity) Owner).Key, key);
      return combineTransform.Apply(TupleTransformType.TransformedTuple, key, ((Entity) Owner).Key);
    }


    // Constructors.

    public EntitySet(Persistent owner, FieldInfo field, bool isReverse)
      : base(owner, field)
    {
      this.isReverse = isReverse;
      combineTransform = isReverse
        ? new CombineTransform(true, ((Entity) owner).Key.Descriptor, field.Association.ReferencedType.Hierarchy.KeyTupleDescriptor)
        : new CombineTransform(true, field.Association.ReferencedType.Hierarchy.KeyTupleDescriptor, ((Entity) owner).Key.Descriptor);
    }
  }
}