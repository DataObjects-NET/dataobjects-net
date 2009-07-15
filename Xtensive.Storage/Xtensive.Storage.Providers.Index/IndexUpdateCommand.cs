// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Indexing;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// Update command for indexing storage.
  /// </summary>
  [Serializable]
  public class IndexUpdateCommand : UpdateCommand
  {
    /// <summary>
    /// Creates update command.
    /// </summary>
    /// <param name="indexName">Name of the primary index.</param>
    /// <param name="key">The key.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>Update command.</returns>
    public static UpdateCommand Update(string indexName, Tuple key, Tuple newValue)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = true,
          TableName = indexName,
          Value = newValue
        };
    }

    /// <summary>
    /// Creates insert command.
    /// </summary>
    /// <param name="indexName">Name of the primary index.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Insert command.</returns>
    public static UpdateCommand Insert(string indexName, Tuple key, Tuple value)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = false,
          TableName = indexName,
          Value = value
        };
    }

    /// <summary>
    /// Creates remove command.
    /// </summary>
    /// <param name="indexName">Name of the primary index.</param>
    /// <param name="key">The key.</param>
    /// <returns>Remove command.</returns>
    public static UpdateCommand Remove(string indexName, Tuple key)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = true,
          TableName = indexName
        };
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected IndexUpdateCommand()
    {
    }
  }
}