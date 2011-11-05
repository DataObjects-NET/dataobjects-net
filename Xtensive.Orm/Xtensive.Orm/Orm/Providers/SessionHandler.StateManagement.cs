// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    internal virtual bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return TryGetEntityStateFromSessionCache(key, out entityState);
    }

    internal virtual bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      return TryGetEntitySetStateFromSessionCache(key, fieldInfo, out entitySetState);
    }

    internal virtual EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return UpdateEntityStateInSessionCache(key, tuple, false);
    }

    internal virtual EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      return UpdateEntitySetStateInSessionCache(key, fieldInfo, entityKeys, isFullyLoaded);
    }

    public bool TryGetEntityStateFromSessionCache(Key key, out EntityState entityState)
    {
      return Session.EntityStateCache.TryGetItem(key, true, out entityState);
    }

    public bool TryGetEntitySetStateFromSessionCache(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      var entityState = Session.EntityStateCache[key, false];
      if (entityState!=null) {
        var entity = entityState.Entity;
        if (entity!=null) {
          var entitySet = (EntitySetBase) entity.GetFieldValue(fieldInfo);
          if (entitySet.CheckStateIsLoaded()) {
            entitySetState = entitySet.State;
            return true;
          }
        }
      }
      entitySetState = null;
      return false;
    }

    public EntityState UpdateEntityStateInSessionCache(Key key, Tuple tuple, bool isStale)
    {
      return Session.UpdateEntityState(key, tuple);
    }

    public EntitySetState UpdateEntitySetStateInSessionCache(Key key, FieldInfo fieldInfo, IEnumerable<Key> entityKeys, bool isFullyLoaded)
    {
      if (Session.EntityStateCache[key, false]==null)
        return null;
      return Session.UpdateEntitySetState(key, fieldInfo, entityKeys, isFullyLoaded);
    }
  }
}