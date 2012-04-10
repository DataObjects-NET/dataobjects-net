// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Disconnected
{
  /// <summary>
  /// Disconnected state registry.
  /// </summary>
  internal sealed class StateRegistry
  {
    private readonly DisconnectedState owner;
    private readonly StateRegistry origin;
    private readonly Dictionary<Key, DisconnectedEntityState> items;
    private readonly AssociationCache associationCache;

    public StateRegistry Origin { get { return origin; } }

    public IEnumerable<Key> Keys { get { return items.Keys; } }
    public IEnumerable<DisconnectedEntityState> EntityStates { get { return items.Values; } }
    public OperationLog Operations { get; set; }

    public DisconnectedEntityState Get(Key key)
    {
      DisconnectedEntityState state;
      if (items.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        return Origin.Get(key);
      return null;
    }

    public DisconnectedEntityState GetOrCreate(Key key)
    {
      DisconnectedEntityState state;
      if (items.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        state = new DisconnectedEntityState(Origin.GetOrCreate(key));
      else 
        state = new DisconnectedEntityState(key);
      items.Add(key, state);
      return state;
    }

    public void Create(Key key, Tuple tuple, bool isLoaded)
    {
      if (isLoaded)
        EnsureOriginIsNull();
      else
        EnsureOriginNotNull();

      var state = GetOrCreate(key);
      if (state.IsLoadedOrRemoved)
        throw new InvalidOperationException(string.Format(
          Strings.ExStateWithKeyXIsAlreadyExists, key));

      state.Tuple = tuple;
      OnStateChanged(key, null, tuple);

      if (!isLoaded)
        foreach (var fieldInfo in associationCache.GetEntitySetFields(key.TypeReference.Type))
          state.GetEntitySetState(fieldInfo).IsFullyLoaded = true;
    }

    public void Update(Key key, Tuple difference)
    {
      EnsureOriginNotNull();

      var state = GetOrCreate(key);
      if (!state.IsLoadedOrRemoved)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);

      var oldTuple = state.Tuple.ToRegular();
      state.Update(difference);
      var newTuple = state.Tuple.ToRegular();

      var type = state.Key.TypeReference.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof(EntitySetItem<,>);
      if (isAuxEntity)
        return;
      OnStateChanged(key, oldTuple, newTuple);
    }

    public void Remove(Key key)
    {
      EnsureOriginNotNull();
      
      var state = GetOrCreate(key);
      if (!state.IsLoadedOrRemoved)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);
      
      OnStateChanged(key, state.Tuple, null);
      state.Remove();
    }

    public void UpdateOrigin(Key key, Tuple tuple, MergeBehavior mergeBehavior)
    {
      if (Origin!=null)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      var state = GetOrCreate(key);
      if (state.IsRemoved || !state.IsLoadedOrRemoved)
        throw new InvalidOperationException(Strings.ExCantMergeState);

      if (state.UpdateOrigin(tuple, mergeBehavior))
        OnStateChanged(key, null, state.Tuple);
    }

    public void Commit(bool clearLoggedOperations)
    {
      try {
        if (Origin==null)
          return;

        foreach (var state in items)
          state.Value.Commit();
        if (Origin.Operations!=null)
          Origin.Operations.Log(Operations);
      }
      finally {
        if (clearLoggedOperations)
          Operations = new OperationLog(owner.OperationLogType);
      }
    }

    /// <exception cref="InvalidOperationException">Origin!=null</exception>
    public void Remap(KeyMapping keyMapping)
    {
      if (origin!=null)
        throw Exceptions.InternalError("Origin!=null", OrmLog.Instance);
      foreach (var map in keyMapping.Map) {
        var hasToBeReplaced = false;
        foreach (var itemPair in items) {
          if (!hasToBeReplaced && itemPair.Key==map.Key)
            hasToBeReplaced = true;
          itemPair.Value.Remap(map.Key, map.Value);
        }
        if (!hasToBeReplaced)
          continue;
        DisconnectedEntityState entityState;
        if (items.TryGetValue(map.Key, out entityState)) {
          items.Remove(map.Key);
          items.Add(map.Value, entityState);
        }
      }
    }

    public void AddState(DisconnectedEntityState state)
    {
      items.Add(state.Key, state);
    }

    public void OnStateChanged(Key key, Tuple oldTuple, Tuple newTuple)
    {
      // Inserting
      if (oldTuple==null) {
        foreach (var item in associationCache.GetEntitySetItems(key, newTuple))
          InsertIntoEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in associationCache.GetReferencesFrom(key, newTuple))
          AddReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Deleting
      else if (newTuple==null) {
        foreach (var item in associationCache.GetEntitySetItems(key, oldTuple))
          RemoveFromEntitySet(item.OwnerKey, item.Field, item.ItemKey);
        foreach (var reference in associationCache.GetReferencesFrom(key, oldTuple))
          RemoveReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      }
      // Updating
      else {
        foreach (var pair in associationCache.GetEntitySets(key.TypeInfo)) {
          var prevOwnerKey = associationCache.GetKeyFieldValue(pair.First, oldTuple);
          var ownerKey = associationCache.GetKeyFieldValue(pair.First, newTuple);
          if (ownerKey!=prevOwnerKey) {
            if (prevOwnerKey!=null)
              RemoveFromEntitySet(prevOwnerKey, pair.Second, key);
            if (ownerKey!=null)
              InsertIntoEntitySet(ownerKey, pair.Second, key);
          }
        }
        foreach (var field in associationCache.GetReferencingFields(key.TypeInfo)) {
          var prevOwnerKey = associationCache.GetKeyFieldValue(field, oldTuple);
          var ownerKey = associationCache.GetKeyFieldValue(field, newTuple);
          if (ownerKey!=prevOwnerKey) {
            if (prevOwnerKey!=null)
              RemoveReference(prevOwnerKey, field, key);
            if (ownerKey!=null)
              AddReference(ownerKey, field, key);
          }
        }
      }
    }

    #region Private \ internal methods

    private void EnsureOriginNotNull()
    {
      if (Origin==null)
        throw Exceptions.InternalError(Strings.ExOriginIsNull, OrmLog.Instance);
    }

    private void EnsureOriginIsNull()
    {
      if (Origin!=null)
        throw Exceptions.InternalError(Strings.ExOriginIsNotNull, OrmLog.Instance);
    }

    private void InsertIntoEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetOrCreate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (!entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Add(itemKey, itemKey);
    }

    private void RemoveFromEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetOrCreate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Remove(itemKey);
    }

    private void AddReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetOrCreate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (!references.ContainsKey(itemKey))
        references.Add(itemKey, itemKey);
    }

    private void RemoveReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetOrCreate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (references.ContainsKey(itemKey))
        references.Remove(itemKey);
    }

    #endregion


    // Constructors

    public StateRegistry(DisconnectedState owner, AssociationCache associationCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(owner, "owner");
      ArgumentValidator.EnsureArgumentNotNull(associationCache, "modelRequestCache");

      this.owner = owner;
      items = new Dictionary<Key, DisconnectedEntityState>();
      this.associationCache = associationCache;
      Operations = new OperationLog(owner.OperationLogType);
    }

    public StateRegistry(StateRegistry origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      owner = origin.owner;
      this.origin = origin;
      items = new Dictionary<Key, DisconnectedEntityState>();
      associationCache = origin.associationCache;
      Operations = new OperationLog(owner.OperationLogType);
    }
  }
}