// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using System.Linq;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class SimpleEntitySet<T> : EntitySet<T>
    where T : Entity
  {
    private IndexInfo index;

    /// <inheritdoc/>
    public override int RemoveWhere(Predicate<T> match)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override void CopyTo(T[] array, int arrayIndex)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override T this[T item]
    {
      get { throw new System.NotImplementedException(); }
    }

    /// <inheritdoc/>
    public override bool Contains(T item)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool Add(T item)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool Remove(T item)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override long Count
    {
      get { throw new System.NotImplementedException(); }
    }

    public SimpleEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
      FieldInfo referencingField = field.Association.PairTo.ReferencingField;
      index = referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
//      Tuple key = ((Entity) owner).Key.Tuple;
//      var rs = index.ToRecordSet().Range(key, key);
//      var ent = rs.ToEntities<T>();
    }
  }
}