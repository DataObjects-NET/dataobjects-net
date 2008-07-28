// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.09

using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class KeyResolver
  {
    public static Entity Resolve(Key key)
    {
      Session session = Session.Current;

      // Key is already resolved
      EntityData data;
      if (session.DataCache.TryGetValue(key, out data))
        return data.Entity ?? Entity.Activate(data.Type.UnderlyingType, data);

      Tuple tuple;
      if (key.Type==null) {
        // TypeId is unknown, so 1 fetch request required
        FieldInfo field = key.Hierarchy.Root.Fields[session.Domain.NameProvider.TypeId];
        tuple = Fetcher.Fetch(key, field);
        if (tuple == null)
          return null;
        int columnIndex = key.Hierarchy.Root.Fields[session.Domain.NameProvider.TypeId].MappingInfo.Offset;
        int typeId = tuple.GetValue<int>(columnIndex);
        key.Type = key.Type = session.Domain.Model.Types[typeId];
      }
      else {
        // Creating empty Entity
        tuple = Tuple.Create(key.Type.TupleDescriptor);
        key.Tuple.CopyTo(tuple);
      }

      // TODO: Refactor
      // EntityData registration
      data = new EntityData(key, new DifferentialTuple(tuple));
      data.Entity = Entity.Activate(data.Type.UnderlyingType, data);
      return data.Entity;
    }
  }
}