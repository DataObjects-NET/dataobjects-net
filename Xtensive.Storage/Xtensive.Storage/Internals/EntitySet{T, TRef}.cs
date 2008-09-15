// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
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
    private readonly CombineTransform combineTransform;
    private bool isReverse;

    public override bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!Contains(item)) {
        typeof(TRef).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Tuple) }, null).Invoke(new object[] { CombineKey(item.Key) });
        if (Field.Association.Multiplicity == Multiplicity.ManyToMany) {
          //update paired EntitySet
          FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
          var accessor = referencingField.GetAccessor<EntitySet>();
          var pairedEntitySet = accessor.GetValue(item, referencingField);
          pairedEntitySet.AddToCache(((Entity)Owner).Key, false);
        }
        AddToCache(item.Key, false);
        return true;
      }
      return false;
    }


    public override bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (Contains(item)) {
        Key entityKey = Key.Get(typeof(TRef), CombineKey(item.Key));
        var referenceEntity = (TRef)entityKey.Resolve(); // Resolve entity
        referenceEntity.Remove();
        if (Field.Association.Multiplicity == Multiplicity.ManyToMany) {
          //update paired EntitySet
          FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
          var accessor = referencingField.GetAccessor<EntitySet>();
          var pairedEntitySet = accessor.GetValue(item, referencingField);
          pairedEntitySet.RemoveFromCache(((Entity)Owner).Key, false);
        }
        RemoveFromCache(item.Key, false);
        return true;
      }
      return false;
    }

    public override bool Contains(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    private Tuple CombineKey(Key key)
    {
      if (isReverse)
        return combineTransform.Apply(TupleTransformType.TransformedTuple, key.Tuple, ((Entity) Owner).Key.Tuple);
      return combineTransform.Apply(TupleTransformType.TransformedTuple, key.Tuple, ((Entity)Owner).Key.Tuple);
    }

    protected override IndexInfo GetIndex()
    {
      if (isReverse)
        return Field.ReflectedType.Model.Types[typeof (TRef)].Indexes.First(indexInfo => indexInfo.IsSecondary);
      return Field.ReflectedType.Model.Types[typeof(TRef)].Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
    }

    protected override MapTransform GetKeyTransform()
    {
      throw new NotImplementedException();
      var keyTupleDescriptor = Owner.Session.Domain.Model.Types[typeof(T)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = Index.Columns.Where(columnInfo => columnInfo.IsPrimaryKey).Select(columnInfo => Index.Columns.IndexOf(columnInfo));
      return new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }

    public EntitySet(Persistent owner, FieldInfo field, bool isReverse)
      : base(owner, field)
    {
      this.isReverse = isReverse;
      combineTransform = isReverse 
        ? new CombineTransform(true, ((Entity)owner).Key.Tuple.Descriptor, field.ReflectedType.Hierarchy.KeyTupleDescriptor) 
        : new CombineTransform(true, field.ReflectedType.Hierarchy.KeyTupleDescriptor, ((Entity)owner).Key.Tuple.Descriptor);
    }
  }
}