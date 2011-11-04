// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using Xtensive.Indexing;
using Xtensive.Orm;
using Xtensive.Storage.Model;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Providers.Indexing.Memory
{
  /// <summary>
  /// <see cref="Session"/>-level handler for memory index storage.
  /// </summary>
  public class SessionHandler : Indexing.SessionHandler
  {
    public override IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      var result = base.GetIndex(indexInfo);
      if (result == null) {
        var memoryIndexStorage = (MemoryIndexStorage) storage;
        result = memoryIndexStorage.realIndexes[indexInfo];
      }
      return result;
    }
  }
}