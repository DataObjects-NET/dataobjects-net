// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class ReverseWrappingEntitySet<T1, T2, TRef> : SimpleEntitySet<T1>
    where T1 : Entity
    where T2 : Entity
    where TRef : EntitySetReference<T2, T1>
  {
    private readonly CombineTransform combineTransform;

    public override bool Add(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      if (!Contains(item)) {
        Key newEntityKey = Key.Get(typeof(TRef), CombineKey(item.Key));
        newEntityKey.Resolve();
        return true;
      }
      return false;
    }

    public override bool Contains(T1 item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      return Contains(item.Key);
    }

    public override bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (Cache.Contains(key))
        return true;
      if (Cache.Count == Count)
        return false;
      Tuple tupleKey = CombineKey(key);
      throw new NotImplementedException();
    }

    private Tuple CombineKey(Key key)
    {
      return combineTransform.Apply(TupleTransformType.TransformedTuple, ((Entity)Owner).Key.Tuple, key.Tuple);
    }

    protected override IndexInfo GetIndex()
    {
      var referencingField = Field.ReflectedType.Model.Types[typeof(TRef)].Fields["Entity1"];
      return referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
    }

    public ReverseWrappingEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
      combineTransform = new CombineTransform(true, ((Entity)owner).Key.Tuple.Descriptor, field.ReflectedType.Hierarchy.KeyTupleDescriptor);
    }
  }
}