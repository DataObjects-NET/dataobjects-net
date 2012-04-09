// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Disposing;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal sealed class Pinner : SessionBound
  {
    private readonly HashSet<EntityState> roots = new HashSet<EntityState>();

    private EntityChangeRegistry activeRegistry;
    private HashSet<EntityState> pinnedItems;

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

      PinnedItems = new EntityChangeRegistry(Session);
      PersistableItems = new EntityChangeRegistry(Session);
      
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
      return pinnedItems.Contains(state);
    }

    private bool Pin(EntityState state)
    {
      pinnedItems.Add(state);
      return state.PersistenceState==PersistenceState.New;
    }

    private void PinAll()
    {
      pinnedItems = new HashSet<EntityState>();
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
      pinnedItems = null;
    }

    private IEnumerable<EntityState> FindReferencers(EntityState referencedItem)
    {
      var referencedKey = referencedItem.Key.Value;
      var referencedType = referencedItem.Type;
      foreach (var referencingAssociation in referencedType.GetTargetAssociations()) {
        var referencingType = referencingAssociation.OwnerType;
        foreach (var possibleReferencer in QueryRegistry(referencingType)) {
          var referenceValue = referencingAssociation.Master
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

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"><see cref="Session"/>, to which current instance 
    /// is bound.</param>
    public Pinner(Session session)
      : base(session)
    {
    }
  }
}