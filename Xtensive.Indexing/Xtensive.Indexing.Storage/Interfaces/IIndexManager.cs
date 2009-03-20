// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using Xtensive.Indexing.Storage.Model;

namespace Xtensive.Indexing.Storage.Interfaces
{
  /// <summary>
  /// Index manager API (DDL API).
  /// Manages the indexes stored in the <see cref="IStorage"/>.
  /// </summary>
  public interface IIndexManager
  {
    ///// <summary>
    ///// Gets the current storage model.
    ///// </summary>
    //StorageInfo Model { get; }

    ///// <summary>
    ///// Creates a new index in the storage.
    ///// </summary>
    ///// <param name="index">The index to create.</param>
    //void CreateIndex(IndexInfo index);

    ///// <summary>
    ///// Removes the index from the storage.
    ///// </summary>
    ///// <param name="indexName">Name of the index to remove.</param>
    //void RemoveIndex(string indexName);

    ///// <summary>
    ///// Changes the index.
    ///// </summary>
    ///// <param name="changeInfo">The index change specification.</param>
    //void ChangeIndex(IndexChangeInfo changeInfo);

    ///// <summary>
    ///// Copies the index.
    ///// </summary>
    ///// <param name="changeInfo">The index copy specification.</param>
    //void CopyIndex(IndexChangeInfo changeInfo);
  }
}