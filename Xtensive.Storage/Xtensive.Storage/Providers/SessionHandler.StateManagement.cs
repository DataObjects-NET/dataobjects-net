// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    /// <summary>
    /// Updates the state of the <see cref="EntitySet{TItem}"/>.
    /// </summary>
    /// <param name="key">The owner's key.</param>
    /// <param name="fieldInfo">The referencing field.</param>
    /// <param name="items">The items.</param>
    /// <param name="isFullyLoaded">if set to <see langword="true"/> then <paramref name="items"/> 
    /// contains all elements of an <see cref="EntitySet{TItem}"/>.</param>
    /// <returns>
    /// The updated <see cref="EntitySetState"/>, or <see langword="null"/>
    /// if a state was not found.
    /// </returns>
    protected EntitySetState UpdateEntitySetState(Key key, FieldInfo fieldInfo, IEnumerable<Key> items,
      bool isFullyLoaded)
    {
      var entityState = Session.EntityStateCache[key, true];
      if (entityState==null)
        return null;
      var entity = entityState.Entity;
      if (entity==null)
        return null;
      var entitySet = (EntitySetBase) entity.GetFieldValue(fieldInfo);
      return entitySet.UpdateState(items, isFullyLoaded);
    }

    internal virtual EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return Session.UpdateEntityState(key, tuple);
    }

    internal virtual EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      if (Session.EntityStateCache[key, false]==null)
        return null;
      return UpdateEntitySetState(key, fieldInfo, entityKeys, isFullyLoaded);
    }

    internal virtual bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return Session.EntityStateCache.TryGetItem(key, true, out entityState);
    }

    internal virtual bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
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
  }
}