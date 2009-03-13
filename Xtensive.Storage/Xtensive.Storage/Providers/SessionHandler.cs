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
        var insertQueue = new List<EntityState>();
        var sortEtitiyDatas = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
        foreach (EntityState data in newEntities) {
          sortEtitiyDatas.Add(data.Key, new Node<EntityState, AssociationInfo>(data));
        }
        foreach (var data in sortEtitiyDatas) {
          EntityState processingEntityState = data.Value.Item;
          var associations = processingEntityState.Type.GetAssociations();
          foreach (var association in associations) {
            Node<EntityState, AssociationInfo> source = null;
            Node<EntityState, AssociationInfo> destination = null;
            switch (association.Multiplicity) {
              case Multiplicity.ZeroToOne:
              case Multiplicity.ManyToOne:
//                index = association.ReferencingType.Indexes.GetIndex(association.ReferencingField.Name);
//                var key = data.Value.Entity.Key;
//                rs = index.ToRecordSet().Range(keyTuple, keyTuple);
//                foreach (Entity item in rs.ToEntities(association.ReferencingField.DeclaringType.UnderlyingType))
//                  yield return item;
                break;
              case Multiplicity.OneToOne:
              case Multiplicity.OneToMany:
                source = data.Value;
                Key foreignKey = data.Value.Item.Entity.GetKey(association.Reversed.ReferencingField);
                if (foreignKey!=null && !foreignKey.Equals(data.Value.Item.Key))
                  sortEtitiyDatas.TryGetValue(foreignKey, out destination);
                break;
              case Multiplicity.ZeroToMany:
              case Multiplicity.ManyToMany:
                // Do nothing - session automatically persisits any time access to entityset
                break;
            }
            if (source!=null && destination!=null) {
              source.AddConnection(destination, true, association);
            }
          }
        }
        // Sort
        List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
        var sortResult = TopologicalSorter.Sort(sortEtitiyDatas.Values, out removedEdges);
        // Remove links
        var keysToRestore = new List<Triplet<EntityState, FieldInfo, Key>>();
        foreach (var edge in removedEdges) {
          AssociationInfo associationInfo = edge.ConnectionItem;
          Key foreignKey = edge.Source.Item.Entity.GetKey(associationInfo.ReferencingField);
          keysToRestore.Add(new Triplet<EntityState, FieldInfo, Key>(edge.Source.Item, associationInfo.ReferencingField, foreignKey));
          edge.Source.Item.Entity.SetField<object>(associationInfo.ReferencingField, null, false);
        }
        // Insert 
        var dataToInsert = insertQueue.Union(sortResult);
        foreach (EntityState data in dataToInsert) {
          Insert(data);
        }
        // Update links
        foreach (var restoreData in keysToRestore) {
          restoreData.First.Entity.SetField<object>(restoreData.Second, restoreData.Third, false);
          Update(restoreData.First);
        }

        // Merge
        foreach (EntityState data in dataToInsert)
          data.Tuple.Merge();
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
  }
}