// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Transactions;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Building;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    ///<summary>
    /// Gets the specified <see cref="IsolationLevel"/>.
    ///</summary>
    public IsolationLevel DefaultIsolationLevel { get; internal set; }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction();

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public abstract void CommitTransaction();

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public abstract void RollbackTransaction();

    /// <summary>
    /// Persists changed entities.
    /// </summary>    
    public void Persist()
    {
      bool foreignKeysEnabled = (Session.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0;

      // Insert
      IEnumerable<EntityState> insertEntities = Session.EntityStateRegistry.GetItems(PersistenceState.New).Where(entityState=>!entityState.IsRemoved);
      if (foreignKeysEnabled)
        InsertInAccordanceWithForeignKeys(insertEntities);
      else
        foreach (EntityState data in insertEntities) {
          Insert(data);
          data.Tuple.Merge();
        }

      // Update
      foreach (EntityState data in Session.EntityStateRegistry.GetItems(PersistenceState.Modified)) {
        if (data.IsRemoved)
          continue;
        Update(data);
        data.Tuple.Merge();
      }

      // Delete
      foreach (EntityState data in Session.EntityStateRegistry.GetItems(PersistenceState.Removed))
          Remove(data);
    }

    /// <summary>
    /// Inserts the specified data into database.
    /// </summary>
    /// <param name="state">The data to insert.</param>
    protected abstract void Insert(EntityState state);

    /// <summary>
    /// Updates the specified data in database.
    /// </summary>
    /// <param name="state">The data to update.</param>
    protected abstract void Update(EntityState state);

    /// <summary>
    /// Removes the specified data from database.
    /// </summary>
    /// <param name="state">The data to remove.</param>
    protected abstract void Remove(EntityState state);

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    /// <inheritdoc/>
    public abstract void Dispose();

    protected internal virtual Key Fetch(Key key)
    {
      return Fetcher.Fetch(key);
    }

    protected internal virtual Key Fetch(Key key, FieldInfo field)
    {
      return Fetcher.Fetch(key, field);
    }

    protected internal virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    protected internal virtual IEnumerable<T> Execute<T>(Expression expression)
    {
      return Provider.Execute<IEnumerable<T>>(expression);
    }

    protected internal virtual TranslatedQuery<IEnumerable<T>> Translate<T>(Expression expression)
    {
      return Provider.Translate<IEnumerable<T>>(expression);
    }
    
    private void InsertInAccordanceWithForeignKeys(IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> loopReferences;
      List<EntityState> sortedEntities;
      List<EntityState> unreferencedEntities;
      SortAndRemoveLoopEdges(entityStates, out sortedEntities, out unreferencedEntities, out loopReferences);

      // Insert 
      sortedEntities.Reverse();
      sortedEntities.AddRange(unreferencedEntities);

      foreach (EntityState data in sortedEntities)
        Insert(data);

      // Restore loop links
      foreach (var restoreData in loopReferences) {
        Persistent.GetAccessor<Entity>(restoreData.Second).SetValue(restoreData.First.Entity, restoreData.Second, restoreData.Third);
        Update(restoreData.First);
      }

      // Merge
      foreach (EntityState data in sortedEntities)
        data.Tuple.Merge();
    }

    private void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates, out List<EntityState> sortResult, out List<EntityState> unreferencedData, out List<Triplet<EntityState, FieldInfo, Entity>> keysToRestore)
    {
      var sortData = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
      unreferencedData = new List<EntityState>();
      foreach (EntityState data in entityStates) {
        if (data.Type.GetTargetAssociations().Count==0 && data.Type.GetOwnerAssociations().Count==0)
          unreferencedData.Add(data);
        else
          sortData.Add(data.Key, new Node<EntityState, AssociationInfo>(data));
      }

      // Add connections
      foreach (var data in sortData) {
        EntityState processingEntityState = data.Value.Item;
        foreach (var association in processingEntityState.Type.GetOwnerAssociations().Where(associationInfo => associationInfo.OwnerField.IsEntity)) {
          Key foreignKey = processingEntityState.Entity.GetReferenceKey(association.OwnerField);
          Node<EntityState, AssociationInfo> destination;
          if (foreignKey!=null && !foreignKey.Equals(data.Value.Item.Key) && sortData.TryGetValue(foreignKey, out destination))
            data.Value.AddConnection(destination, true, association);
        }
      }

      // Sort
      List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
      sortResult = TopologicalSorter.Sort(sortData.Values, out removedEdges);

      // Remove loop links
      keysToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in removedEdges) {
        AssociationInfo associationInfo = edge.ConnectionItem;
        keysToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(edge.Source.Item, associationInfo.OwnerField, edge.Destination.Item.Entity));
        Persistent.GetAccessor<Entity>(associationInfo.OwnerField).SetValue(edge.Source.Item.Entity, associationInfo.OwnerField, null);
      }
    }
  }
}