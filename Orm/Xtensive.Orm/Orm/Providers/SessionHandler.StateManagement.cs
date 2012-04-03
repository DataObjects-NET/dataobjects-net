// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    internal virtual bool LookupState(Key key, out EntityState entityState)
    {
      return LookupStateInCache(key, out entityState);
    }

    internal virtual bool LookupState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      return LookupStateInCache(key, fieldInfo, out entitySetState);
    }

    internal virtual EntityState UpdateState(Key key, Tuple tuple)
    {
      return UpdateStateInCache(key, tuple);
    }

    internal virtual EntitySetState UpdateState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      return UpdateStateInCache(key, fieldInfo, entityKeys, isFullyLoaded);
    }

    internal bool LookupStateInCache(Key key, out EntityState entityState)
    {
      return Session.EntityStateCache.TryGetItem(key, true, out entityState);
    }

    internal bool LookupStateInCache(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
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

    internal EntityState UpdateStateInCache(Key key, Tuple tuple)
    {
      return Session.UpdateEntityState(key, tuple);
    }

    internal EntitySetState UpdateStateInCache(Key key, FieldInfo fieldInfo, IEnumerable<Key> entityKeys, bool isFullyLoaded)
    {
      if (Session.EntityStateCache[key, false]==null)
        return null;
      return Session.UpdateEntitySetState(key, fieldInfo, entityKeys, isFullyLoaded);
    }
  }
}