// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Disconnected
{
  /// <summary>
  /// Disconnected entity state
  /// </summary>
  [DebuggerDisplay("Key = {Key}, Tuple = {Tuple}, IsRemoved = {IsRemoved}")]
  internal sealed class DisconnectedEntityState
  {
    private readonly Dictionary<FieldInfo, DisconnectedEntitySetState> setStates = 
      new Dictionary<FieldInfo, DisconnectedEntitySetState>();
    private readonly Dictionary<FieldInfo, IDictionary<Key, Key>> references = 
      new Dictionary<FieldInfo, IDictionary<Key, Key>>();
    
    private DifferentialTuple tuple;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is removed.
    /// </summary>
    public bool IsRemoved { get; private set; }

    public Tuple Tuple {
      get {
        if (tuple!=null)
          return tuple;
        if (Origin!=null)
          return Origin.Tuple;
        return null;
      }
      set {
        if (tuple!=null)
          throw Exceptions.AlreadyInitialized("Tuple");

        tuple = Origin!=null && Origin.Tuple!=null
          ? new DifferentialTuple(Origin.Tuple, value) 
          : new DifferentialTuple(value);
      }
    }

    public DisconnectedEntityState Origin { get; private set; }

    public bool IsLoadedOrRemoved {
      get { return tuple!=null || IsRemoved || Tuple!=null; }
    }

    public DisconnectedEntitySetState GetEntitySetState(FieldInfo field)
    {
      DisconnectedEntitySetState state;
      if (setStates.TryGetValue(field, out state))
        return state;

      state = Origin==null 
        ? new DisconnectedEntitySetState(Enumerable.Empty<Key>(), false) 
        : new DisconnectedEntitySetState(Origin.GetEntitySetState(field));
      setStates.Add(field, state);
      return state;
    }

    public IDictionary<Key, Key> GetReferences(FieldInfo field)
    {
      IDictionary<Key, Key> refs;
      if (references.TryGetValue(field, out refs))
        return refs;

      refs = Origin==null
        ? (IDictionary<Key, Key>) new Dictionary<Key, Key>()
        : new DifferentialDictionary<Key, Key>(Origin.GetReferences(field));
      references.Add(field, refs);
      return refs;
    }

    public void Update(Tuple difference)
    {
      if (tuple==null)
        tuple = new DifferentialTuple(Origin.Tuple, difference);
      else if (tuple.Difference==null)
        tuple.Difference = difference;
      else 
        tuple.Difference.MergeWith(difference, MergeBehavior.PreferDifference);
    }

    public bool UpdateOrigin(Tuple difference, MergeBehavior mergeBehavior)
    {
      if (Origin!=null)
        return Origin.UpdateOrigin(difference, mergeBehavior);

      bool wasChanged = false;
      var origin = tuple.Origin;
      if (mergeBehavior==MergeBehavior.PreferOrigin) {
        for (int i = 0; i < origin.Count; i++)
          if (!origin.GetFieldState(i).IsAvailable() && difference.GetFieldState(i).IsAvailable()) {
            origin.SetValue(i, difference.GetValue(i));
            wasChanged = true;
          }
      }
      else { // PreferDifference
        for (int i = 0; i < origin.Count; i++)
          if (difference.GetFieldState(i).IsAvailable()) {
            origin.SetValue(i, difference.GetValue(i));
            wasChanged = true;
          }
      }
      return wasChanged;
    }

    /// <exception cref="InvalidOperationException">State is not loaded.</exception>
    public void Remove()
    {
      IsRemoved = true;
      tuple = null;
      foreach (var state in setStates)
        state.Value.Items.Clear();
      references.Clear();
    }

    /// <summary>
    /// Commits changes to origin state.
    /// </summary>
    public void Commit()
    {
      if (Origin==null)
        return;
      if (IsRemoved && Origin.IsLoadedOrRemoved) {
        Origin.Remove();
        return;
      }
      foreach (var state in setStates)
        state.Value.Commit();
      foreach (var reference in references) {
        var refs = reference.Value as DifferentialDictionary<Key, Key>;
        if (refs!=null)
          refs.ApplyChanges();
      }
      if (tuple!=null)
        if (Origin.Tuple==null)
          Origin.Tuple = tuple;
        else 
          Origin.Update(tuple.Difference);
    }

    #region Private \ internal methods

    internal void Remap(Key oldKey, Key newKey)
    {
      Key realEntityKey;
      if (Key==oldKey) {
        if (tuple!=null)
          newKey.Value.CopyTo(tuple, 0, oldKey.Value.Count);
        Key = newKey;
      }
      if (references.Count > 0)
        foreach (var backReferences in references.Values)
          ReplaceKey(oldKey, newKey, backReferences);
      if (setStates.Count > 0)
        foreach (var setState in setStates.Values)
          ReplaceKey(oldKey, newKey, setState.Items);
    }

    private static void ReplaceKey(Key oldKey, Key newKey, IDictionary<Key, Key> keys)
    {
      if (keys.Remove(oldKey))
        keys.Add(newKey, newKey);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="key">The key.</param>
    public DisconnectedEntityState(Key key)
    {
      Key = key;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="origin">The origin state.</param>
    public DisconnectedEntityState(DisconnectedEntityState origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      Origin = origin;
      Key = origin.Key;
    }

    #region Serializing members

    public SerializableEntityState ToSerializable()
    {
      if (!Key.HasExactType && tuple!=null)
        throw Exceptions.InternalError(Strings.ExCannotAssociateNonEmptyEntityStateWithKeyOfUnknownType, Log.Instance);

      var key = Key.ToString(true);
      var type = Key.TypeReference.Type.UnderlyingType.FullName;
      var refs = references.Select(pair => {
        var dictionary = pair.Value as DifferentialDictionary<Key, Key>;
        var difference = dictionary!=null ? dictionary.Difference : null;
        return new SerializableReference {
          Field = new FieldInfoRef(pair.Key),
          AddedItems = difference==null
            ? null
            : difference.AddedItems.Select(keyPair => keyPair.Key.ToString(true)).ToArray(),
          RemovedItems = difference==null
            ? null
            : difference.RemovedItems.Select(keyPair => keyPair.Key.ToString(true)).ToArray(),
          Items = dictionary==null
            ? pair.Value.Select(keyPair => keyPair.Key.ToString(true)).ToArray()
            : null
        };
      }).ToArray();
      var entitySets = setStates.Select(pair => {
        var dictionary = pair.Value.Items as DifferentialDictionary<Key, Key>;
        var difference = dictionary!=null ? dictionary.Difference : null;
        return new SerializableEntitySet {
          Field = new FieldInfoRef(pair.Key),
          IsFullyLoaded = pair.Value.IsFullyLoaded,
          AddedItems = difference==null
            ? null
            : difference.AddedItems.Select(keyPair => keyPair.Key.ToString(true)).ToArray(),
          RemovedItems = difference==null
            ? null
            : difference.RemovedItems.Select(keyPair => keyPair.Key.ToString(true)).ToArray(),
          Items = dictionary==null
            ? pair.Value.Items.Select(keyPair => keyPair.Key.ToString(true)).ToArray()
            : null
        };
      }).ToArray();

      return new SerializableEntityState() {
        Key = key,
        Type = type,
        Tuple = tuple,
        IsRemoved = IsRemoved,
        References = refs,
        EntitySets = entitySets
      };
    }

    public static DisconnectedEntityState FromSerializable(SerializableEntityState serialized, StateRegistry registry, Domain domain)
    {
      var key = Key.Parse(domain, serialized.Key);
      var type = domain.Model.Types.Find(serialized.Type);
      // Getting key with exact type
      key = Key.Create(domain, type, TypeReferenceAccuracy.ExactType, key.Value);

      var origin = registry.Origin!=null ? registry.Origin.Get(key) : null;
      var state = origin!=null
        ? new DisconnectedEntityState(origin)
        : new DisconnectedEntityState(key);
      state.IsRemoved = serialized.IsRemoved;
      if (serialized.Tuple!=null)
        state.Tuple = serialized.Tuple;
      if (serialized.References!=null) {
        foreach (var reference in serialized.References) {
          var field = reference.Field.Resolve(domain.Model);
          var dictionary = state.GetReferences(field);
          if (reference.Items!=null)
            foreach (var item in reference.Items) {
              var itemKey = Key.Parse(domain, item);
              dictionary.Add(itemKey, itemKey);
            }
          if (reference.AddedItems!=null)
            foreach (var item in reference.AddedItems) {
              var itemKey = Key.Parse(domain, item);
              dictionary.Add(itemKey, itemKey);
            }
          if (reference.RemovedItems!=null)
            foreach (var item in reference.RemovedItems) {
              var itemKey = Key.Parse(domain, item);
              dictionary.Remove(itemKey);
            }
        }
      }
      if (serialized.EntitySets!=null) {
        foreach (var entitySet in serialized.EntitySets) {
          var field = entitySet.Field.Resolve(domain.Model);
          var setState = state.GetEntitySetState(field);
          setState.IsFullyLoaded = entitySet.IsFullyLoaded;
          if (entitySet.Items!=null)
            foreach (var item in entitySet.Items) {
              var itemKey = Key.Parse(domain, item);
              setState.Items.Add(itemKey, itemKey);
            }
          if (entitySet.AddedItems!=null)
            foreach (var item in entitySet.AddedItems) {
              var itemKey = Key.Parse(domain, item);
              setState.Items.Add(itemKey, itemKey);
            }
          if (entitySet.RemovedItems!=null)
            foreach (var item in entitySet.RemovedItems) {
              var itemKey = Key.Parse(domain, item);
              setState.Items.Remove(itemKey);
            }
        }
      }
      return state;
    }

    #endregion
  }
}