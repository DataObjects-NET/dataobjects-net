// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Commands;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Indexing
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
    /// <param name="tableName">Name of the table.</param>
    /// <param name="key">The key.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>Update command.</returns>
    public static UpdateCommand Update(string tableName, Tuple key, Tuple newValue)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = true,
          TableName = tableName,
          Value = newValue
        };
    }

    /// <summary>
    /// Creates insert command.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Insert command.</returns>
    public static UpdateCommand Insert(string tableName, Tuple key, Tuple value)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = false,
          TableName = tableName,
          Value = value
        };
    }

    /// <summary>
    /// Creates remove command.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="key">The key.</param>
    /// <returns>Remove command.</returns>
    public static UpdateCommand Remove(string tableName, Tuple key)
    {
      return new IndexUpdateCommand
        {
          Key = key,
          KeyMustExist = true,
          TableName = tableName
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