// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public partial class Session
  {
    internal ICache<Key, EntityState> EntityStateCache { get; private set; }
    internal EntityStateRegistry EntityStateRegistry { get; private set; }

    internal EntityState CreateEntityState(Key key)
    {
      // Checking for deleted entity with the same key
      var cachedState = EntityStateCache[key, false];
      if (cachedState != null && cachedState.PersistenceState==PersistenceState.Removed)
        Persist();
      else
        EntityStateRegistry.EnforceSizeLimit(); // Must be done before new entity registration

      // If type is unknown, we consider tuple is null, 
      // so its Entity is considered as non-existing
      Tuple tuple = null;
      if (key.IsTypeCached)
        // A tuple with all the fields set to default values rather then N/A
        tuple = key.Type.CreateEntityTuple(key.Value);

      var result = new EntityState(this, key, tuple) {
        PersistenceState = PersistenceState.New
      };
      EntityStateCache.Add(result);

      if (IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Caching: {1}", this, result);
      return result;
    }

    /// <exception cref="InvalidOperationException">
    /// Attempt to associate non-null <paramref name="tuple"/> with <paramref name="key"/> of unknown type.
    /// </exception>
    internal EntityState UpdateEntityState(Key key, Tuple tuple)
    {
      var result = EntityStateCache[key, true];
      if (result == null) {
        if (!key.IsTypeCached && tuple!=null)
          throw Exceptions.InternalError(Strings.ExCannotAssociateNonEmptyEntityStateWithKeyOfUnknownType, Log.Instance);
        result = new EntityState(this, key, tuple) {
          PersistenceState = PersistenceState.Synchronized
        };
        EntityStateCache.Add(result);
        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Caching: {1}", this, result);
      }
      else {
        result.Update(tuple);
        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Updating cache: {1}", this, result);
      }
      return result;
    }
  }
}