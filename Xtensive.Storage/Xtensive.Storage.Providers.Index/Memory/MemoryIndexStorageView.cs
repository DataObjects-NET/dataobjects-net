// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Indexing;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Core.Tuples;
using System.Transactions;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Providers.Index.Resources;

namespace Xtensive.Storage.Providers.Index.Memory
{
  /// <summary>
  /// View of "in memory" indexing storage.
  /// </summary>
  public sealed class MemoryIndexStorageView : IndexStorageView
  {
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
    public override IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
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

    private void Update(Tuple key, Tuple value, string primaryIndexName)
    {
      var oldValue = FindTuple(primaryIndexName, key);
      var newValue = Tuple.Create(oldValue.Descriptor);
      oldValue.CopyTo(newValue);
      newValue.MergeWith(value, MergeBehavior.PreferDifference);

      foreach (var indexInfo in GetAffectedIndexes(primaryIndexName)) {
        var realIndex = GetIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var oldTransformed = transform.Apply(TupleTransformType.Tuple, oldValue).ToFastReadOnly();
        var newTransformed = transform.Apply(TupleTransformType.Tuple, newValue);
        realIndex.Remove(oldTransformed);
        realIndex.Add(newTransformed);
      }
    }

    private void Insert(Tuple key, Tuple value, string primaryIndexName)
    {
      foreach (var indexInfo in GetAffectedIndexes(primaryIndexName)) {
        var realIndex = GetIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value).ToFastReadOnly();
        realIndex.Add(transformedTuple);
      }
    }

    private void Remove(Tuple key, string primaryIndexName)
    {
      var value = FindTuple(primaryIndexName, key);
      foreach (var indexInfo in GetAffectedIndexes(primaryIndexName)) {
        var realIndex = GetIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value).ToFastReadOnly();
        realIndex.Remove(transformedTuple);
      }
    }

    private IEnumerable<IndexInfo> GetAffectedIndexes(string primaryIndexName)
    {
      var table = Model.Tables.Single(tableInfo => tableInfo.PrimaryIndex.Name==primaryIndexName);
      yield return table.PrimaryIndex;
      foreach (var indexInfo in table.SecondaryIndexes)
        yield return indexInfo;
    }

    /// <exception cref="InvalidOperationException">Instance with specific key is not found.</exception>
    private Tuple FindTuple(string primaryIndexName, Tuple key)
    {
      var indexInfo = Model.Tables.Select(table => table.PrimaryIndex)
        .Single(index => index.Name==primaryIndexName);
      var primaryIndex = GetIndex(indexInfo);
      var seekResult = primaryIndex.Seek(new Ray<Entire<Tuple>>(new Entire<Tuple>(key)));
      if (seekResult.ResultType!=SeekResultType.Exact)
        throw new InvalidOperationException(
          string.Format(Strings.ExInstanceXIsNotFound, primaryIndexName));

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
    public MemoryIndexStorageView(MemoryIndexStorage storage, StorageInfo model, IsolationLevel isolationLevel)
      :base(storage, model)
    {
      transaction = new MemoryIndexTransaction(Guid.NewGuid(), isolationLevel);
    }
  }
}