// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.03

using Xtensive.Tuples;
using Xtensive.Storage.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// A resolver of indexes.
  /// </summary>
  public interface IIndexResolver
  {
    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <param name="indexInfo">The index.</param>
    /// <param name="sessionHandler"></param>
    /// <returns>The unique ordered index.</returns>
    IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo, SessionHandler sessionHandler);
  }
}