// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using Xtensive.Storage.Indexing;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class IndexUpdateCommand : UpdateCommand
  {
    public static UpdateCommand Update(string indexName, Tuple key, Tuple newValue)
    {
      return new IndexUpdateCommand { Key = key, KeyMustExist = true, TableName = indexName, Value = newValue };
    }
    
    public static UpdateCommand Insert(string indexName, Tuple key, Tuple value)
    {
      return new IndexUpdateCommand { Key = key, KeyMustExist = false, TableName = indexName, Value = value };
    }

    public static UpdateCommand Remove(string indexName, Tuple key)
    {
      return new IndexUpdateCommand { Key = key, KeyMustExist = true, TableName = indexName };
    }

    protected IndexUpdateCommand()
    {
    }
  }
}