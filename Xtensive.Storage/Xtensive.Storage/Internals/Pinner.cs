// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal sealed class Pinner
  {
    private readonly HashSet<EntityState> roots = new HashSet<EntityState>();

    private EntityChangeRegistry activeRegistry;
    private HashSet<EntityState> stronglyPinnedItems;
    private HashSet<EntityState> weaklyPinnedItems;

    public int RootCount { get { return roots.Count; } }

    public EntityChangeRegistry PinnedItems { get; private set; }
    public EntityChangeRegistry PersistableItems { get; private set; }
    
    public IDisposable RegisterRoot(EntityState state)
    {
      if (roots.Contains(state))
        return null;
      roots.Add(state);
      return new Disposable<Pinner, EntityState>(
        this, state,
        (disposing, _this, _state) => _this.roots.Remove(_state));
    }

    public void ClearRoots()
    {
      roots.Clear();
    }

    public void Process(EntityChangeRegistry registry)
    {
      activeRegistry = registry;
      PinAll();

      PinnedItems = new EntityChangeRegistry();
      PersistableItems = new EntityChangeRegistry();
      
      ProcessRegistry(PersistenceState.New);
      ProcessRegistry(PersistenceState.Modified);
      ProcessRegistry(PersistenceState.Removed);

      ClearPinned();
      activeRegistry = null;
    }

    public void Reset()
    {
      PinnedItems = null;
      PersistableItems = null;
    }

    #region Private / internal methods

    private void ProcessRegistry(PersistenceState persistenceState)
    {
      foreach (var item in activeRegistry.GetItems(persistenceState))
        if (IsPinned(item))
          PinnedItems.Register(item);
        else
          PersistableItems.Register(item);
    }
    
    private bool IsPinned(EntityState state)
    {
      return stronglyPinnedItems.Contains(state) || weaklyPinnedItems.Contains(state);
    }

    private bool Pin(EntityState state)
    {
      if (state.PersistenceState==PersistenceState.New) {
        stronglyPinnedItems.Add(state);
        return true;
      }

      weaklyPinnedItems.Add(state);
      return false;
    }

    private void PinAll()
    {
      stronglyPinnedItems = new HashSet<EntityState>();
      weaklyPinnedItems = new HashSet<EntityState>();
      
      foreach (var item in roots)
        ProcessRoot(item);
    }

    private void ProcessRoot(EntityState root)
    {
      var stack = new Stack<EntityState>();
      if (!Pin(root))
        return;
      stack.Push(root);
      while (stack.Count > 0) {
        var item = stack.Pop();
        foreach (var referencer in FindReferencers(item)) {
          if (IsPinned(referencer))
            continue;
          if (Pin(referencer))
            stack.Push(referencer);
        }
      }
    }

    private void ClearPinned()
    {
      stronglyPinnedItems = null;
      weaklyPinnedItems = null;
    }

    private IEnumerable<EntityState> FindReferencers(EntityState referencedItem)
    {
      var referencedKey = referencedItem.Key.Value;
      var referencedType = referencedItem.Type;
      foreach (var referencingAssociation in referencedType.GetTargetAssociations()) {
        var referencingType = referencingAssociation.OwnerType;
        foreach (var possibleReferencer in QueryRegistry(referencingType)) {
          var referenceValue = referencingAssociation
            .ExtractForeignKey(possibleReferencer.Type, possibleReferencer.Tuple);
          if (referencedKey.Equals(referenceValue))
            yield return possibleReferencer;
        }
      }
    }

    private IEnumerable<EntityState> QueryRegistry(TypeInfo type)
    {
      var requiredType = type.UnderlyingType;
      return activeRegistry.GetItems(PersistenceState.New)
        .Concat(activeRegistry.GetItems(PersistenceState.Modified))
        .Where(item => requiredType.IsAssignableFrom(item.Type.UnderlyingType));
    }

    #endregion
  }
}