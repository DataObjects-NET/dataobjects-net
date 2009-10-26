// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
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

    public Tuple Tuple
    {
      get
      {
        if (tuple!=null)
          return tuple;
        if (Origin!=null)
          return Origin.Tuple;
        return null;
      }
      set
      {
        if (tuple!=null)
          throw new InvalidOperationException("State is already contains value.");

        tuple = Origin!=null && Origin.Tuple != null
          ? new DifferentialTuple(Origin.Tuple, value) 
          : new DifferentialTuple(value);
      }
    }

    public DisconnectedEntityState Origin { get; private set; }

    public bool IsLoaded { get { return IsRemoved || tuple!=null || Tuple!=null; } }

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
        : new ChainedDictionary<Key, Key>(Origin.GetReferences(field));
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

    /// <exception cref="InvalidOperationException">State is not loaded.</exception>
    public void Remove()
    {
      IsRemoved = true;
      tuple = null;
      foreach (var state in setStates)
        state.Value.Items.Clear();
      references.Clear();
    }

    public bool Merge(Tuple newValue)
    {
      if (Origin!=null)
        return false;
      
      if (tuple==null) {
        tuple = new DifferentialTuple(newValue.Clone());
        return true;
      }
      
      return MergeTuples(tuple.Origin, newValue);
    }

    /// <summary>
    /// Commits changes to origin state.
    /// </summary>
    public void Commit()
    {
      if (Origin==null)
        return;
      if (IsRemoved && Origin.IsLoaded) {
        Origin.Remove();
        return;
      }
      foreach (var state in setStates)
        state.Value.Commit();
      foreach (var reference in references) {
        var refs = reference.Value as ChainedDictionary<Key, Key>;
        if (refs!=null)
          refs.Commit();
      }
      if (tuple!=null)
        if (Origin.Tuple==null)
          Origin.Tuple = tuple;
        else 
          Origin.Update(tuple.Difference);
    }

    private static bool MergeTuples(Tuple origin, Tuple newValue)
    {
      bool isMerged = false;
      for (int i = 0; i < origin.Count; i++)
        if (!origin.GetFieldState(i).IsAvailable() 
          && newValue.GetFieldState(i).IsAvailable()) {
          origin.SetValue(i, newValue.GetValue(i));
          isMerged = true;
        }
      return isMerged;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="key">The key.</param>
    public DisconnectedEntityState(Key key)
    {
      Key = key;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The origin state.</param>
    public DisconnectedEntityState(DisconnectedEntityState origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");

      Origin = origin;
      Key = origin.Key;
    }
  }
}