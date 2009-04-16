// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using Xtensive.Indexing;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Memory
{
  public class IndexStorageView : MarshalByRefObject, IStorageView
  {
    public IndexStorage Storage { get; private set; }

    public StorageInfo Model { get; private set; }
    
    public CommandResult Execute(Command command)
    {
      if (command.Type == CommandType.Update) {
        ExecuteUpdateCommand(command as UpdateCommand);
      }
      return null;
    }

    public Dictionary<int, CommandResult> Execute(List<Command> commands)
    {
      var result = new Dictionary<int, CommandResult>(commands.Count);
      var count = 0;
      foreach (var command in commands) {
        result.Add(count++, Execute(command));
      }
      return result;
    }

    public ITransaction Transaction
    {
      get { throw new System.NotImplementedException(); }
    }

    public void Update(ActionSequence sequence)
    {
      throw new System.NotImplementedException();
    }

    public void ClearSchema()
    {
      Storage.ClearSchema();
      Model = Storage.Model;
    }

    public void CreateNewSchema(StorageInfo model)
    {
      Storage.CreateNewSchema(model);
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
      var index = GetRealPrimaryIndex(tableName);
      index.RemoveKey(key);
      index.Add(value);
    }

    private void Insert(Tuple key, Tuple value, string tableName)
    {
      var index = GetRealPrimaryIndex(tableName);
      index.Add(value);
    }
    
    private void Remove(Tuple key, string tableName)
    {
      var index = GetRealPrimaryIndex(tableName);
      index.RemoveKey(key);
    }

    private IUniqueOrderedIndex<Tuple, Tuple> GetRealPrimaryIndex(string tableName)
    {
      var indexInfo = Model.Tables[tableName].PrimaryIndex;
      return Storage.GetRealIndex(indexInfo);
    }


    // Constructor

    public IndexStorageView(IndexStorage storage, StorageInfo model)
    {
      Storage = storage;
      Model = model;
    }
  }
}