// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Disconnected.Log;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected state registry.
  /// </summary>
  internal sealed class StateRegistry
  {
    private readonly StateRegistry origin;
    private readonly DisconnectedState disconnectedState;
    private readonly Dictionary<Key, DisconnectedEntityState> states;

    private ModelHelper ModelHelper { get { return disconnectedState.ModelHelper; } }

    public OperationLog Log { get; set; }

    public StateRegistry Origin { get { return origin; } }

    public DisconnectedEntityState GetState(Key key)
    {
      DisconnectedEntityState state;
      if (states.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        return Origin.GetState(key);
      return null;
    }

    public DisconnectedEntityState GetForUpdate(Key key)
    {
      DisconnectedEntityState state;
      if (states.TryGetValue(key, out state))
        return state;
      if (Origin!=null)
        state = new DisconnectedEntityState(Origin.GetForUpdate(key));
      else 
        state = new DisconnectedEntityState(key);
      states.Add(key, state);
      return state;
    }
    
    public void Insert(Key key, Tuple tuple)
    {
      if (Origin==null)
        return;

      var state = GetForUpdate(key);
      if (state.IsLoaded)
        throw new InvalidOperationException(string.Format(
          Strings.ExStateWithKeyXIsAlreadyExists, key));

      state.Tuple = tuple.Clone();
      foreach (var item in ModelHelper.GetEntitySetItems(key, state.Tuple))
        InsertIntoEntitySet(item.OwnerKey, item.Field, item.ItemKey);
      foreach (var reference in ModelHelper.GetReferencesFrom(key, state.Tuple))
        AddReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      foreach (var fieldInfo in ModelHelper.GetEntitySetFields(key.TypeRef.Type))
        state.GetEntitySetState(fieldInfo).IsFullyLoaded = true;
    }

    public void Update(Key key, Tuple difference)
    {
      if (Origin==null)
        return;

      var state = GetForUpdate(key);
      if (!state.IsLoaded)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);

      var prevValue = state.Tuple.ToRegular();
      state.Update(difference.Clone());
      var newValue = state.Tuple.ToRegular();

      var type = state.Key.TypeRef.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity)
        return;
      // Add/remove to entity sets
      foreach (var pair in ModelHelper.GetEntitySets(type)) {
        var prevOwnerKey = ModelHelper.GetKeyFieldValue(pair.First, prevValue);
        var ownerKey = ModelHelper.GetKeyFieldValue(pair.First, newValue);
        if (ownerKey!=prevOwnerKey) {
          if (prevOwnerKey!=null)
            RemoveFromEntitySet(prevOwnerKey, pair.Second, state.Key);
          if (ownerKey!=null)
            InsertIntoEntitySet(ownerKey, pair.Second, state.Key);
        }
      }
      // Add/remove to reference mappings
      foreach (var field in ModelHelper.GetReferencingFields(type)) {
        var prevOwnerKey = ModelHelper.GetKeyFieldValue(field, prevValue);
        var ownerKey = ModelHelper.GetKeyFieldValue(field, newValue);
        if (ownerKey!=prevOwnerKey) {
          if (prevOwnerKey!=null)
            RemoveReference(prevOwnerKey, field, state.Key);
          if (ownerKey!=null)
            AddReference(ownerKey, field, state.Key);
        }
      }
    }

    public void Remove(Key key)
    {
      if (Origin==null)
        return;
      
      var state = GetForUpdate(key);
      if (!state.IsLoaded)
        throw new InvalidOperationException(Strings.ExStateIsNotLoaded);
      
      foreach (var item in ModelHelper.GetEntitySetItems(key, state.Tuple))
        RemoveFromEntitySet(item.OwnerKey, item.Field, item.ItemKey);
      foreach (var reference in ModelHelper.GetReferencesFrom(key, state.Tuple))
        RemoveReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
      state.Remove();
    }

    public void Register(Key key, Tuple tuple)
    {
      if (Origin!=null)
        return;

      var state = GetForUpdate(key);
      if (state.IsRemoved)
        return;
      if (!state.Merge(tuple.Clone()))
        return;
      foreach (var item in ModelHelper.GetEntitySetItems(key, state.Tuple))
        InsertIntoEntitySet(item.OwnerKey, item.Field, item.ItemKey);
      foreach (var reference in ModelHelper.GetReferencesFrom(key, state.Tuple))
        AddReference(reference.TargetKey, reference.Field, reference.ReferencingKey);
    }

    public void Commit()
    {
      if (Origin==null)
        return;

      foreach (var state in states)
        state.Value.Commit();
      if (Origin.Log!=null)
        Origin.Log.Append(Log);
    }

    public void AddState(DisconnectedEntityState state)
    {
      states.Add(state.Key, state);
    }

    public IEnumerable<DisconnectedEntityState> States { get { return states.Values; } }
    
    private void InsertIntoEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetForUpdate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (!entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Add(itemKey, itemKey);
    }

    private void RemoveFromEntitySet(Key ownerKey, FieldInfo field, Key itemKey)
    {
      var state = GetForUpdate(ownerKey);
      var entitySet = state.GetEntitySetState(field);
      if (entitySet.Items.ContainsKey(itemKey))
        entitySet.Items.Remove(itemKey);
    }

    private void AddReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetForUpdate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (!references.ContainsKey(itemKey))
        references.Add(itemKey, itemKey);
    }

    private void RemoveReference(Key targetKey, FieldInfo fieldInfo, Key itemKey)
    {
      var state = GetForUpdate(targetKey);
      var references = state.GetReferences(fieldInfo);
      if (references.ContainsKey(itemKey))
        references.Remove(itemKey);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="disconnectedState">State of the disconnected.</param>
    public StateRegistry(DisconnectedState disconnectedState)
    {
      ArgumentValidator.EnsureArgumentNotNull(disconnectedState, "disconnectedState");

      states = new Dictionary<Key, DisconnectedEntityState>();
      this.disconnectedState = disconnectedState;
      Log = new OperationLog();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin registry.</param>
    public StateRegistry(StateRegistry origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      states = new Dictionary<Key, DisconnectedEntityState>();
      this.origin = origin;
      disconnectedState = origin.disconnectedState;
      Log = new OperationLog();
    }


    // Serialization
    /*
    [OnSerializing]
    protected void OnSerializing(StreamingContext context)
    {
      serialized = new List<SerializedEntityState>();
      foreach (var state in states)
        serialized.Add(new SerializedEntityState(state.Value));
    }

    [OnSerialized]
    protected void OnSerialized(StreamingContext context)
    {
      
    }
    */
  }
}