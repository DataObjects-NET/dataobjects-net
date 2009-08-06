// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.04

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class EntitySetTypeState
  {
    public readonly RecordSet SeekRecordSet;

    public readonly CombineTransform SeekTransform;

    public readonly Func<Tuple, Entity> ItemCtor;

    public readonly Delegate ItemsQuery;

    public readonly Delegate ItemCountQuery;

    public readonly Delegate RestrictedItemsQuery;
    
    public EntitySetTypeState(RecordSet seekRecordSet, CombineTransform seekTransform,
      Func<Tuple, Entity> itemCtor, Delegate itemsQuery, Delegate itemCountQuery, Delegate restrictedItemsQuery)
    {
      SeekRecordSet = seekRecordSet;
      SeekTransform = seekTransform;
      ItemCtor = itemCtor;
      ItemsQuery = itemsQuery;
      ItemCountQuery = itemCountQuery;
      RestrictedItemsQuery = restrictedItemsQuery;
    }
  }
}