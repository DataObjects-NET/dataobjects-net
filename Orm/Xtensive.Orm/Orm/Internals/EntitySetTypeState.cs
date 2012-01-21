// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.04

using System;
using Xtensive.Tuples;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class EntitySetTypeState
  {
    public readonly ExecutableProvider SeekProvider;

    public readonly MapTransform SeekTransform;

    public readonly Func<Tuple, Entity> ItemCtor;

    public readonly Func<QueryEndpoint,long> ItemCountQuery;

    public EntitySetTypeState(ExecutableProvider seekProvider, MapTransform seekTransform,
      Func<Tuple, Entity> itemCtor, Func<QueryEndpoint, long> itemCountQuery)
    {
      SeekProvider = seekProvider;
      SeekTransform = seekTransform;
      ItemCtor = itemCtor;
      ItemCountQuery = itemCountQuery;
    }
  }
}