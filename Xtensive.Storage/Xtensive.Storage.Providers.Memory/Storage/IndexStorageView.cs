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

namespace Xtensive.Storage.Providers.Memory
{
  /// <summary>
  /// View of "in memory" indexing storage.
  /// </summary>
  public class IndexStorageView : Index.IndexStorageView
  {
    private readonly IndexTransaction transaction;

    /// <inheritdoc/>
    public override CommandResult Execute(Command command)
    {
      if (command.Type == CommandType.Update)
        ExecuteUpdateCommand(command as UpdateCommand);
      
      return null;
    }

    /// <inheritdoc/>
    public override Dictionary<int, CommandResult> Execute(List<Command> commands)
    {
      var result = new Dictionary<int, CommandResult>(commands.Count);
      var count = 0;
      foreach (var command in commands)
        result.Add(count++, Execute(command));
      
      return result;
    }

    /// <inheritdoc/>
    public override ITransaction Transaction
    {
      get { return transaction; }
    }

    /// <inheritdoc/>
    public override IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      return Storage.GetRealIndex(indexInfo);
    }
    
    /// <inheritdoc/>
    public override void Update(ActionSequence sequence)
    {
      throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override void ClearSchema()
    {
      ((IndexStorage)Storage).ClearSchema();
      Model = Storage.Model;
    }

    /// <inheritdoc/>
    public override void CreateNewSchema(StorageInfo model)
    {
      ((IndexStorage)Storage).CreateNewSchema(model);
      Model = Storage.Model;
    }

    private void ExecuteUpdateCommand(UpdateCommand cmd)
    {
      if (!cmd.KeyMustExist) {
        Insert(cmd.Key, cmd.Value, cmd.TableName);
      }
      else if(cmd.Value!=null) {
        Update(cmd.Key, cmd.Value, cmd.TableName);
      }
      else {
        Remove(cmd.Key, cmd.TableName);
      }
    }

    private void Update(Tuple key, Tuple value, string tableName)
    {
      var oldValue = FindTuple(tableName, key);
      var newValue = Tuple.Create(oldValue.Descriptor);
      oldValue.CopyTo(newValue);
      newValue.MergeWith(value, MergeBehavior.PreferDifference);

      foreach (var indexInfo in GetAllIndexes(tableName)) {
        var realIndex = Storage.GetRealIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var oldTransformed = transform.Apply(TupleTransformType.Tuple, oldValue).ToFastReadOnly();
        var newTransformed = transform.Apply(TupleTransformType.Tuple, newValue);
        realIndex.Remove(oldTransformed);
        realIndex.Add(newTransformed);
      }
    }

    private void Insert(Tuple key, Tuple value, string tableName)
    {
      foreach (var indexInfo in GetAllIndexes(tableName)) {
        var realIndex = Storage.GetRealIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value).ToFastReadOnly();
        realIndex.Add(transformedTuple);
      }
    }

    private void Remove(Tuple key, string tableName)
    {
      var value = FindTuple(tableName, key);
      foreach (var indexInfo in GetAllIndexes(tableName)) {
        var realIndex = Storage.GetRealIndex(indexInfo);
        var transform = Storage.GetTransform(indexInfo);
        var transformedTuple = transform.Apply(TupleTransformType.Tuple, value).ToFastReadOnly();
        realIndex.Remove(transformedTuple);
      }
    }

    private IEnumerable<IndexInfo> GetAllIndexes(string tableName)
    {
      var table = Model.Tables.SingleOrDefault(tableInfo => tableInfo.PrimaryIndex.Name==tableName);
      if (table==null)
        yield return null;
      yield return table.PrimaryIndex;
      foreach (var indexInfo in table.SecondaryIndexes) {
        yield return indexInfo;
      }
    }

    private Tuple FindTuple(string tableName, Tuple key)
    {
      var indexInfo = Model.Tables.Single(table=>table.PrimaryIndex.Name==tableName).PrimaryIndex;
      var primaryIndex = Storage.GetRealIndex(indexInfo);
      var seekResult = primaryIndex.Seek(new Ray<Entire<Tuple>>(new Entire<Tuple>(key)));
      if (seekResult.ResultType != SeekResultType.Exact)
        throw new InvalidOperationException();
      
      return seekResult.Result;
    }

    
    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="model">The model.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    public IndexStorageView(IndexStorage storage, StorageInfo model, IsolationLevel isolationLevel)
      :base(storage, model)
    {
      transaction = new IndexTransaction(Guid.NewGuid(), isolationLevel);
    }
  }
}