// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class SimpleEntitySet<T> : EntitySet<T>
    where T : Entity
  {
    public override int RemoveWhere(Predicate<T> match)
    {
      throw new System.NotImplementedException();
    }

    public override void Clear()
    {
      throw new System.NotImplementedException();
    }

    public override void CopyTo(T[] array, int arrayIndex)
    {
      throw new System.NotImplementedException();
    }

    public override T this[T item]
    {
      get { throw new System.NotImplementedException(); }
    }

    public override bool Contains(T item)
    {
      throw new System.NotImplementedException();
    }

    public override bool Add(T item)
    {
      throw new System.NotImplementedException();
    }

    public override bool Remove(T item)
    {
      throw new System.NotImplementedException();
    }

    public override long Count
    {
      get { throw new System.NotImplementedException(); }
    }

    public SimpleEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}