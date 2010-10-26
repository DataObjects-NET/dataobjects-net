// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;
using Xtensive.Transactions;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Transactions;
using Xtensive.Tuples.Transform;
using Xtensive.Storage.Providers.Indexing.Resources;

namespace Xtensive.Storage.Providers.Indexing.Memory
{
  /// <summary>
  /// View of "in memory" indexing storage.
  /// </summary>
  public sealed class MemoryIndexStorageView : IndexStorageView
  {
    private readonly Providers.SessionHandler sessionHandler;
    private readonly MemoryIndexTransaction transaction;

    /// <inheritdoc/>
    public override CommandResult Execute(Command command)
    {
      if (command.Type==CommandType.Update)
        ExecuteUpdateCommand(command as UpdateCommand);

      return null;
    }

    /// <inheritdoc/>
    public override Dictionary<int, CommandResult> Execute(List<Command> commands)
    {
      foreach (var command in commands)
        Execute(command);

      return new Dictionary<int, CommandResult>();
    }

    /// <inheritdoc/>
    public override ITransaction Transaction
    {
      get { return transaction; }
    }

    /// <inheritdoc/>
    public override void Update(ActionSequence sequence)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override void ClearSchema()
    {
      ((MemoryIndexStorage) Storage).ClearSchema();
      Model = Storage.Model;
    }

    /// <inheritdoc/>
    public override void CreateNewSchema(StorageInfo model)
    {
      ((MemoryIndexStorage) Storage).CreateNewSchema(model);
      Model = Storage.Model;
    }
    
    /// <inheritdoc/>
    public override IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo, Providers.SessionHandler sessionHandler)
    {
      return Storage.GetRealIndex(indexInfo);
    }

    #region Private / internal methods
    
    private void ExecuteUpdateCommand(UpdateCommand command)
    {
      if (!command.KeyMustExist)
        Insert(command.Key, command.Value, command.TableName);
      else if (command.Value!=null)
        Update(command.Key, command.Value, command.TableName);
      else
        Remove(command.Key, command.TableName);
    }

    private void Update(Tuple key, Tuple value, string tableName)
    {
      var table = Model.Tables[tableName];
      var primaryIndex = GetIndex(table.PrimaryIndex, sessionHandler);
      var oldValue = FindTuple(tableName, key);
      var newValue = oldValue.Clone();
      newValue.MergeWith(value, MergeBehavior.PreferDifference);

      primaryIndex.RemoveKey(key);
      primaryIndex.Add(newValue);

      foreach (var indexInfo in table.SecondaryIndexes) {
        var transform = Storage.GetTransform(indexInfo);
        var oldTransformed = transform.Apply(TupleTransformType.Tuple, oldValue);
        if (!oldTransformed.GetFieldState(0).IsAvailable())
          continue;
        var newTransformed = transform.Apply(TupleTransformType.Tuple, newValue);
        var secondaryIndex = GetIndex(indexInfo, sessionHandler);
        secondaryIndex.Remove(oldTransformed);
        secondaryIndex.Add(newTransformed);
      }
    }

    private void Insert(Tuple key, Tuple value, string tableName)
    {
      var table = Model.Tables[tableName];
      var primaryIndex = GetIndex(table.PrimaryIndex, sessionHandler);
      primaryIndex.Add(value);

      foreach (var indexInfo in table.SecondaryIndexes) {
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value);
        if (!transformedTuple.GetFieldState(0).IsAvailable()) 
          continue;
        var secondaryIndex = GetIndex(indexInfo, sessionHandler);
        secondaryIndex.Add(transformedTuple);
      }
    }

    private void Remove(Tuple key, string tableName)
    {
      var value = FindTuple(tableName, key);
      var table = Model.Tables[tableName];
      var primaryIndex = GetIndex(table.PrimaryIndex, sessionHandler);
      primaryIndex.RemoveKey(value);

      foreach (var indexInfo in table.SecondaryIndexes) {
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value);
        var secondaryIndex = GetIndex(indexInfo, sessionHandler);
        secondaryIndex.Remove(transformedTuple);
      }
    }

    /// <exception cref="InvalidOperationException">Instance with specific key is not found.</exception>
    private Tuple FindTuple(string tableName, Tuple key)
    {
      var indexInfo = Model.Tables[tableName].PrimaryIndex;
      var primaryIndex = GetIndex(indexInfo, sessionHandler);
      var seekResult = primaryIndex.Seek(key);
      if (seekResult.ResultType!=SeekResultType.Exact)
        throw new InvalidOperationException(
          string.Format(Strings.ExInstanceXIsNotFound, tableName));

      return seekResult.Result;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="model">The model.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    public MemoryIndexStorageView(MemoryIndexStorage storage, StorageInfo model, Providers.SessionHandler sessionHandler, IsolationLevel isolationLevel)
      :base(storage, model, sessionHandler)
    {
      this.sessionHandler = sessionHandler;
      transaction = new MemoryIndexTransaction(Guid.NewGuid(), isolationLevel);
    }
  }
}