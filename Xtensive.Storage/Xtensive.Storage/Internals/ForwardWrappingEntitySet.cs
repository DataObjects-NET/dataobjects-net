// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class ForwardWrappingEntitySet<T1, T2, TRef> : SimpleEntitySet<T1>
    where T1 : Entity
    where T2 : Entity
    where TRef : EntitySetReference<T1, T2>
  {
//    private readonly CombineTransform combineTransform;
//    private readonly IndexInfo index;
//
//    public override bool Add(T1 item)
//    {
//      ArgumentValidator.EnsureArgumentNotNull(item, "item");
//      if (!Contains(item)) {
//        Key newEntityKey = Key.Get(typeof (TRef), combineTransform.Apply(TupleTransformType.TransformedTuple, item.Key.Tuple, ((Entity) Owner).Key.Tuple));
//        newEntityKey.Resolve();
//        return true;
//      }
//      return false;
//    }
//
//    public override bool Contains(T1 item)
//    {
//      ArgumentValidator.EnsureArgumentNotNull(item, "item");
//      return Contains(item.Key);
//    }
//
//    public override bool Contains(Key key)
//    {
//      ArgumentValidator.EnsureArgumentNotNull(key, "key");
//      if (Cache.Contains(key))
//        return true;
//      if (Cache.Count == Count)
//        return false;
//    }
//
//    

    public ForwardWrappingEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
//      combineTransform = new CombineTransform(true, field.ReflectedType.Hierarchy.KeyTupleDescriptor, ((Entity)owner).Key.Tuple.Descriptor);
//      index = owner.Type.Model.Types[typeof (TRef)].Indexes.PrimaryIndex;
    }
  }
}