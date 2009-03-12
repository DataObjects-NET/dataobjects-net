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
      if ((Session.Domain.Configuration.ForeignKeyMode&ForeignKeyMode.Reference)>0) {
        var insertQueue = new List<EntityState>();
        var sortEtitiyDatas = new Dictionary<Key, EntityState>();
        foreach (EntityState data in newEntities) {
          var associations = data.Type.GetAssociations();
          if (associations.Count == 0)
            insertQueue.Add(data);
          else
            sortEtitiyDatas.Add(data.Key, data);
        }
        var nodes = new Dictionary<Key, TopologicalSorter<EntityState>.Node>();
        var freeEntityStates = new SetSlim<EntityState>();
        foreach (var data in sortEtitiyDatas) {
          var associations = data.Value.Type.GetAssociations().Where(association=>association.IsMaster);
          TopologicalSorter<EntityState>.Node sourceNode = null;
          foreach (var associationInfo in associations)
          {
            Key destinationKey = data.Value.Entity.GetKey(associationInfo.ReferencingField);
            if (destinationKey!=null) {
              EntityState referencedEntityData;
              if (sortEtitiyDatas.TryGetValue(destinationKey, out referencedEntityData))
              {
                // Need to add nodes
                if (sourceNode == null)
                {
                  if (!nodes.TryGetValue(data.Key, out sourceNode))
                  {
                    sourceNode = new TopologicalSorter<EntityState>.Node(data.Value);
                    nodes.Add(data.Key, sourceNode);
                  }
                }
                TopologicalSorter<EntityState>.Node destinationNode;
                if (!nodes.TryGetValue(destinationKey, out destinationNode))
                {
                  freeEntityStates.Remove(data.Value);
                  destinationNode = new TopologicalSorter<EntityState>.Node(referencedEntityData);
                  nodes.Add(destinationKey, destinationNode);
                }
                sourceNode.AddConnection(destinationNode, true);
              }
            }
          }
          if (sourceNode==null)
            freeEntityStates.Add(data.Value);
        }
        // Sort
        List<Pair<TopologicalSorter<EntityState>.Node>> removedEdges;
        var sortResult = TopologicalSorter<EntityState>.Sort(nodes.Values, out removedEdges);
        // Remove links
        var keysToRestore = new List<Triplet<EntityState, FieldInfo, Key>>();
        foreach (Pair<TopologicalSorter<EntityState>.Node> edge in removedEdges.Distinct()) {
          EntityState item = edge.First.Item;
          foreach (var associationInfo in item.Type.GetAssociations().Where(association=>association.IsMaster && association.ReferencingType==edge.Second.Item.Type)) {
            Key foreignKey = item.Entity.GetKey(associationInfo.ReferencingField);
            keysToRestore.Add(new Triplet<EntityState, FieldInfo, Key>(item, associationInfo.ReferencingField, foreignKey));
            item.Entity.SetField<Key>(associationInfo.ReferencingField, null, false);
          }
        }
        // Insert 
        var data2 = insertQueue.Union(freeEntityStates).Union(sortResult);
        foreach (EntityState data in data2)
        {
          Insert(data);
        }
        // Update links
        foreach (var restoreData in keysToRestore) {
          restoreData.First.Entity.SetField(restoreData.Second, restoreData.Third, false);
          Update(restoreData.First);
        }

        // Merge
        foreach (EntityState data in data2)
        {
          data.Tuple.Merge();
        }
      }
      else {
        foreach (EntityState data in newEntities)
        {
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
    {}

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}