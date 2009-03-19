// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Building;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base session handler class.
  /// </summary>
  public abstract class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    ///<summary>
    /// Gets the specified <see cref="System.Transactions.IsolationLevel"/>.
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
      IEnumerable<EntityState> newEntities = Session.EntityStateRegistry.GetItems(PersistenceState.New);
      if ((Session.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0) {
        InsertAccordingForeignKeys(newEntities);
      }
      else {
        foreach (EntityState data in newEntities) {
          Insert(data);
          data.Tuple.Merge();
        }
      }
      foreach (EntityState data in Session.EntityStateRegistry.GetItems(PersistenceState.Modified)) {
        if (data.IsRemoved)
          continue;
        Update(data);
        data.Tuple.Merge();
      }
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

    private void InsertAccordingForeignKeys(IEnumerable<EntityState> entityStates)
    {
      // Create nodes
      var insertQueue = new List<EntityState>();
      var sortData = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
      var unreferencedData = new List<EntityState>();
      foreach (EntityState data in entityStates) {
        if (data.Type.GetAssociations().Count==0 && data.Type.GetOutgoingAssociations().Count==0)
          unreferencedData.Add(data);
        else
          sortData.Add(data.Key, new Node<EntityState, AssociationInfo>(data));
      }

      // Add connections
      foreach (var data in sortData) {
        EntityState processingEntityState = data.Value.Item;
        foreach (var association in processingEntityState.Type.GetOutgoingAssociations().Where(associationInfo => associationInfo.ReferencingField.IsEntity)) {
          Key foreignKey = processingEntityState.Entity.GetKey(association.ReferencingField);
          Node<EntityState, AssociationInfo> destination;
          if (foreignKey!=null && !foreignKey.Equals(data.Value.Item.Key) && sortData.TryGetValue(foreignKey, out destination))
            data.Value.AddConnection(destination, true, association);
        }
      }

      // Sort
      List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
      var sortResult = TopologicalSorter.Sort(sortData.Values, out removedEdges);

      // Remove loop links
      var keysToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in removedEdges) {
        AssociationInfo associationInfo = edge.ConnectionItem;
        keysToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(edge.Source.Item, associationInfo.ReferencingField, edge.Destination.Item.Entity));
        edge.Source.Item.Entity.SetFieldValue<object>(associationInfo.ReferencingField, null, false);
      }
      sortResult.Reverse();

      // Insert 
      insertQueue.AddRange(sortResult);
      insertQueue.AddRange(unreferencedData);

      foreach (EntityState data in insertQueue)
        Insert(data);

      // Restore loop links
      foreach (var restoreData in keysToRestore) {
        restoreData.First.Entity.SetFieldValue<object>(restoreData.Second, restoreData.Third, false);
        Update(restoreData.First);
      }

      // Merge
      foreach (EntityState data in insertQueue)
        data.Tuple.Merge();
    }
  }
}