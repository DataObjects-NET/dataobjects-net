// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  public class ReverseWrappingEntitySet<T1, T2, TRef> : EntitySet<T1>
    where T1 : Entity
    where T2 : Entity
    where TRef : EntitySetReference<T2, T1>
  {
    private EntitySet<T2, T1, TRef> set;

    public override int RemoveWhere(Predicate<T1> match)
    {
      throw new NotImplementedException();
    }

    public override void Clear()
    {
      throw new NotImplementedException();
    }

    public override void CopyTo(T1[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    public override T1 this[T1 item]
    {
      get { throw new NotImplementedException(); }
    }

    public override bool Contains(T1 item)
    {
      throw new NotImplementedException();
    }

    public override bool Add(T1 item)
    {
      throw new NotImplementedException();
    }

    public override IEnumerator<T1> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public override bool Remove(T1 item)
    {
      throw new NotImplementedException();
    }

    public override long Count
    {
      get { throw new NotImplementedException(); }
    }

    internal override void ClearCache()
    {
      throw new NotImplementedException();
    }

    internal override void AddToCache(Key key)
    {
      throw new NotImplementedException();
    }

    internal override void RemoveFromCache(Key key)
    {
      throw new NotImplementedException();
    }

    public ReverseWrappingEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
      set = new EntitySet<T2, T1, TRef>(owner);
    }
  }
}