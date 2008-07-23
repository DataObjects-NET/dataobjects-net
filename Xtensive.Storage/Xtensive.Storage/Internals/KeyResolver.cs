// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Internals
{
  internal static class KeyResolver
  {
    public static Entity Resolve(Key key)
    {
      Session session = Session.Current;

      EntityData data = session.IdentityMap[key, false];
      if (data!=null)
        return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);

      Tuple tuple;
      if (key.Type!=null) {
        tuple = Tuple.Create(key.Type.TupleDescriptor);
        key.Tuple.Copy(tuple);
        data = new EntityData(key, new DifferentialTuple(tuple));
        data.Entity = Entity.Activate(data.Type.UnderlyingType, data);
        return data.Entity;
      }

      tuple = Fetcher.Fetch(key);
      if (tuple==null)
        return null;

      ResolveType(key, tuple);
      data = new EntityData(key, new DifferentialTuple(tuple));
      data.Entity = Entity.Activate(data.Type.UnderlyingType, data);
      return data.Entity;
    }

    internal static void ResolveType(Key key, Tuple tuple)
    {
      Session session = Session.Current;
      int columnIndex = key.Hierarchy.Root.Fields[session.Domain.NameProvider.TypeId].MappingInfo.Offset;
      int typeId = tuple.GetValue<int>(columnIndex);
      key.Type = session.Domain.Model.Types[typeId];
    }
  }
}